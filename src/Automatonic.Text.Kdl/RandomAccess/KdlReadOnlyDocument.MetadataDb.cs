using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

// We need to target netstandard2.0, so keep using ref for MemoryMarshal.Write
// CS9191: The 'ref' modifier for argument 2 corresponding to 'in' parameter is equivalent to 'in'. Consider using 'in' instead.
#pragma warning disable CS9191

namespace Automatonic.Text.Kdl.RandomAccess
{
    public sealed partial class KdlReadOnlyDocument
    {
        // The database for the parsed structure of a KDL document.
        //
        // Every token from the document gets a row, which has one of the following forms:
        //
        // Number
        // * First int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for token offset
        // * Second int
        //   * Top bit is set if the number uses scientific notation
        //   * 31 bits for the token length
        // * Third int
        //   * 4 bits KdlTokenType
        //   * 28 bits unassigned / always clear
        //
        // String, PropertyName
        // * First int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for token offset
        // * Second int
        //   * Top bit is set if the string requires unescaping
        //   * 31 bits for the token length
        // * Third int
        //   * 4 bits KdlTokenType
        //   * 28 bits unassigned / always clear
        //
        // Other value types (True, False, Null)
        // * First int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for token offset
        // * Second int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for the token length
        // * Third int
        //   * 4 bits KdlTokenType
        //   * 28 bits unassigned / always clear
        //
        // EndObject / EndArray
        // * First int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for token offset
        // * Second int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for the token length (always 1, effectively unassigned)
        // * Third int
        //   * 4 bits KdlTokenType
        //   * 28 bits for the number of rows until the previous value (never 0)
        //
        // StartObject
        // * First int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for token offset
        // * Second int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for the number of properties in this object
        // * Third int
        //   * 4 bits KdlTokenType
        //   * 28 bits for the number of rows until the next value (never 0)
        //
        // StartArray
        // * First int
        //   * Top bit is unassigned / always clear
        //   * 31 bits for token offset
        // * Second int
        //   * Top bit is set if the array contains other arrays or objects ("complex" types)
        //   * 31 bits for the number of elements in this array
        // * Third int
        //   * 4 bits KdlTokenType
        //   * 28 bits for the number of rows until the next value (never 0)
        private struct MetadataDb : IDisposable
        {
            private const int SizeOrLengthOffset = 4;
            private const int NumberOfRowsOffset = 8;

            internal int Length { get; private set; }
            private byte[] _data;

            private bool _convertToAlloc; // Convert the rented data to an alloc when complete.
            private bool _isLocked; // Is the array the correct fixed size.

            // _isLocked _convertToAlloc truth table:
            // false     false  Standard flow. Size is not known and renting used throughout lifetime.
            // true      false  Used by KdlElement.ParseValue() for primitives and KdlDocument.Clone(). Size is known and no renting.
            // false     true   Used by KdlElement.ParseValue() for arrays and objects. Renting used until size is known.
            // true      true   not valid

            private MetadataDb(byte[] initialDb, bool isLocked, bool convertToAlloc)
            {
                _data = initialDb;
                _isLocked = isLocked;
                _convertToAlloc = convertToAlloc;
                Length = 0;
            }

            internal MetadataDb(byte[] completeDb)
            {
                _data = completeDb;
                _isLocked = true;
                _convertToAlloc = false;
                Length = completeDb.Length;
            }

            internal static MetadataDb CreateRented(int payloadLength, bool convertToAlloc)
            {
                // Assume that a token happens approximately every 12 bytes.
                // int estimatedTokens = payloadLength / 12
                // now acknowledge that the number of bytes we need per token is 12.
                // So that's just the payload length.
                //
                // Add one row worth of data since we need at least one row for a primitive type.
                int initialSize = payloadLength + DbRow.Size;

                // Stick with ArrayPool's rent/return range if it looks feasible.
                // If it's wrong, we'll just grow and copy as we would if the tokens
                // were more frequent anyways.
                const int OneMegabyte = 1024 * 1024;

                if (initialSize is > OneMegabyte and <= (4 * OneMegabyte))
                {
                    initialSize = OneMegabyte;
                }

                byte[] data = ArrayPool<byte>.Shared.Rent(initialSize);
                return new MetadataDb(data, isLocked: false, convertToAlloc);
            }

            internal static MetadataDb CreateLocked(int payloadLength)
            {
                // Add one row worth of data since we need at least one row for a primitive type.
                int size = payloadLength + DbRow.Size;

                byte[] data = new byte[size];
                return new MetadataDb(data, isLocked: true, convertToAlloc: false);
            }

