using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the <see cref="decimal"/> value (as a KDL number) as an element of a KDL array.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="decimal"/> using the default <see cref="StandardFormat"/> (that is, 'G').
        /// </remarks>
        public void WriteNumberValue(decimal value)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            if (_options.Indented)
            {
                WriteNumberValueIndented(value);
            }
            else
            {
                WriteNumberValueMinimized(value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        private void WriteNumberValueMinimized(decimal value)
        {
            int maxRequired = KdlConstants.MaximumFormatDecimalLength + 1; // Optionally, 1 list separator

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            bool result = Utf8Formatter.TryFormat(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        private void WriteNumberValueIndented(decimal value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            int maxRequired = indent + KdlConstants.MaximumFormatDecimalLength + 1 + _newLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

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

            bool result = Utf8Formatter.TryFormat(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        internal void WriteNumberValueAsString(decimal value)
        {
            Span<byte> utf8Number = stackalloc byte[KdlConstants.MaximumFormatDecimalLength];
            bool result = Utf8Formatter.TryFormat(value, utf8Number, out int bytesWritten);
            Debug.Assert(result);
            WriteNumberValueAsStringUnescaped(utf8Number[..bytesWritten]);
        }
    }
}
