using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the <see cref="float"/> value (as a KDL number) as an element of a KDL array.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="float"/> using the default <see cref="StandardFormat"/> on .NET Core 3 or higher
        /// and 'G9' on any other framework.
        /// </remarks>
        public void WriteNumberValue(float value)
        {
            KdlWriterHelper.ValidateSingle(value);

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

        private void WriteNumberValueMinimized(float value)
        {
            int maxRequired = KdlConstants.MaximumFormatSingleLength + 1; // Optionally, 1 list separator

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            bool result = TryFormatSingle(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        private void WriteNumberValueIndented(float value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            int maxRequired = indent + KdlConstants.MaximumFormatSingleLength + 1 + _newLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

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

            bool result = TryFormatSingle(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        private static bool TryFormatSingle(float value, Span<byte> destination, out int bytesWritten)
        {
            // Frameworks that are not .NET Core 3.0 or higher do not produce roundtrippable strings by
            // default. Further, the Utf8Formatter on older frameworks does not support taking a precision
            // specifier for 'G' nor does it represent other formats such as 'R'. As such, we duplicate
            // the .NET Core 3.0 logic of forwarding to the UTF16 formatter and transcoding it back to UTF8,
            // with some additional changes to remove dependencies on Span APIs which don't exist downlevel.

#if NET
            return Utf8Formatter.TryFormat(value, destination, out bytesWritten);
#else
            string utf16Text = value.ToString(KdlConstants.SingleFormatString, CultureInfo.InvariantCulture);

            // Copy the value to the destination, if it's large enough.

            if (utf16Text.Length > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(utf16Text);

                if (bytes.Length > destination.Length)
                {
                    bytesWritten = 0;
                    return false;
                }

                bytes.CopyTo(destination);
                bytesWritten = bytes.Length;

                return true;
            }
            catch
            {
                bytesWritten = 0;
                return false;
            }
#endif
        }

        internal void WriteNumberValueAsString(float value)
        {
            Span<byte> utf8Number = stackalloc byte[KdlConstants.MaximumFormatSingleLength];
            bool result = TryFormatSingle(value, utf8Number, out int bytesWritten);
            Debug.Assert(result);
            WriteNumberValueAsStringUnescaped(utf8Number[..bytesWritten]);
        }

        internal void WriteFloatingPointConstant(float value)
        {
            if (float.IsNaN(value))
            {
                WriteNumberValueAsStringUnescaped(KdlConstants.NaNValue);
            }
            else if (float.IsPositiveInfinity(value))
            {
                WriteNumberValueAsStringUnescaped(KdlConstants.PositiveInfinityValue);
            }
            else if (float.IsNegativeInfinity(value))
            {
                WriteNumberValueAsStringUnescaped(KdlConstants.NegativeInfinityValue);
            }
            else
            {
                WriteNumberValue(value);
            }
        }
    }
}
