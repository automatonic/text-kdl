using System.Buffers;
using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and <see cref="DateTime"/> value (as a KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name should already be escaped when the instance of <see cref="KdlEncodedText"/> was created.
        /// </remarks>
        public void WriteString(KdlEncodedText propertyName, DateTime value)
        {
            ReadOnlySpan<byte> utf8PropertyName = propertyName.EncodedUtf8Bytes;
            Debug.Assert(utf8PropertyName.Length <= KdlConstants.MaxUnescapedTokenSize);

            WriteStringByOptions(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        /// <summary>
        /// Writes the property name and <see cref="DateTime"/> value (as a KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
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
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(string propertyName, DateTime value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteString(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and <see cref="DateTime"/> value (as a KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(ReadOnlySpan<char> propertyName, DateTime value)
        {
            KdlWriterHelper.ValidateProperty(propertyName);

            WriteStringEscape(propertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        /// <summary>
        /// Writes the property name and <see cref="DateTime"/> value (as a KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(ReadOnlySpan<byte> utf8PropertyName, DateTime value)
        {
            KdlWriterHelper.ValidateProperty(utf8PropertyName);

            WriteStringEscape(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        private void WriteStringEscape(ReadOnlySpan<char> propertyName, DateTime value)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(propertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStringEscapeProperty(propertyName, value, propertyIdx);
            }
            else
            {
                WriteStringByOptions(propertyName, value);
            }
        }

        private void WriteStringEscape(ReadOnlySpan<byte> utf8PropertyName, DateTime value)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(utf8PropertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStringEscapeProperty(utf8PropertyName, value, propertyIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, value);
            }
        }

        private void WriteStringEscapeProperty(
            ReadOnlySpan<char> propertyName,
            DateTime value,
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

            WriteStringByOptions(escapedPropertyName[..written], value);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscapeProperty(
            ReadOnlySpan<byte> utf8PropertyName,
            DateTime value,
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

            WriteStringByOptions(escapedPropertyName[..written], value);

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringByOptions(ReadOnlySpan<char> propertyName, DateTime value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndented(propertyName, value);
            }
            else
            {
                WriteStringMinimized(propertyName, value);
            }
        }

        private void WriteStringByOptions(ReadOnlySpan<byte> utf8PropertyName, DateTime value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndented(utf8PropertyName, value);
            }
            else
            {
                WriteStringMinimized(utf8PropertyName, value);
            }
        }

        private void WriteStringMinimized(ReadOnlySpan<char> escapedPropertyName, DateTime value)
        {
            Debug.Assert(
                escapedPropertyName.Length
                    < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding)
                        - KdlConstants.MaximumFormatDateTimeOffsetLength
                        - 6
            );

            // All ASCII, 2 quotes for property name, 2 quotes for date, and 1 colon => escapedPropertyName.Length + KdlConstants.MaximumFormatDateTimeOffsetLength + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired =
                (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding)
                + KdlConstants.MaximumFormatDateTimeOffsetLength
                + 6;

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

            output[BytesPending++] = KdlConstants.Quote;

            KdlWriterHelper.WriteDateTimeTrimmed(
                output[BytesPending..],
                value,
                out int bytesWritten
            );
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteStringMinimized(ReadOnlySpan<byte> escapedPropertyName, DateTime value)
        {
            Debug.Assert(
                escapedPropertyName.Length
                    < int.MaxValue - KdlConstants.MaximumFormatDateTimeOffsetLength - 6
            );

            int minRequired =
                escapedPropertyName.Length + KdlConstants.MaximumFormatDateTimeOffsetLength + 5; // 2 quotes for property name, 2 quotes for date, and 1 colon
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

            output[BytesPending++] = KdlConstants.Quote;

            KdlWriterHelper.WriteDateTimeTrimmed(
                output[BytesPending..],
                value,
                out int bytesWritten
            );
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteStringIndented(ReadOnlySpan<char> escapedPropertyName, DateTime value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(
                escapedPropertyName.Length
                    < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding)
                        - indent
                        - KdlConstants.MaximumFormatDateTimeOffsetLength
                        - 7
                        - _newLineLength
            );

            // All ASCII, 2 quotes for property name, 2 quotes for date, 1 colon, and 1 space => escapedPropertyName.Length + KdlConstants.MaximumFormatDateTimeOffsetLength + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired =
                indent
                + (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding)
                + KdlConstants.MaximumFormatDateTimeOffsetLength
                + 7
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

            output[BytesPending++] = KdlConstants.Quote;

            KdlWriterHelper.WriteDateTimeTrimmed(
                output[BytesPending..],
                value,
                out int bytesWritten
            );
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteStringIndented(ReadOnlySpan<byte> escapedPropertyName, DateTime value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(
                escapedPropertyName.Length
                    < int.MaxValue
                        - indent
                        - KdlConstants.MaximumFormatDateTimeOffsetLength
                        - 7
                        - _newLineLength
            );

            int minRequired =
                indent
                + escapedPropertyName.Length
                + KdlConstants.MaximumFormatDateTimeOffsetLength
                + 6; // 2 quotes for property name, 2 quotes for date, 1 colon, and 1 space
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

            output[BytesPending++] = KdlConstants.Quote;

            KdlWriterHelper.WriteDateTimeTrimmed(
                output[BytesPending..],
                value,
                out int bytesWritten
            );
            BytesPending += bytesWritten;

            output[BytesPending++] = KdlConstants.Quote;
        }

        internal void WritePropertyName(DateTime value)
        {
            Span<byte> buffer = stackalloc byte[KdlConstants.MaximumFormatDateTimeOffsetLength];
            KdlWriterHelper.WriteDateTimeTrimmed(buffer, value, out int bytesWritten);
            WritePropertyNameUnescaped(buffer[..bytesWritten]);
        }
    }
}
