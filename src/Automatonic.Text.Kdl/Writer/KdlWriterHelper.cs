using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace Automatonic.Text.Kdl
{
    internal static partial class KdlWriterHelper
    {
        public static void WriteIndentation(Span<byte> buffer, int indent, byte indentByte)
        {
            Debug.Assert(buffer.Length >= indent);

            // Based on perf tests, the break-even point where vectorized Fill is faster
            // than explicitly writing the space in a loop is 8.
            if (indent < 8)
            {
                int i = 0;
                while (i + 1 < indent)
                {
                    buffer[i++] = indentByte;
                    buffer[i++] = indentByte;
                }

                if (i < indent)
                {
                    buffer[i] = indentByte;
                }
            }
            else
            {
                buffer[..indent].Fill(indentByte);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNewLine(string value)
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

            if (
                value
                is not KdlConstants.NewLineLineFeed
                    and not KdlConstants.NewLineCarriageReturnLineFeed
            )
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_NewLine(nameof(value));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateIndentCharacter(char value)
        {
            if (
                value
                is not KdlConstants.DefaultIndentCharacter
                    and not KdlConstants.TabIndentCharacter
            )
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_IndentCharacter(nameof(value));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateIndentSize(int value)
        {
            if (value is < KdlConstants.MinimumIndentSize or > KdlConstants.MaximumIndentSize)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_IndentSize(
                    nameof(value),
                    KdlConstants.MinimumIndentSize,
                    KdlConstants.MaximumIndentSize
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateProperty(ReadOnlySpan<byte> propertyName)
        {
            if (propertyName.Length > KdlConstants.MaxUnescapedTokenSize)
            {
                ThrowHelper.ThrowArgumentException_PropertyNameTooLarge(propertyName.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateValue(ReadOnlySpan<byte> value)
        {
            if (value.Length > KdlConstants.MaxUnescapedTokenSize)
            {
                ThrowHelper.ThrowArgumentException_ValueTooLarge(value.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateDouble(double value)
        {
            if (!KdlHelpers.IsFinite(value))
            {
                ThrowHelper.ThrowArgumentException_ValueNotSupported();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateSingle(float value)
        {
            if (!KdlHelpers.IsFinite(value))
            {
                ThrowHelper.ThrowArgumentException_ValueNotSupported();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateProperty(ReadOnlySpan<char> propertyName)
        {
            if (propertyName.Length > KdlConstants.MaxCharacterTokenSize)
            {
                ThrowHelper.ThrowArgumentException_PropertyNameTooLarge(propertyName.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateValue(ReadOnlySpan<char> value)
        {
            if (value.Length > KdlConstants.MaxCharacterTokenSize)
            {
                ThrowHelper.ThrowArgumentException_ValueTooLarge(value.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePropertyAndValue(
            ReadOnlySpan<char> propertyName,
            ReadOnlySpan<byte> value
        )
        {
            if (
                propertyName.Length > KdlConstants.MaxCharacterTokenSize
                || value.Length > KdlConstants.MaxUnescapedTokenSize
            )
            {
                ThrowHelper.ThrowArgumentException(propertyName, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePropertyAndValue(
            ReadOnlySpan<byte> propertyName,
            ReadOnlySpan<char> value
        )
        {
            if (
                propertyName.Length > KdlConstants.MaxUnescapedTokenSize
                || value.Length > KdlConstants.MaxCharacterTokenSize
            )
            {
                ThrowHelper.ThrowArgumentException(propertyName, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePropertyAndValue(
            ReadOnlySpan<byte> propertyName,
            ReadOnlySpan<byte> value
        )
        {
            if (
                propertyName.Length > KdlConstants.MaxUnescapedTokenSize
                || value.Length > KdlConstants.MaxUnescapedTokenSize
            )
            {
                ThrowHelper.ThrowArgumentException(propertyName, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePropertyAndValue(
            ReadOnlySpan<char> propertyName,
            ReadOnlySpan<char> value
        )
        {
            if (
                propertyName.Length > KdlConstants.MaxCharacterTokenSize
                || value.Length > KdlConstants.MaxCharacterTokenSize
            )
            {
                ThrowHelper.ThrowArgumentException(propertyName, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePropertyNameLength(ReadOnlySpan<char> propertyName)
        {
            if (propertyName.Length > KdlConstants.MaxCharacterTokenSize)
            {
                ThrowHelper.ThrowPropertyNameTooLargeArgumentException(propertyName.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidatePropertyNameLength(ReadOnlySpan<byte> propertyName)
        {
            if (propertyName.Length > KdlConstants.MaxUnescapedTokenSize)
            {
                ThrowHelper.ThrowPropertyNameTooLargeArgumentException(propertyName.Length);
            }
        }

        internal static void ValidateNumber(ReadOnlySpan<byte> utf8FormattedNumber)
        {
            // This is a simplified version of the number reader from KdlReader.TryGetNumber,
            // because it doesn't need to deal with "NeedsMoreData", or remembering the format.
            //
            // The Debug.Asserts in this method should change to validated ArgumentExceptions if/when
            // writing a formatted number becomes public API.
            Debug.Assert(!utf8FormattedNumber.IsEmpty);

            int i = 0;

            if (utf8FormattedNumber[i] == '-')
            {
                i++;

                if (utf8FormattedNumber.Length <= i)
                {
                    throw new ArgumentException(
                        SR.RequiredDigitNotFoundEndOfData,
                        nameof(utf8FormattedNumber)
                    );
                }
            }

            if (utf8FormattedNumber[i] == '0')
            {
                i++;
            }
            else
            {
                while (i < utf8FormattedNumber.Length && KdlHelpers.IsDigit(utf8FormattedNumber[i]))
                {
                    i++;
                }
            }

            if (i == utf8FormattedNumber.Length)
            {
                return;
            }

            // The non digit character inside the number
            byte val = utf8FormattedNumber[i];

            if (val == '.')
            {
                i++;

                if (utf8FormattedNumber.Length <= i)
                {
                    throw new ArgumentException(
                        SR.RequiredDigitNotFoundEndOfData,
                        nameof(utf8FormattedNumber)
                    );
                }

                while (i < utf8FormattedNumber.Length && KdlHelpers.IsDigit(utf8FormattedNumber[i]))
                {
                    i++;
                }

                if (i == utf8FormattedNumber.Length)
                {
                    return;
                }

                Debug.Assert(i < utf8FormattedNumber.Length);
                val = utf8FormattedNumber[i];
            }

            if (val is (byte)'e' or (byte)'E')
            {
                i++;

                if (utf8FormattedNumber.Length <= i)
                {
                    throw new ArgumentException(
                        SR.RequiredDigitNotFoundEndOfData,
                        nameof(utf8FormattedNumber)
                    );
                }

                val = utf8FormattedNumber[i];

                if (val is (byte)'+' or (byte)'-')
                {
                    i++;
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        SR.ExpectedEndOfDigitNotFound,
                        ThrowHelper.GetPrintableString(val)
                    ),
                    nameof(utf8FormattedNumber)
                );
            }

            if (utf8FormattedNumber.Length <= i)
            {
                throw new ArgumentException(
                    SR.RequiredDigitNotFoundEndOfData,
                    nameof(utf8FormattedNumber)
                );
            }

            while (i < utf8FormattedNumber.Length && KdlHelpers.IsDigit(utf8FormattedNumber[i]))
            {
                i++;
            }

            if (i != utf8FormattedNumber.Length)
            {
                throw new ArgumentException(
                    string.Format(
                        SR.ExpectedEndOfDigitNotFound,
                        ThrowHelper.GetPrintableString(utf8FormattedNumber[i])
                    ),
                    nameof(utf8FormattedNumber)
                );
            }
        }


        public static unsafe bool IsValidUtf8String(ReadOnlySpan<byte> bytes)
        {
            return Utf8.IsValid(bytes);
        }

        internal static unsafe OperationStatus ToUtf8(
            ReadOnlySpan<char> source,
            Span<byte> destination,
            out int written
        )
        {
#if NET
            OperationStatus status = Utf8.FromUtf16(
                source,
                destination,
                out int charsRead,
                out written,
                replaceInvalidSequences: false,
                isFinalBlock: true
            );
            Debug.Assert(
                status
                    is OperationStatus.Done
                        or OperationStatus.DestinationTooSmall
                        or OperationStatus.InvalidData
            );
            Debug.Assert(charsRead == source.Length || status is not OperationStatus.Done);
            return status;
#else
            written = 0;
            try
            {
                if (!source.IsEmpty)
                {
                    fixed (char* charPtr = source)
                    fixed (byte* destPtr = destination)
                    {
                        written = s_utf8Encoding.GetBytes(
                            charPtr,
                            source.Length,
                            destPtr,
                            destination.Length
                        );
                    }
                }

                return OperationStatus.Done;
            }
            catch (EncoderFallbackException)
            {
                return OperationStatus.InvalidData;
            }
            catch (ArgumentException)
            {
                return OperationStatus.DestinationTooSmall;
            }
#endif
        }
    }
}
