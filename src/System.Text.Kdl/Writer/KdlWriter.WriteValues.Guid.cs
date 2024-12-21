using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the <see cref="Guid"/> value (as a KDL string) as an element of a KDL array.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="Guid"/> using the default <see cref="StandardFormat"/> (that is, 'D'), as the form: nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn.
        /// </remarks>
        public void WriteStringValue(Guid value)
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

        private void WriteStringValueMinimized(Guid value)
        {
            int maxRequired = KdlConstants.MaximumFormatGuidLength + 3; // 2 quotes, and optionally, 1 list separator

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

            bool result = Utf8Formatter.TryFormat(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteStringValueIndented(Guid value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            // 2 quotes, and optionally, 1 list separator and 1-2 bytes for new line
            int maxRequired = indent + KdlConstants.MaximumFormatGuidLength + 3 + _newLineLength;

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

            bool result = Utf8Formatter.TryFormat(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }
    }
}
