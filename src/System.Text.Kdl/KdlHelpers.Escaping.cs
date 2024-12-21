using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;

namespace System.Text.Kdl
{
    internal static partial class KdlHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetEscapedPropertyNameSection(ReadOnlySpan<byte> utf8Value, JavaScriptEncoder? encoder)
        {
            int idx = KdlWriterHelper.NeedsEscaping(utf8Value, encoder);

            if (idx != -1)
            {
                return GetEscapedPropertyNameSection(utf8Value, idx, encoder);
            }
            else
            {
                return GetPropertyNameSection(utf8Value);
            }
        }

        public static byte[] EscapeValue(
            ReadOnlySpan<byte> utf8Value,
            int firstEscapeIndexVal,
            JavaScriptEncoder? encoder)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= utf8Value.Length);
            Debug.Assert(firstEscapeIndexVal >= 0 && firstEscapeIndexVal < utf8Value.Length);

            byte[]? valueArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(utf8Value.Length, firstEscapeIndexVal);

            Span<byte> escapedValue = length <= KdlConstants.StackallocByteThreshold ?
                stackalloc byte[KdlConstants.StackallocByteThreshold] :
                (valueArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(utf8Value, escapedValue, firstEscapeIndexVal, encoder, out int written);

            byte[] escapedString = escapedValue[..written].ToArray();

            if (valueArray != null)
            {
                ArrayPool<byte>.Shared.Return(valueArray);
            }

            return escapedString;
        }

        private static byte[] GetEscapedPropertyNameSection(
            ReadOnlySpan<byte> utf8Value,
            int firstEscapeIndexVal,
            JavaScriptEncoder? encoder)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= utf8Value.Length);
            Debug.Assert(firstEscapeIndexVal >= 0 && firstEscapeIndexVal < utf8Value.Length);

            byte[]? valueArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(utf8Value.Length, firstEscapeIndexVal);

            Span<byte> escapedValue = length <= KdlConstants.StackallocByteThreshold ?
                stackalloc byte[KdlConstants.StackallocByteThreshold] :
                (valueArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(utf8Value, escapedValue, firstEscapeIndexVal, encoder, out int written);

            byte[] propertySection = GetPropertyNameSection(escapedValue[..written]);

            if (valueArray != null)
            {
                ArrayPool<byte>.Shared.Return(valueArray);
            }

            return propertySection;
        }

        private static byte[] GetPropertyNameSection(ReadOnlySpan<byte> utf8Value)
        {
            int length = utf8Value.Length;
            byte[] propertySection = new byte[length + 3];

            propertySection[0] = KdlConstants.Quote;
            utf8Value.CopyTo(propertySection.AsSpan(1, length));
            propertySection[++length] = KdlConstants.Quote;
            propertySection[++length] = KdlConstants.KeyValueSeparator;

            return propertySection;
        }
    }
}