            public void Dispose()
            {
                byte[]? data = Interlocked.Exchange(ref _data, null!);
                if (data == null)
                {
                    return;
                }

                Debug.Assert(!_isLocked, "Dispose called on a locked database");

                // The data in this rented buffer only conveys the positions and
                // lengths of tokens in a document, but no content; so it does not
                // need to be cleared.
                ArrayPool<byte>.Shared.Return(data);
                Length = 0;
            }

            /// <summary>
            /// If using array pools, trim excess if necessary.
            /// If not using array pools, release the temporary array pool and alloc.
            /// </summary>
            internal void CompleteAllocations()
            {
                if (!_isLocked)
                {
                    if (_convertToAlloc)
                    {
                        Debug.Assert(_data != null);
                        byte[] returnBuf = _data;
                        _data = _data.AsSpan(0, Length).ToArray();
                        _isLocked = true;
                        _convertToAlloc = false;

                        // The data in this rented buffer only conveys the positions and
                        // lengths of tokens in a document, but no content; so it does not
                        // need to be cleared.
                        ArrayPool<byte>.Shared.Return(returnBuf);
                    }
                    else
                    {
                        // There's a chance that the size we have is the size we'd get for this
                        // amount of usage (particularly if Enlarge ever got called); and there's
                        // the small copy-cost associated with trimming anyways. "Is half-empty" is
                        // just a rough metric for "is trimming worth it?".
                        if (Length <= _data.Length / 2)
                        {
                            byte[] newRent = ArrayPool<byte>.Shared.Rent(Length);
                            byte[] returnBuf = newRent;

                            if (newRent.Length < _data.Length)
                            {
                                Buffer.BlockCopy(_data, 0, newRent, 0, Length);
                                returnBuf = _data;
                                _data = newRent;
                            }

                            // The data in this rented buffer only conveys the positions and
                            // lengths of tokens in a document, but no content; so it does not
                            // need to be cleared.
                            ArrayPool<byte>.Shared.Return(returnBuf);
                        }
                    }
                }
            }

            internal void Append(KdlTokenType tokenType, int startLocation, int length)
            {
                // StartArray or StartObject should have length -1, otherwise the length should not be -1.
                Debug.Assert(
                    (
                        tokenType == KdlTokenType.StartChildrenBlock
                        || tokenType == KdlTokenType.StartChildrenBlock
                    ) == (length == DbRow.UnknownSize)
                );

                if (Length >= _data.Length - DbRow.Size)
                {
                    Enlarge();
                }

                DbRow row = new DbRow(tokenType, startLocation, length);
                MemoryMarshal.Write(_data.AsSpan(Length), ref row);
                Length += DbRow.Size;
            }

            private void Enlarge()
            {
                Debug.Assert(!_isLocked, "Appending to a locked database");

                byte[] toReturn = _data;

                // Allow the data to grow up to maximum possible capacity (~2G bytes) before encountering overflow.
                // Note: Array.MaxLength exists only on .NET 6 or greater,
                // so for the other versions value is hardcoded
                const int MaxArrayLength = 0x7FFFFFC7;
                Debug.Assert(MaxArrayLength == Array.MaxLength);

                int newCapacity = toReturn.Length * 2;

                // Note that this check works even when newCapacity overflowed thanks to the (uint) cast
                if ((uint)newCapacity > MaxArrayLength)
                {
                    newCapacity = MaxArrayLength;
                }

                // If the maximum capacity has already been reached,
                // then set the new capacity to be larger than what is possible
                // so that ArrayPool.Rent throws an OutOfMemoryException for us.
                if (newCapacity == toReturn.Length)
                {
                    newCapacity = int.MaxValue;
                }

                _data = ArrayPool<byte>.Shared.Rent(newCapacity);
                Buffer.BlockCopy(toReturn, 0, _data, 0, toReturn.Length);

                // The data in this rented buffer only conveys the positions and
                // lengths of tokens in a document, but no content; so it does not
                // need to be cleared.
                ArrayPool<byte>.Shared.Return(toReturn);
            }

            [Conditional("DEBUG")]
            private void AssertValidIndex(int index)
            {
                Debug.Assert(index >= 0);
                Debug.Assert(index <= Length - DbRow.Size, $"index {index} is out of bounds");
                Debug.Assert(
                    index % DbRow.Size == 0,
                    $"index {index} is not at a record start position"
                );
            }

