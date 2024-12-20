using System.Diagnostics;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the KDL literal "null" as an element of a KDL array.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteNullValue()
        {
            WriteLiteralByOptions(KdlConstants.NullValue);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Null;
        }

        /// <summary>
        /// Writes the <see cref="bool"/> value (as a KDL literal "true" or "false") as an element of a KDL array.
        /// </summary>
        /// <param name="value">The value write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteBooleanValue(bool value)
        {
            if (value)
            {
                WriteLiteralByOptions(KdlConstants.TrueValue);
                _tokenType = KdlTokenType.True;
            }
            else
            {
                WriteLiteralByOptions(KdlConstants.FalseValue);
                _tokenType = KdlTokenType.False;
            }

            SetFlagToAddListSeparatorBeforeNextItem();
        }

        private void WriteLiteralByOptions(ReadOnlySpan<byte> utf8Value)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            if (_options.Indented)
            {
                WriteLiteralIndented(utf8Value);
            }
            else
            {
                WriteLiteralMinimized(utf8Value);
            }
        }

        private void WriteLiteralMinimized(ReadOnlySpan<byte> utf8Value)
        {
            Debug.Assert(utf8Value.Length <= 5);

            int maxRequired = utf8Value.Length + 1; // Optionally, 1 list separator

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            utf8Value.CopyTo(output.Slice(BytesPending));
            BytesPending += utf8Value.Length;
        }

        private void WriteLiteralIndented(ReadOnlySpan<byte> utf8Value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);
            Debug.Assert(utf8Value.Length <= 5);

            int maxRequired = indent + utf8Value.Length + 1 + _newLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

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
                WriteIndentation(output.Slice(BytesPending), indent);
                BytesPending += indent;
            }

            utf8Value.CopyTo(output.Slice(BytesPending));
            BytesPending += utf8Value.Length;
        }
    }
}
