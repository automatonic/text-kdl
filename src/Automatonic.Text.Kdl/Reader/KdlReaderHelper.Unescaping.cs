using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Unicode;

namespace Automatonic.Text.Kdl
{
    internal static partial class KdlReaderHelper
    {
        public static bool TryGetUnescapedBase64Bytes(
            ReadOnlySpan<byte> utf8Source,
            [NotNullWhen(true)] out byte[]? bytes
        )
        {
            byte[]? unescapedArray = null;

            Span<byte> utf8Unescaped =
                utf8Source.Length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (unescapedArray = ArrayPool<byte>.Shared.Rent(utf8Source.Length));

            Unescape(utf8Source, utf8Unescaped, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped[..written];
            Debug.Assert(!utf8Unescaped.IsEmpty);

            bool result = TryDecodeBase64InPlace(utf8Unescaped, out bytes!);

            if (unescapedArray != null)
            {
                utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray);
            }
            return result;
        }

        // Reject any invalid UTF-8 data rather than silently replacing.
        public static readonly UTF8Encoding s_utf8Encoding = new(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true
        );

        // TODO: Similar to escaping, replace the unescaping logic with publicly shipping APIs from https://github.com/dotnet/runtime/issues/27919
        public static string GetUnescapedString(ReadOnlySpan<byte> utf8Source)
        {
            // The escaped name is always >= than the unescaped, so it is safe to use escaped name for the buffer length.
            int length = utf8Source.Length;
            byte[]? pooledName = null;

            Span<byte> utf8Unescaped =
                length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (pooledName = ArrayPool<byte>.Shared.Rent(length));

            Unescape(utf8Source, utf8Unescaped, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped[..written];
            Debug.Assert(!utf8Unescaped.IsEmpty);

            string utf8String = TranscodeHelper(utf8Unescaped);

            if (pooledName != null)
            {
                utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(pooledName);
            }

            return utf8String;
        }

        public static ReadOnlySpan<byte> GetUnescapedSpan(ReadOnlySpan<byte> utf8Source)
        {
            // The escaped name is always >= than the unescaped, so it is safe to use escaped name for the buffer length.
            int length = utf8Source.Length;
            byte[]? pooledName = null;

            Span<byte> utf8Unescaped =
                length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (pooledName = ArrayPool<byte>.Shared.Rent(length));

            Unescape(utf8Source, utf8Unescaped, out int written);
            Debug.Assert(written > 0);

            ReadOnlySpan<byte> propertyName = utf8Unescaped[..written].ToArray();
            Debug.Assert(!propertyName.IsEmpty);

            if (pooledName != null)
            {
                new Span<byte>(pooledName, 0, written).Clear();
                ArrayPool<byte>.Shared.Return(pooledName);
            }

            return propertyName;
        }

        public static bool UnescapeAndCompare(
            ReadOnlySpan<byte> utf8Source,
            ReadOnlySpan<byte> other
        )
        {
            Debug.Assert(
                utf8Source.Length >= other.Length
                    && utf8Source.Length / KdlConstants.MaxExpansionFactorWhileEscaping
                        <= other.Length
            );

            byte[]? unescapedArray = null;

            Span<byte> utf8Unescaped =
                utf8Source.Length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (unescapedArray = ArrayPool<byte>.Shared.Rent(utf8Source.Length));

            Unescape(utf8Source, utf8Unescaped, 0, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped[..written];
            Debug.Assert(!utf8Unescaped.IsEmpty);

            bool result = other.SequenceEqual(utf8Unescaped);

            if (unescapedArray != null)
            {
                utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray);
            }

            return result;
        }

        public static bool UnescapeAndCompare(
            ReadOnlySequence<byte> utf8Source,
            ReadOnlySpan<byte> other
        )
        {
            Debug.Assert(!utf8Source.IsSingleSegment);
            Debug.Assert(
                utf8Source.Length >= other.Length
                    && utf8Source.Length / KdlConstants.MaxExpansionFactorWhileEscaping
                        <= other.Length
            );

            byte[]? escapedArray = null;
            byte[]? unescapedArray = null;

            int length = checked((int)utf8Source.Length);

            Span<byte> utf8Unescaped =
                length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (unescapedArray = ArrayPool<byte>.Shared.Rent(length));

            Span<byte> utf8Escaped =
                length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (escapedArray = ArrayPool<byte>.Shared.Rent(length));

            utf8Source.CopyTo(utf8Escaped);
            utf8Escaped = utf8Escaped[..length];

            Unescape(utf8Escaped, utf8Unescaped, 0, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped[..written];
            Debug.Assert(!utf8Unescaped.IsEmpty);

            bool result = other.SequenceEqual(utf8Unescaped);

            if (unescapedArray != null)
            {
                Debug.Assert(escapedArray != null);
                utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray);
                utf8Escaped.Clear();
                ArrayPool<byte>.Shared.Return(escapedArray);
            }

            return result;
        }

        public static bool UnescapeAndCompareBothInputs(
            ReadOnlySpan<byte> utf8Source1,
            ReadOnlySpan<byte> utf8Source2
        )
        {
            int index1 = utf8Source1.IndexOf(KdlConstants.BackSlash);
            int index2 = utf8Source2.IndexOf(KdlConstants.BackSlash);

            Debug.Assert(index1 >= 0, "the first parameter is not escaped");
            Debug.Assert(index2 >= 0, "the second parameter is not escaped");

            byte[]? unescapedArray1 = null;
            byte[]? unescapedArray2 = null;

            Span<byte> utf8Unescaped1 =
                utf8Source1.Length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (unescapedArray1 = ArrayPool<byte>.Shared.Rent(utf8Source1.Length));

            Span<byte> utf8Unescaped2 =
                utf8Source2.Length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (unescapedArray2 = ArrayPool<byte>.Shared.Rent(utf8Source2.Length));

            Unescape(utf8Source1, utf8Unescaped1, index1, out int written);
            utf8Unescaped1 = utf8Unescaped1[..written];
            Debug.Assert(!utf8Unescaped1.IsEmpty);

            Unescape(utf8Source2, utf8Unescaped2, index2, out written);
            utf8Unescaped2 = utf8Unescaped2[..written];
            Debug.Assert(!utf8Unescaped2.IsEmpty);

            bool result = utf8Unescaped1.SequenceEqual(utf8Unescaped2);

            if (unescapedArray1 != null)
            {
                utf8Unescaped1.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray1);
            }

            if (unescapedArray2 != null)
            {
                utf8Unescaped2.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray2);
            }

