using System.Buffers;
using System.Diagnostics;

namespace Automatonic.Text.Kdl.RandomAccess
{
    public sealed partial class KdlReadOnlyDocument
    {
        internal bool TryGetNamedPropertyValue(
            int index,
            ReadOnlySpan<char> propertyName,
            out KdlReadOnlyElement value
        )
        {
            CheckNotDisposed();

            DbRow row = _parsedData.Get(index);

            CheckExpectedType(KdlTokenType.StartChildrenBlock, row.TokenType);

            // Only one row means it was EndObject.
            if (row.NumberOfRows == 1)
            {
                value = default;
                return false;
            }

            int maxBytes = KdlReaderHelper.s_utf8Encoding.GetMaxByteCount(propertyName.Length);
            int startIndex = index + DbRow.Size;
            int endIndex = checked((row.NumberOfRows * DbRow.Size) + index);

            if (maxBytes < KdlConstants.StackallocByteThreshold)
            {
                Span<byte> utf8Name = stackalloc byte[KdlConstants.StackallocByteThreshold];
                int len = KdlReaderHelper.GetUtf8FromText(propertyName, utf8Name);
                utf8Name = utf8Name[..len];

                return TryGetNamedPropertyValue(startIndex, endIndex, utf8Name, out value);
            }

            // Unescaping the property name will make the string shorter (or the same)
            // So the first viable candidate is one whose length in bytes matches, or
            // exceeds, our length in chars.
            //
            // The maximal escaping seems to be 6 -> 1 ("\u0030" => "0"), but just transcode
            // and switch once one viable long property is found.

            int minBytes = propertyName.Length;
            // Move to the row before the EndObject
            int candidateIndex = endIndex - DbRow.Size;

            while (candidateIndex > index)
            {
                int passedIndex = candidateIndex;

                row = _parsedData.Get(candidateIndex);
                Debug.Assert(row.TokenType != KdlTokenType.PropertyName);

                // Move before the value
                if (row.IsSimpleValue)
                {
                    candidateIndex -= DbRow.Size;
                }
                else
                {
                    Debug.Assert(row.NumberOfRows > 0);
                    candidateIndex -= DbRow.Size * (row.NumberOfRows + 1);
                }

                row = _parsedData.Get(candidateIndex);
                Debug.Assert(row.TokenType == KdlTokenType.PropertyName);

                if (row.SizeOrLength >= minBytes)
                {
                    byte[] tmpUtf8 = ArrayPool<byte>.Shared.Rent(maxBytes);
                    Span<byte> utf8Name = default;

                    try
                    {
                        int len = KdlReaderHelper.GetUtf8FromText(propertyName, tmpUtf8);
                        utf8Name = tmpUtf8.AsSpan(0, len);

                        return TryGetNamedPropertyValue(
                            startIndex,
                            passedIndex + DbRow.Size,
                            utf8Name,
                            out value
                        );
                    }
                    finally
                    {
                        // While property names aren't usually a secret, they also usually
                        // aren't long enough to end up in the rented buffer transcode path.
                        //
                        // On the basis that this is user data, go ahead and clear it.
                        utf8Name.Clear();
                        ArrayPool<byte>.Shared.Return(tmpUtf8);
                    }
                }

                // Move to the previous value
                candidateIndex -= DbRow.Size;
            }

            // None of the property names were within the range that the UTF-8 encoding would have been.
            value = default;
            return false;
        }

        internal bool TryGetNamedPropertyValue(
            int index,
            ReadOnlySpan<byte> propertyName,
            out KdlReadOnlyElement value
        )
        {
            CheckNotDisposed();

            DbRow row = _parsedData.Get(index);

            CheckExpectedType(KdlTokenType.StartChildrenBlock, row.TokenType);

            // Only one row means it was EndObject.
            if (row.NumberOfRows == 1)
            {
                value = default;
                return false;
            }

            int endIndex = checked((row.NumberOfRows * DbRow.Size) + index);

            return TryGetNamedPropertyValue(index + DbRow.Size, endIndex, propertyName, out value);
        }

        private bool TryGetNamedPropertyValue(
            int startIndex,
            int endIndex,
            ReadOnlySpan<byte> propertyName,
            out KdlReadOnlyElement value
        )
        {
            ReadOnlySpan<byte> documentSpan = _utf8Kdl.Span;
            Span<byte> utf8UnescapedStack = stackalloc byte[KdlConstants.StackallocByteThreshold];

            // Move to the row before the EndObject
            int index = endIndex - DbRow.Size;

            while (index > startIndex)
            {
                DbRow row = _parsedData.Get(index);
                Debug.Assert(row.TokenType != KdlTokenType.PropertyName);

                // Move before the value
                if (row.IsSimpleValue)
                {
                    index -= DbRow.Size;
                }
                else
                {
                    Debug.Assert(row.NumberOfRows > 0);
                    index -= DbRow.Size * (row.NumberOfRows + 1);
                }

                row = _parsedData.Get(index);
                Debug.Assert(row.TokenType == KdlTokenType.PropertyName);

                ReadOnlySpan<byte> currentPropertyName = documentSpan.Slice(
                    row.Location,
                    row.SizeOrLength
                );

                if (row.HasComplexChildren)
                {
                    // An escaped property name will be longer than an unescaped candidate, so only unescape
                    // when the lengths are compatible.
                    if (currentPropertyName.Length > propertyName.Length)
                    {
                        int idx = currentPropertyName.IndexOf(KdlConstants.BackSlash);
                        Debug.Assert(idx >= 0);

                        // If everything up to where the property name has a backslash matches, keep going.
                        if (
                            propertyName.Length > idx
                            && currentPropertyName[..idx].SequenceEqual(propertyName[..idx])
                        )
                        {
                            int remaining = currentPropertyName.Length - idx;
                            int written = 0;
                            byte[]? rented = null;

                            try
                            {
                                Span<byte> utf8Unescaped =
                                    remaining <= utf8UnescapedStack.Length
                                        ? utf8UnescapedStack
                                        : (rented = ArrayPool<byte>.Shared.Rent(remaining));

                                // Only unescape the part we haven't processed.
                                KdlReaderHelper.Unescape(
                                    currentPropertyName[idx..],
                                    utf8Unescaped,
                                    0,
                                    out written
                                );

                                // If the unescaped remainder matches the input remainder, it's a match.
                                if (utf8Unescaped[..written].SequenceEqual(propertyName[idx..]))
                                {
                                    // If the property name is a match, the answer is the next element.
                                    value = new KdlReadOnlyElement(this, index + DbRow.Size);
                                    return true;
                                }
                            }
                            finally
                            {
                                if (rented != null)
                                {
                                    rented.AsSpan(0, written).Clear();
                                    ArrayPool<byte>.Shared.Return(rented);
                                }
                            }
                        }
                    }
                }
                else if (currentPropertyName.SequenceEqual(propertyName))
                {
                    // If the property name is a match, the answer is the next element.
                    value = new KdlReadOnlyElement(this, index + DbRow.Size);
                    return true;
                }

                // Move to the previous value
                index -= DbRow.Size;
            }

            value = default;
            return false;
        }
    }
}
