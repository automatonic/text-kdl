using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and raw bytes value (as a Base64 encoded KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write.</param>
        /// <param name="bytes">The binary data to write as Base64 encoded text.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteBase64String(KdlEncodedText propertyName, ReadOnlySpan<byte> bytes)
        {
            ReadOnlySpan<byte> utf8PropertyName = propertyName.EncodedUtf8Bytes;
            Debug.Assert(utf8PropertyName.Length <= KdlConstants.MaxUnescapedTokenSize);

            WriteBase64ByOptions(utf8PropertyName, bytes);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        /// <summary>
        /// Writes the property name and raw bytes value (as a Base64 encoded KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="bytes">The binary data to write as Base64 encoded text.</param>
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
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBase64String(string propertyName, ReadOnlySpan<byte> bytes)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteBase64String(propertyName.AsSpan(), bytes);
        }

        /// <summary>
        /// Writes the property name and raw bytes value (as a Base64 encoded KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="bytes">The binary data to write as Base64 encoded text.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBase64String(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> bytes)
        {
            KdlWriterHelper.ValidatePropertyNameLength(propertyName);

            WriteBase64Escape(propertyName, bytes);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        /// <summary>
        /// Writes the property name and raw bytes value (as a Base64 encoded KDL string) as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="bytes">The binary data to write as Base64 encoded text.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBase64String(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> bytes)
        {
            KdlWriterHelper.ValidatePropertyNameLength(utf8PropertyName);

            WriteBase64Escape(utf8PropertyName, bytes);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.String;
        }

        private void WriteBase64Escape(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> bytes)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(propertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteBase64EscapeProperty(propertyName, bytes, propertyIdx);
            }
            else
            {
                WriteBase64ByOptions(propertyName, bytes);
            }
        }

        private void WriteBase64Escape(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> bytes)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(utf8PropertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteBase64EscapeProperty(utf8PropertyName, bytes, propertyIdx);
            }
            else
            {
                WriteBase64ByOptions(utf8PropertyName, bytes);
            }
        }

        private void WriteBase64EscapeProperty(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> bytes, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= propertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < propertyName.Length);

            char[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

            Span<char> escapedPropertyName = length <= KdlConstants.StackallocCharThreshold ?
                stackalloc char[KdlConstants.StackallocCharThreshold] :
                (propertyArray = ArrayPool<char>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(propertyName, escapedPropertyName, firstEscapeIndexProp, _options.Encoder, out int written);

            WriteBase64ByOptions(escapedPropertyName[..written], bytes);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteBase64EscapeProperty(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> bytes, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < utf8PropertyName.Length);

            byte[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

            Span<byte> escapedPropertyName = length <= KdlConstants.StackallocByteThreshold ?
                stackalloc byte[KdlConstants.StackallocByteThreshold] :
                (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(utf8PropertyName, escapedPropertyName, firstEscapeIndexProp, _options.Encoder, out int written);

            WriteBase64ByOptions(escapedPropertyName[..written], bytes);

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteBase64ByOptions(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> bytes)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteBase64Indented(propertyName, bytes);
            }
            else
            {
                WriteBase64Minimized(propertyName, bytes);
            }
        }

        private void WriteBase64ByOptions(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> bytes)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteBase64Indented(utf8PropertyName, bytes);
            }
            else
            {
                WriteBase64Minimized(utf8PropertyName, bytes);
            }
        }

        private void WriteBase64Minimized(ReadOnlySpan<char> escapedPropertyName, ReadOnlySpan<byte> bytes)
        {
            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);

            Debug.Assert(escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding < int.MaxValue - encodedLength - 6);

            // All ASCII, 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, and 1 colon => escapedPropertyName.Length + encodedLength + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding.
            int maxRequired = (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) + encodedLength + 6;

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

            Base64EncodeAndWrite(bytes, output);

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteBase64Minimized(ReadOnlySpan<byte> escapedPropertyName, ReadOnlySpan<byte> bytes)
        {
            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);

            Debug.Assert(escapedPropertyName.Length < int.MaxValue - encodedLength - 6);

            // 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, and 1 colon => escapedPropertyName.Length + encodedLength + 5
            // Optionally, 1 list separator.
            int maxRequired = escapedPropertyName.Length + encodedLength + 6;

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

            Base64EncodeAndWrite(bytes, output);

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteBase64Indented(ReadOnlySpan<char> escapedPropertyName, ReadOnlySpan<byte> bytes)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);

            Debug.Assert(escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding < int.MaxValue - indent - encodedLength - 7 - _newLineLength);

            // All ASCII, 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, 1 colon, and 1 space => indent + escapedPropertyName.Length + encodedLength + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding.
            int maxRequired = indent + (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) + encodedLength + 7 + _newLineLength;

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

            Base64EncodeAndWrite(bytes, output);

            output[BytesPending++] = KdlConstants.Quote;
        }

        private void WriteBase64Indented(ReadOnlySpan<byte> escapedPropertyName, ReadOnlySpan<byte> bytes)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);

            Debug.Assert(escapedPropertyName.Length < int.MaxValue - indent - encodedLength - 7 - _newLineLength);

            // 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, 1 colon, and 1 space => indent + escapedPropertyName.Length + encodedLength + 6
            // Optionally, 1 list separator, and 1-2 bytes for new line.
            int maxRequired = indent + escapedPropertyName.Length + encodedLength + 7 + _newLineLength;

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

            Base64EncodeAndWrite(bytes, output);

            output[BytesPending++] = KdlConstants.Quote;
        }
    }
}