            return result;
        }

        public static bool TryDecodeBase64InPlace(
            Span<byte> utf8Unescaped,
            [NotNullWhen(true)] out byte[]? bytes
        )
        {
            OperationStatus status = Base64.DecodeFromUtf8InPlace(
                utf8Unescaped,
                out int bytesWritten
            );
            if (status != OperationStatus.Done)
            {
                bytes = null;
                return false;
            }
            bytes = utf8Unescaped[..bytesWritten].ToArray();
            return true;
        }

        public static bool TryDecodeBase64(
            ReadOnlySpan<byte> utf8Unescaped,
            [NotNullWhen(true)] out byte[]? bytes
        )
        {
            byte[]? pooledArray = null;

            Span<byte> byteSpan =
                utf8Unescaped.Length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (pooledArray = ArrayPool<byte>.Shared.Rent(utf8Unescaped.Length));

            OperationStatus status = Base64.DecodeFromUtf8(
                utf8Unescaped,
                byteSpan,
                out int bytesConsumed,
                out int bytesWritten
            );

            if (status != OperationStatus.Done)
            {
                bytes = null;

                if (pooledArray != null)
                {
                    byteSpan.Clear();
                    ArrayPool<byte>.Shared.Return(pooledArray);
                }

                return false;
            }
            Debug.Assert(bytesConsumed == utf8Unescaped.Length);

            bytes = byteSpan[..bytesWritten].ToArray();

            if (pooledArray != null)
            {
                byteSpan.Clear();
                ArrayPool<byte>.Shared.Return(pooledArray);
            }

            return true;
        }

