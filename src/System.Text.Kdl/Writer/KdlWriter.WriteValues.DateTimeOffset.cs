using System.Buffers;
using System.Diagnostics;


namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the <see cref="DateTimeOffset"/> value (as a KDL string) as an element of a KDL array.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="DateTimeOffset"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000-07:00.
        /// </remarks>
        public void WriteStringValue(DateTimeOffset value)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            if (_options.Indented)
            {
                WriteStringValueIndented(value);
            }
            else
            {
                WriteStringValueMinimized(value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        private void WriteStringValueMinimized(DateTimeOffset value)
        {
            int maxRequired = KdlConstants.MaximumFormatDateTimeOffsetLength + 3; // 2 quotes, and optionally, 1 list separator

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            output[BytesPending++] = KdlConstants.Quote;

            KdlWriterHelper.WriteDateTimeOffsetTrimmed(output[BytesPending..], value, out int bytesWritten);
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteStringValueIndented(DateTimeOffset value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            // 2 quotes, and optionally, 1 list separator and 1-2 bytes for new line
            int maxRequired = indent + KdlConstants.MaximumFormatDateTimeOffsetLength + 3 + _newLineLength;

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            if (_tokenType != KdlTokenType.PropertyName)
            {
                if (_tokenType != KdlTokenType.None)
                {
                    WriteNewLine(output);
                }
                WriteIndentation(output[BytesPending..], indent);
                BytesPending += indent;
            }

            output[BytesPending++] = KdlConstants.Quote;

            KdlWriterHelper.WriteDateTimeOffsetTrimmed(output[BytesPending..], value, out int bytesWritten);
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }
    }
}
