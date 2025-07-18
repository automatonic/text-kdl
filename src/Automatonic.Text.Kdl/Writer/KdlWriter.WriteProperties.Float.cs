using System.Buffers;
using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and <see cref="float"/> value (as a KDL number) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write..</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="float"/> using the default <see cref="StandardFormat"/> (that is, 'G').
        /// </remarks>
        public void WriteNumber(KdlEncodedText propertyName, float value)
        {
            ReadOnlySpan<byte> utf8PropertyName = propertyName.EncodedUtf8Bytes;
            Debug.Assert(utf8PropertyName.Length <= KdlConstants.MaxUnescapedTokenSize);

            KdlWriterHelper.ValidateSingle(value);

            WriteNumberByOptions(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        /// <summary>
        /// Writes the property name and <see cref="float"/> value (as a KDL number) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write..</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="float"/> using the default <see cref="StandardFormat"/> (that is, 'G').
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(string propertyName, float value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteNumber(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and <see cref="float"/> value (as a KDL number) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write..</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="float"/> using the default <see cref="StandardFormat"/> (that is, 'G').
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(ReadOnlySpan<char> propertyName, float value)
        {
            KdlWriterHelper.ValidateProperty(propertyName);
            KdlWriterHelper.ValidateSingle(value);

            WriteNumberEscape(propertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        /// <summary>
        /// Writes the property name and <see cref="float"/> value (as a KDL number) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="float"/> using the default <see cref="StandardFormat"/> (that is, 'G').
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(ReadOnlySpan<byte> utf8PropertyName, float value)
        {
            KdlWriterHelper.ValidateProperty(utf8PropertyName);
            KdlWriterHelper.ValidateSingle(value);

            WriteNumberEscape(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Number;
        }

        private void WriteNumberEscape(ReadOnlySpan<char> propertyName, float value)
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

        private void WriteNumberEscape(ReadOnlySpan<byte> utf8PropertyName, float value)
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

        private void WriteNumberEscapeProperty(
            ReadOnlySpan<char> propertyName,
            float value,
            int firstEscapeIndexProp
        )
        {
            Debug.Assert(
                int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= propertyName.Length
            );
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < propertyName.Length);

            char[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(
                propertyName.Length,
                firstEscapeIndexProp
            );

            Span<char> escapedPropertyName =
                length <= KdlConstants.StackallocCharThreshold
                    ? stackalloc char[KdlConstants.StackallocCharThreshold]
                    : (propertyArray = ArrayPool<char>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(
                propertyName,
                escapedPropertyName,
                firstEscapeIndexProp,
                _options.Encoder,
                out int written
            );

            WriteNumberByOptions(escapedPropertyName[..written], value);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteNumberEscapeProperty(
            ReadOnlySpan<byte> utf8PropertyName,
            float value,
            int firstEscapeIndexProp
        )
        {
            Debug.Assert(
                int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping
                    >= utf8PropertyName.Length
            );
            Debug.Assert(
                firstEscapeIndexProp >= 0 && firstEscapeIndexProp < utf8PropertyName.Length
            );

            byte[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(
                utf8PropertyName.Length,
                firstEscapeIndexProp
            );

            Span<byte> escapedPropertyName =
                length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(
                utf8PropertyName,
                escapedPropertyName,
                firstEscapeIndexProp,
                _options.Encoder,
                out int written
            );

            WriteNumberByOptions(escapedPropertyName[..written], value);

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteNumberByOptions(ReadOnlySpan<char> propertyName, float value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteNumberIndented(propertyName, value);
            }
            else
            {
                WriteNumberMinimized(propertyName, value);
            }
        }

        private void WriteNumberByOptions(ReadOnlySpan<byte> utf8PropertyName, float value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteNumberIndented(utf8PropertyName, value);
            }
            else
            {
                WriteNumberMinimized(utf8PropertyName, value);
            }
        }

        private void WriteNumberMinimized(ReadOnlySpan<char> escapedPropertyName, float value)
        {
            Debug.Assert(
                escapedPropertyName.Length
                    < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding)
                        - KdlConstants.MaximumFormatSingleLength
                        - 4
            );

            // All ASCII, 2 quotes for property name, and 1 colon => escapedPropertyName.Length + KdlConstants.MaximumFormatSingleLength + 3
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired =
                (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding)
                + KdlConstants.MaximumFormatSingleLength
                + 4;

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

            TranscodeAndWrite(escapedPropertyName, output);

            output[BytesPending++] = KdlConstants.Quote;
            output[BytesPending++] = KdlConstants.KeyValueSeparator;

            bool result = TryFormatSingle(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        private void WriteNumberMinimized(ReadOnlySpan<byte> escapedPropertyName, float value)
        {
            Debug.Assert(
                escapedPropertyName.Length
                    < int.MaxValue - KdlConstants.MaximumFormatSingleLength - 4
            );

            int minRequired =
                escapedPropertyName.Length + KdlConstants.MaximumFormatSingleLength + 3; // 2 quotes for property name, and 1 colon
            int maxRequired = minRequired + 1; // Optionally, 1 list separator

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

            escapedPropertyName.CopyTo(output[BytesPending..]);
            BytesPending += escapedPropertyName.Length;

            output[BytesPending++] = KdlConstants.Quote;
            output[BytesPending++] = KdlConstants.KeyValueSeparator;

            bool result = TryFormatSingle(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        private void WriteNumberIndented(ReadOnlySpan<char> escapedPropertyName, float value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(
                escapedPropertyName.Length
                    < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding)
                        - indent
                        - KdlConstants.MaximumFormatSingleLength
                        - 5
                        - _newLineLength
            );

            // All ASCII, 2 quotes for property name, 1 colon, and 1 space => escapedPropertyName.Length + KdlConstants.MaximumFormatSingleLength + 4
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired =
                indent
                + (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding)
                + KdlConstants.MaximumFormatSingleLength
                + 5
                + _newLineLength;

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            Debug.Assert(_options.SkipValidation || _tokenType != KdlTokenType.PropertyName);

            if (_tokenType != KdlTokenType.None)
            {
                WriteNewLine(output);
            }

            WriteIndentation(output[BytesPending..], indent);
            BytesPending += indent;

            output[BytesPending++] = KdlConstants.Quote;

            TranscodeAndWrite(escapedPropertyName, output);

            output[BytesPending++] = KdlConstants.Quote;
            output[BytesPending++] = KdlConstants.KeyValueSeparator;
            output[BytesPending++] = KdlConstants.Space;

            bool result = TryFormatSingle(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        private void WriteNumberIndented(ReadOnlySpan<byte> escapedPropertyName, float value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(
                escapedPropertyName.Length
                    < int.MaxValue
                        - indent
                        - KdlConstants.MaximumFormatSingleLength
                        - 5
                        - _newLineLength
            );

            int minRequired =
                indent + escapedPropertyName.Length + KdlConstants.MaximumFormatSingleLength + 4; // 2 quotes for property name, 1 colon, and 1 space
            int maxRequired = minRequired + 1 + _newLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            Debug.Assert(_options.SkipValidation || _tokenType != KdlTokenType.PropertyName);

            if (_tokenType != KdlTokenType.None)
            {
                WriteNewLine(output);
            }

            WriteIndentation(output[BytesPending..], indent);
            BytesPending += indent;

            output[BytesPending++] = KdlConstants.Quote;

            escapedPropertyName.CopyTo(output[BytesPending..]);
            BytesPending += escapedPropertyName.Length;

            output[BytesPending++] = KdlConstants.Quote;
            output[BytesPending++] = KdlConstants.KeyValueSeparator;
            output[BytesPending++] = KdlConstants.Space;

            bool result = TryFormatSingle(value, output[BytesPending..], out int bytesWritten);
            Debug.Assert(result);
            BytesPending += bytesWritten;
        }

        internal void WritePropertyName(float value)
        {
            Span<byte> utf8PropertyName = stackalloc byte[KdlConstants.MaximumFormatSingleLength];
            bool result = TryFormatSingle(value, utf8PropertyName, out int bytesWritten);
            Debug.Assert(result);
            WritePropertyNameUnescaped(utf8PropertyName[..bytesWritten]);
        }
    }
}