        public static string TranscodeHelper(ReadOnlySpan<byte> utf8Unescaped)
        {
            try
            {
                return s_utf8Encoding.GetString(utf8Unescaped);
            }
            catch (DecoderFallbackException ex)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw InvalidOperationException for mismatch token type,
                // and while unescaping, using that exception for failure to decode invalid UTF-8 bytes as well.
                // Therefore, wrapping the DecoderFallbackException around an InvalidOperationException.
                throw ThrowHelper.GetInvalidOperationException_ReadInvalidUTF8(ex);
            }
        }

        public static int TranscodeHelper(ReadOnlySpan<byte> utf8Unescaped, Span<char> destination)
        {
            try
            {
                return s_utf8Encoding.GetChars(utf8Unescaped, destination);
            }
            catch (DecoderFallbackException dfe)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw InvalidOperationException for mismatch token type,
                // and while unescaping, using that exception for failure to decode invalid UTF-8 bytes as well.
                // Therefore, wrapping the DecoderFallbackException around an InvalidOperationException.
                throw ThrowHelper.GetInvalidOperationException_ReadInvalidUTF8(dfe);
            }
            catch (ArgumentException)
            {
                // Destination buffer was too small; clear it up since the encoder might have not.
                destination.Clear();
                throw;
            }
        }

        public static void ValidateUtf8(ReadOnlySpan<byte> utf8Buffer)
        {
            if (!Utf8.IsValid(utf8Buffer))
            {
                throw ThrowHelper.GetInvalidOperationException_ReadInvalidUTF8();
            }
        }

        internal static int GetUtf8ByteCount(ReadOnlySpan<char> text)
        {
            try
            {
                return s_utf8Encoding.GetByteCount(text);
            }
            catch (EncoderFallbackException ex)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw ArgumentException when validating other arguments,
                // using that exception for failure to encode invalid UTF-16 chars as well.
                // Therefore, wrapping the EncoderFallbackException around an ArgumentException.
                throw ThrowHelper.GetArgumentException_ReadInvalidUTF16(ex);
            }
        }

        internal static int GetUtf8FromText(ReadOnlySpan<char> text, Span<byte> dest)
        {
            try
            {
                return s_utf8Encoding.GetBytes(text, dest);
            }
            catch (EncoderFallbackException ex)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw ArgumentException when validating other arguments,
                // using that exception for failure to encode invalid UTF-16 chars as well.
                // Therefore, wrapping the EncoderFallbackException around an ArgumentException.
                throw ThrowHelper.GetArgumentException_ReadInvalidUTF16(ex);
            }
        }

        internal static string GetTextFromUtf8(ReadOnlySpan<byte> utf8Text)
        {
            return s_utf8Encoding.GetString(utf8Text);
        }

        internal static void Unescape(
            ReadOnlySpan<byte> source,
            Span<byte> destination,
            out int written
        )
        {
            Debug.Assert(destination.Length >= source.Length);

            int idx = source.IndexOf(KdlConstants.BackSlash);
            Debug.Assert(idx >= 0);

            bool result = TryUnescape(source, destination, idx, out written);
            Debug.Assert(result);
        }

        internal static void Unescape(
            ReadOnlySpan<byte> source,
            Span<byte> destination,
            int idx,
            out int written
        )
        {
            Debug.Assert(idx >= 0 && idx < source.Length);
            Debug.Assert(source[idx] == KdlConstants.BackSlash);
            Debug.Assert(destination.Length >= source.Length);

            bool result = TryUnescape(source, destination, idx, out written);
            Debug.Assert(result);
        }

        /// <summary>
        /// Used when writing to buffers not guaranteed to fit the unescaped result.
        /// </summary>
        internal static bool TryUnescape(
            ReadOnlySpan<byte> source,
            Span<byte> destination,
            out int written
        )
        {
            int idx = source.IndexOf(KdlConstants.BackSlash);
            Debug.Assert(idx >= 0);

            return TryUnescape(source, destination, idx, out written);
        }

        /// <summary>
        /// Used when writing to buffers not guaranteed to fit the unescaped result.
        /// </summary>
        private static bool TryUnescape(
            ReadOnlySpan<byte> source,
            Span<byte> destination,
            int idx,
            out int written
        )
        {
            Debug.Assert(idx >= 0 && idx < source.Length);
            Debug.Assert(source[idx] == KdlConstants.BackSlash);

            if (!source[..idx].TryCopyTo(destination))
            {
                written = 0;
                goto DestinationTooShort;
            }

            written = idx;

            while (true)
            {
                Debug.Assert(source[idx] == KdlConstants.BackSlash);

                if (written == destination.Length)
                {
                    goto DestinationTooShort;
                }

                switch (source[++idx])
                {
                    case KdlConstants.Quote:
                        destination[written++] = KdlConstants.Quote;
                        break;
                    case (byte)'n':
                        destination[written++] = KdlConstants.LineFeed;
                        break;
                    case (byte)'r':
                        destination[written++] = KdlConstants.CarriageReturn;
                        break;
                    case KdlConstants.BackSlash:
                        destination[written++] = KdlConstants.BackSlash;
                        break;
                    case KdlConstants.Slash:
                        destination[written++] = KdlConstants.Slash;
                        break;
                    case (byte)'t':
                        destination[written++] = KdlConstants.Tab;
                        break;
                    case (byte)'b':
                        destination[written++] = KdlConstants.BackSpace;
                        break;
                    case (byte)'f':
                        destination[written++] = KdlConstants.FormFeed;
                        break;
                    default:
                        Debug.Assert(
                            source[idx] == 'u',
                            "invalid escape sequences must have already been caught by KdlReader.Read()"
                        );

                        // The source is known to be valid KDL, and hence if we see a \u, it is guaranteed to have 4 hex digits following it
                        // Otherwise, the KdlReader would have already thrown an exception.
                        Debug.Assert(source.Length >= idx + 5);

                        bool result = Utf8Parser.TryParse(
                            source.Slice(idx + 1, 4),
                            out int scalar,
                            out int bytesConsumed,
                            'x'
                        );
                        Debug.Assert(result);
                        Debug.Assert(bytesConsumed == 4);
                        idx += 4;

                        if (
                            KdlHelpers.IsInRangeInclusive(
                                (uint)scalar,
                                KdlConstants.HighSurrogateStartValue,
                                KdlConstants.LowSurrogateEndValue
                            )
                        )
                        {
                            // The first hex value cannot be a low surrogate.
                            if (scalar >= KdlConstants.LowSurrogateStartValue)
                            {
                                ThrowHelper.ThrowInvalidOperationException_ReadInvalidUTF16(scalar);
                            }

                            Debug.Assert(
                                KdlHelpers.IsInRangeInclusive(
                                    (uint)scalar,
                                    KdlConstants.HighSurrogateStartValue,
                                    KdlConstants.HighSurrogateEndValue
                                )
                            );

                            // We must have a low surrogate following a high surrogate.
                            if (
                                source.Length < idx + 7
                                || source[idx + 1] != '\\'
                                || source[idx + 2] != 'u'
                            )
                            {
                                ThrowHelper.ThrowInvalidOperationException_ReadIncompleteUTF16();
                            }

                            // The source is known to be valid KDL, and hence if we see a \u, it is guaranteed to have 4 hex digits following it
                            // Otherwise, the KdlReader would have already thrown an exception.
                            result = Utf8Parser.TryParse(
                                source.Slice(idx + 3, 4),
                                out int lowSurrogate,
                                out bytesConsumed,
                                'x'
                            );
                            Debug.Assert(result);
                            Debug.Assert(bytesConsumed == 4);
                            idx += 6;

                            // If the first hex value is a high surrogate, the next one must be a low surrogate.
                            if (
                                !KdlHelpers.IsInRangeInclusive(
                                    (uint)lowSurrogate,
                                    KdlConstants.LowSurrogateStartValue,
                                    KdlConstants.LowSurrogateEndValue
                                )
                            )
                            {
                                ThrowHelper.ThrowInvalidOperationException_ReadInvalidUTF16(
                                    lowSurrogate
                                );
                            }

                            // To find the unicode scalar:
                            // (0x400 * (High surrogate - 0xD800)) + Low surrogate - 0xDC00 + 0x10000
                            scalar =
                                (
                                    KdlConstants.BitShiftBy10
                                    * (scalar - KdlConstants.HighSurrogateStartValue)
                                )
                                + (lowSurrogate - KdlConstants.LowSurrogateStartValue)
                                + KdlConstants.UnicodePlane01StartValue;
                        }

                        var rune = new Rune(scalar);
                        bool success = rune.TryEncodeToUtf8(
                            destination[written..],
                            out int bytesWritten
                        );
                        if (!success)
                        {
                            goto DestinationTooShort;
                        }

                        Debug.Assert(bytesWritten <= 4);
                        written += bytesWritten;
                        break;
                }

                if (++idx == source.Length)
                {
                    goto Success;
                }

                if (source[idx] != KdlConstants.BackSlash)
                {
                    ReadOnlySpan<byte> remaining = source[idx..];
                    int nextUnescapedSegmentLength = remaining.IndexOf(KdlConstants.BackSlash);
                    if (nextUnescapedSegmentLength < 0)
                    {
                        nextUnescapedSegmentLength = remaining.Length;
                    }

                    if ((uint)(written + nextUnescapedSegmentLength) >= (uint)destination.Length)
                    {
                        goto DestinationTooShort;
                    }

                    Debug.Assert(nextUnescapedSegmentLength > 0);
                    switch (nextUnescapedSegmentLength)
                    {
                        case 1:
                            destination[written++] = source[idx++];
                            break;
                        case 2:
                            destination[written++] = source[idx++];
                            destination[written++] = source[idx++];
                            break;
                        case 3:
                            destination[written++] = source[idx++];
                            destination[written++] = source[idx++];
                            destination[written++] = source[idx++];
                            break;
                        default:
                            remaining[..nextUnescapedSegmentLength].CopyTo(destination[written..]);
                            written += nextUnescapedSegmentLength;
                            idx += nextUnescapedSegmentLength;
                            break;
                    }

                    Debug.Assert(idx == source.Length || source[idx] == KdlConstants.BackSlash);

                    if (idx == source.Length)
                    {
                        goto Success;
                    }
                }
            }

            Success:
            return true;

            DestinationTooShort:
            return false;
        }
    }
}