            internal void SetLength(int index, int length)
            {
                AssertValidIndex(index);
                Debug.Assert(length >= 0);
                Span<byte> destination = _data.AsSpan(index + SizeOrLengthOffset);
                MemoryMarshal.Write(destination, ref length);
            }

            internal void SetNumberOfRows(int index, int numberOfRows)
            {
                AssertValidIndex(index);
                Debug.Assert(numberOfRows is >= 1 and <= 0x0FFFFFFF);

                Span<byte> dataPos = _data.AsSpan(index + NumberOfRowsOffset);
                int current = MemoryMarshal.Read<int>(dataPos);

                // Persist the most significant nybble
                int value = (current & unchecked((int)0xF0000000)) | numberOfRows;
                MemoryMarshal.Write(dataPos, ref value);
            }

            internal void SetHasComplexChildren(int index)
            {
                AssertValidIndex(index);

                // The HasComplexChildren bit is the most significant bit of "SizeOrLength"
                Span<byte> dataPos = _data.AsSpan(index + SizeOrLengthOffset);
                int current = MemoryMarshal.Read<int>(dataPos);

                int value = current | unchecked((int)0x80000000);
                MemoryMarshal.Write(dataPos, ref value);
            }

            internal readonly int FindIndexOfFirstUnsetSizeOrLength(KdlTokenType lookupType)
            {
                Debug.Assert(
                    lookupType is KdlTokenType.StartChildrenBlock or KdlTokenType.StartChildrenBlock
                );
                return FindOpenElement(lookupType);
            }

            private readonly int FindOpenElement(KdlTokenType lookupType)
            {
                Span<byte> data = _data.AsSpan(0, Length);

                for (int i = Length - DbRow.Size; i >= 0; i -= DbRow.Size)
                {
                    DbRow row = MemoryMarshal.Read<DbRow>(data[i..]);

                    if (row.IsUnknownSize && row.TokenType == lookupType)
                    {
                        return i;
                    }
                }

                // We should never reach here.
                Debug.Fail($"Unable to find expected {lookupType} token");
                return -1;
            }

            internal DbRow Get(int index)
            {
                AssertValidIndex(index);
                return MemoryMarshal.Read<DbRow>(_data.AsSpan(index));
            }

            internal KdlTokenType GetKdlTokenType(int index)
            {
                AssertValidIndex(index);
                uint union = MemoryMarshal.Read<uint>(_data.AsSpan(index + NumberOfRowsOffset));

                return (KdlTokenType)(union >> 28);
            }

            internal MetadataDb CopySegment(int startIndex, int endIndex)
            {
                Debug.Assert(
                    endIndex > startIndex,
                    $"endIndex={endIndex} was at or before startIndex={startIndex}"
                );

                AssertValidIndex(startIndex);
                Debug.Assert(endIndex <= Length);

                DbRow start = Get(startIndex);
#if DEBUG
                DbRow end = Get(endIndex - DbRow.Size);

                if (start.TokenType == KdlTokenType.StartChildrenBlock)
                {
                    Debug.Assert(
                        end.TokenType == KdlTokenType.EndChildrenBlock,
                        $"StartObject paired with {end.TokenType}"
                    );
                }
                else if (start.TokenType == KdlTokenType.StartChildrenBlock)
                {
                    Debug.Assert(
                        end.TokenType == KdlTokenType.EndChildrenBlock,
                        $"StartArray paired with {end.TokenType}"
                    );
                }
                else
                {
                    Debug.Assert(
                        startIndex + DbRow.Size == endIndex,
                        $"{start.TokenType} should have been one row"
                    );
                }
#endif

                int length = endIndex - startIndex;

                byte[] newDatabase = new byte[length];
                _data.AsSpan(startIndex, length).CopyTo(newDatabase);

                Span<int> newDbInts = MemoryMarshal.Cast<byte, int>(newDatabase.AsSpan());
                int locationOffset = newDbInts[0];

                // Need to nudge one forward to account for the hidden quote on the string.
                if (start.TokenType == KdlTokenType.String)
                {
                    locationOffset--;
                }

                for (
                    int i = (length - DbRow.Size) / sizeof(int);
                    i >= 0;
                    i -= DbRow.Size / sizeof(int)
                )
                {
                    Debug.Assert(newDbInts[i] >= locationOffset);
                    newDbInts[i] -= locationOffset;
                }

                return new MetadataDb(newDatabase);
            }
        }
    }
}
