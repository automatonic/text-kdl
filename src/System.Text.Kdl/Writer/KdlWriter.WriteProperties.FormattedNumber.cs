using System.Buffers;
using System.Diagnostics;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the property name and value (as a KDL number) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="utf8FormattedNumber">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="utf8FormattedNumber"/> does not represent a valid KDL number.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="long"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        internal void WriteNumber(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> utf8FormattedNumber)
        {
            KdlWriterHelper.ValidateProperty(propertyName);
            KdlWriterHelper.ValidateValue(utf8FormattedNumber);
            KdlWriterHelper.ValidateNumber(utf8FormattedNumber);

            WriteNumberEscape(propertyName, utf8FormattedNumber);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        /// <summary>
        /// Writes the property name and value (as a KDL number) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write..</param>
        /// <param name="utf8FormattedNumber">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="utf8FormattedNumber"/> does not represent a valid KDL number.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="long"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        internal void WriteNumber(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> utf8FormattedNumber)
        {
            KdlWriterHelper.ValidateProperty(utf8PropertyName);
            KdlWriterHelper.ValidateValue(utf8FormattedNumber);
            KdlWriterHelper.ValidateNumber(utf8FormattedNumber);

            WriteNumberEscape(utf8PropertyName, utf8FormattedNumber);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        internal void WriteNumber(KdlEncodedText propertyName, ReadOnlySpan<byte> utf8FormattedNumber)
        {
            KdlWriterHelper.ValidateValue(utf8FormattedNumber);
            KdlWriterHelper.ValidateNumber(utf8FormattedNumber);

            WriteNumberByOptions(propertyName.EncodedUtf8Bytes, utf8FormattedNumber);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        private void WriteNumberEscape(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> value)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(propertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteNumberEscapeProperty(propertyName, value, propertyIdx);
            }
            else
            {
                WriteNumberByOptions(propertyName, value);
            }
        }

        private void WriteNumberEscape(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(utf8PropertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteNumberEscapeProperty(utf8PropertyName, value, propertyIdx);
            }
            else
            {
                WriteNumberByOptions(utf8PropertyName, value);
            }
        }

        private void WriteNumberEscapeProperty(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> value, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= propertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < propertyName.Length);

            char[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

            Span<char> escapedPropertyName = length <= KdlConstants.StackallocCharThreshold ?
                stackalloc char[KdlConstants.StackallocCharThreshold] :
                (propertyArray = ArrayPool<char>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(propertyName, escapedPropertyName, firstEscapeIndexProp, _options.Encoder, out int written);

            WriteNumberByOptions(escapedPropertyName.Slice(0, written), value);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteNumberEscapeProperty(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < utf8PropertyName.Length);

            byte[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

            Span<byte> escapedPropertyName = length <= KdlConstants.StackallocByteThreshold ?
                stackalloc byte[KdlConstants.StackallocByteThreshold] :
                (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(utf8PropertyName, escapedPropertyName, firstEscapeIndexProp, _options.Encoder, out int written);

            WriteNumberByOptions(escapedPropertyName.Slice(0, written), value);

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteNumberByOptions(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteLiteralIndented(propertyName, value);
            }
            else
            {
                WriteLiteralMinimized(propertyName, value);
            }
        }

        private void WriteNumberByOptions(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteLiteralIndented(utf8PropertyName, value);
            }
            else
            {
                WriteLiteralMinimized(utf8PropertyName, value);
            }
        }
    }
}
