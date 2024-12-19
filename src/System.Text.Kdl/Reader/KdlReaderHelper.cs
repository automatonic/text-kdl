using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text.Kdl
{
    internal static partial class KdlReaderHelper
    {
        private const string SpecialCharacters = ". '/\"[]()\t\n\r\f\b\\\u0085\u2028\u2029";
        private static readonly SearchValues<char> s_specialCharacters = SearchValues.Create(SpecialCharacters);

        public static bool ContainsSpecialCharacters(this ReadOnlySpan<char> text) =>
            text.ContainsAny(s_specialCharacters);


        public static (int, int) CountNewLines(ReadOnlySpan<byte> data)
        {
            int lastLineFeedIndex = data.LastIndexOf(KdlConstants.LineFeed);
            int newLines = 0;

            if (lastLineFeedIndex >= 0)
            {
                newLines = 1;
                data = data.Slice(0, lastLineFeedIndex);
                newLines += data.Count(KdlConstants.LineFeed);
            }

            return (newLines, lastLineFeedIndex);
        }

        internal static KdlValueKind ToValueKind(this KdlTokenType tokenType)
        {
            switch (tokenType)
            {
                case KdlTokenType.None:
                    return KdlValueKind.Undefined;
                case KdlTokenType.StartArray:
                    return KdlValueKind.Array;
                case KdlTokenType.StartObject:
                    return KdlValueKind.Object;
                case KdlTokenType.String:
                case KdlTokenType.Number:
                case KdlTokenType.True:
                case KdlTokenType.False:
                case KdlTokenType.Null:
                    // This is the offset between the set of literals within KdlValueType and KdlTokenType
                    // Essentially: KdlTokenType.Null - KdlValueType.Null
                    return (KdlValueKind)((byte)tokenType - 4);
                default:
                    Debug.Fail($"No mapping for token type {tokenType}");
                    return KdlValueKind.Undefined;
            }
        }

        // Returns true if the TokenType is a primitive "value", i.e. String, Number, True, False, and Null
        // Otherwise, return false.
        public static bool IsTokenTypePrimitive(KdlTokenType tokenType) =>
            (tokenType - KdlTokenType.String) <= (KdlTokenType.Null - KdlTokenType.String);

        // A hex digit is valid if it is in the range: [0..9] | [A..F] | [a..f]
        // Otherwise, return false.
        public static bool IsHexDigit(byte nextByte) => HexConverter.IsHexChar(nextByte);

        public static bool TryGetEscapedDateTime(ReadOnlySpan<byte> source, out DateTime value)
        {
            Debug.Assert(source.Length <= KdlConstants.MaximumEscapedDateTimeOffsetParseLength);
            Span<byte> sourceUnescaped = stackalloc byte[KdlConstants.MaximumEscapedDateTimeOffsetParseLength];

            Unescape(source, sourceUnescaped, out int written);
            Debug.Assert(written > 0);

            sourceUnescaped = sourceUnescaped.Slice(0, written);
            Debug.Assert(!sourceUnescaped.IsEmpty);

            if (KdlHelpers.IsValidUnescapedDateTimeOffsetParseLength(sourceUnescaped.Length)
                && KdlHelpers.TryParseAsISO(sourceUnescaped, out DateTime tmp))
            {
                value = tmp;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetEscapedDateTimeOffset(ReadOnlySpan<byte> source, out DateTimeOffset value)
        {
            Debug.Assert(source.Length <= KdlConstants.MaximumEscapedDateTimeOffsetParseLength);
            Span<byte> sourceUnescaped = stackalloc byte[KdlConstants.MaximumEscapedDateTimeOffsetParseLength];

            Unescape(source, sourceUnescaped, out int written);
            Debug.Assert(written > 0);

            sourceUnescaped = sourceUnescaped.Slice(0, written);
            Debug.Assert(!sourceUnescaped.IsEmpty);

            if (KdlHelpers.IsValidUnescapedDateTimeOffsetParseLength(sourceUnescaped.Length)
                && KdlHelpers.TryParseAsISO(sourceUnescaped, out DateTimeOffset tmp))
            {
                value = tmp;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetEscapedGuid(ReadOnlySpan<byte> source, out Guid value)
        {
            Debug.Assert(source.Length <= KdlConstants.MaximumEscapedGuidLength);

            Span<byte> utf8Unescaped = stackalloc byte[KdlConstants.MaximumEscapedGuidLength];
            Unescape(source, utf8Unescaped, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped.Slice(0, written);
            Debug.Assert(!utf8Unescaped.IsEmpty);

            if (utf8Unescaped.Length == KdlConstants.MaximumFormatGuidLength
                && Utf8Parser.TryParse(utf8Unescaped, out Guid tmp, out _, 'D'))
            {
                value = tmp;
                return true;
            }

            value = default;
            return false;
        }

#if NET
        public static bool TryGetFloatingPointConstant(ReadOnlySpan<byte> span, out Half value)
        {
            if (span.Length == 3)
            {
                if (span.SequenceEqual(KdlConstants.NaNValue))
                {
                    value = Half.NaN;
                    return true;
                }
            }
            else if (span.Length == 8)
            {
                if (span.SequenceEqual(KdlConstants.PositiveInfinityValue))
                {
                    value = Half.PositiveInfinity;
                    return true;
                }
            }
            else if (span.Length == 9)
            {
                if (span.SequenceEqual(KdlConstants.NegativeInfinityValue))
                {
                    value = Half.NegativeInfinity;
                    return true;
                }
            }

            value = default;
            return false;
        }
#endif

        public static bool TryGetFloatingPointConstant(ReadOnlySpan<byte> span, out float value)
        {
            if (span.Length == 3)
            {
                if (span.SequenceEqual(KdlConstants.NaNValue))
                {
                    value = float.NaN;
                    return true;
                }
            }
            else if (span.Length == 8)
            {
                if (span.SequenceEqual(KdlConstants.PositiveInfinityValue))
                {
                    value = float.PositiveInfinity;
                    return true;
                }
            }
            else if (span.Length == 9)
            {
                if (span.SequenceEqual(KdlConstants.NegativeInfinityValue))
                {
                    value = float.NegativeInfinity;
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public static bool TryGetFloatingPointConstant(ReadOnlySpan<byte> span, out double value)
        {
            if (span.Length == 3)
            {
                if (span.SequenceEqual(KdlConstants.NaNValue))
                {
                    value = double.NaN;
                    return true;
                }
            }
            else if (span.Length == 8)
            {
                if (span.SequenceEqual(KdlConstants.PositiveInfinityValue))
                {
                    value = double.PositiveInfinity;
                    return true;
                }
            }
            else if (span.Length == 9)
            {
                if (span.SequenceEqual(KdlConstants.NegativeInfinityValue))
                {
                    value = double.NegativeInfinity;
                    return true;
                }
            }

            value = 0;
            return false;
        }
    }
}