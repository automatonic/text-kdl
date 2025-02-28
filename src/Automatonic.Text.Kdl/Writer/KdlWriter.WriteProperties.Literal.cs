using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and the KDL literal "null" as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteNull(KdlEncodedText propertyName)
        {
            WriteLiteralHelper(propertyName.EncodedUtf8Bytes, KdlConstants.NullValue);
            _tokenType = KdlTokenType.Null;
        }

        internal void WriteNullSection(ReadOnlySpan<byte> escapedPropertyNameSection)
        {
            if (_options.Indented)
            {
                ReadOnlySpan<byte> escapedName =
                    escapedPropertyNameSection[1..^2];

                WriteLiteralHelper(escapedName, KdlConstants.NullValue);
                _tokenType = KdlTokenType.Null;
            }
            else
            {
                Debug.Assert(escapedPropertyNameSection.Length <= KdlConstants.MaxUnescapedTokenSize - 3);

                ReadOnlySpan<byte> span = KdlConstants.NullValue;

                WriteLiteralSection(escapedPropertyNameSection, span);

                SetFlagToAddListSeparatorBeforeNextItem();
                _tokenType = KdlTokenType.Null;
            }
        }

        private void WriteLiteralHelper(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value)
        {
            Debug.Assert(utf8PropertyName.Length <= KdlConstants.MaxUnescapedTokenSize);

            WriteLiteralByOptions(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
        }

        /// <summary>
        /// Writes the property name and the KDL literal "null" as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
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
        public void WriteNull(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteNull(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the property name and the KDL literal "null" as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNull(ReadOnlySpan<char> propertyName)
        {
            KdlWriterHelper.ValidateProperty(propertyName);

            ReadOnlySpan<byte> span = KdlConstants.NullValue;

            WriteLiteralEscape(propertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Null;
        }

        /// <summary>
        /// Writes the property name and the KDL literal "null" as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNull(ReadOnlySpan<byte> utf8PropertyName)
        {
            KdlWriterHelper.ValidateProperty(utf8PropertyName);

            ReadOnlySpan<byte> span = KdlConstants.NullValue;

            WriteLiteralEscape(utf8PropertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = KdlTokenType.Null;
        }

        /// <summary>
        /// Writes the pre-encoded property name and <see cref="bool"/> value (as a KDL literal "true" or "false") as part of a name/value pair of a KDL object.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteBoolean(KdlEncodedText propertyName, bool value)
        {
            if (value)
            {
                WriteLiteralHelper(propertyName.EncodedUtf8Bytes, KdlConstants.TrueValue);
                _tokenType = KdlTokenType.True;
            }
            else
            {
                WriteLiteralHelper(propertyName.EncodedUtf8Bytes, KdlConstants.FalseValue);
                _tokenType = KdlTokenType.False;
            }
        }

        /// <summary>
        /// Writes the property name and <see cref="bool"/> value (as a KDL literal "true" or "false") as part of a name/value pair of a KDL object.
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
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBoolean(string propertyName, bool value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteBoolean(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and <see cref="bool"/> value (as a KDL literal "true" or "false") as part of a name/value pair of a KDL object.
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
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBoolean(ReadOnlySpan<char> propertyName, bool value)
        {
            KdlWriterHelper.ValidateProperty(propertyName);

            ReadOnlySpan<byte> span = value ? KdlConstants.TrueValue : KdlConstants.FalseValue;

            WriteLiteralEscape(propertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = value ? KdlTokenType.True : KdlTokenType.False;
        }

        /// <summary>
        /// Writes the property name and <see cref="bool"/> value (as a KDL literal "true" or "false") as part of a name/value pair of a KDL object.
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
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBoolean(ReadOnlySpan<byte> utf8PropertyName, bool value)
        {
            KdlWriterHelper.ValidateProperty(utf8PropertyName);

            ReadOnlySpan<byte> span = value ? KdlConstants.TrueValue : KdlConstants.FalseValue;

            WriteLiteralEscape(utf8PropertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = value ? KdlTokenType.True : KdlTokenType.False;
        }

        private void WriteLiteralEscape(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> value)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(propertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteLiteralEscapeProperty(propertyName, value, propertyIdx);
            }
            else
            {
                WriteLiteralByOptions(propertyName, value);
            }
        }

        private void WriteLiteralEscape(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(utf8PropertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteLiteralEscapeProperty(utf8PropertyName, value, propertyIdx);
            }
            else
            {
                WriteLiteralByOptions(utf8PropertyName, value);
            }
        }

        private void WriteLiteralEscapeProperty(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> value, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= propertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < propertyName.Length);

            char[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

            Span<char> escapedPropertyName = length <= KdlConstants.StackallocCharThreshold ?
                stackalloc char[KdlConstants.StackallocCharThreshold] :
                (propertyArray = ArrayPool<char>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(propertyName, escapedPropertyName, firstEscapeIndexProp, _options.Encoder, out int written);

            WriteLiteralByOptions(escapedPropertyName[..written], value);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteLiteralEscapeProperty(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < utf8PropertyName.Length);

            byte[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

            Span<byte> escapedPropertyName = length <= KdlConstants.StackallocByteThreshold ?
                stackalloc byte[KdlConstants.StackallocByteThreshold] :
                (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(utf8PropertyName, escapedPropertyName, firstEscapeIndexProp, _options.Encoder, out int written);

            WriteLiteralByOptions(escapedPropertyName[..written], value);

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteLiteralByOptions(ReadOnlySpan<char> propertyName, ReadOnlySpan<byte> value)
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

        private void WriteLiteralByOptions(ReadOnlySpan<byte> utf8PropertyName, ReadOnlySpan<byte> value)
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

        private void WriteLiteralMinimized(ReadOnlySpan<char> escapedPropertyName, ReadOnlySpan<byte> value)
        {
            Debug.Assert(value.Length <= KdlConstants.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding) - value.Length - 4);

            // All ASCII, 2 quotes for property name, and 1 colon => escapedPropertyName.Length + value.Length + 3
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) + value.Length + 4;

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

            value.CopyTo(output[BytesPending..]);
            BytesPending += value.Length;
        }

        private void WriteLiteralMinimized(ReadOnlySpan<byte> escapedPropertyName, ReadOnlySpan<byte> value)
        {
            Debug.Assert(value.Length <= KdlConstants.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < int.MaxValue - value.Length - 4);

            int minRequired = escapedPropertyName.Length + value.Length + 3; // 2 quotes for property name, and 1 colon
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

            value.CopyTo(output[BytesPending..]);
            BytesPending += value.Length;
        }

        // AggressiveInlining used since this is only called from one location.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteLiteralSection(ReadOnlySpan<byte> escapedPropertyNameSection, ReadOnlySpan<byte> value)
        {
            Debug.Assert(value.Length <= KdlConstants.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyNameSection.Length < int.MaxValue - value.Length - 1);

            int minRequired = escapedPropertyNameSection.Length + value.Length;
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

            escapedPropertyNameSection.CopyTo(output[BytesPending..]);
            BytesPending += escapedPropertyNameSection.Length;

            value.CopyTo(output[BytesPending..]);
            BytesPending += value.Length;
        }

        private void WriteLiteralIndented(ReadOnlySpan<char> escapedPropertyName, ReadOnlySpan<byte> value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(value.Length <= KdlConstants.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding) - indent - value.Length - 5 - _newLineLength);

            // All ASCII, 2 quotes for property name, 1 colon, and 1 space => escapedPropertyName.Length + value.Length + 4
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) + value.Length + 5 + _newLineLength;

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

            value.CopyTo(output[BytesPending..]);
            BytesPending += value.Length;
        }

        private void WriteLiteralIndented(ReadOnlySpan<byte> escapedPropertyName, ReadOnlySpan<byte> value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(value.Length <= KdlConstants.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < int.MaxValue - indent - value.Length - 5 - _newLineLength);

            int minRequired = indent + escapedPropertyName.Length + value.Length + 4; // 2 quotes for property name, 1 colon, and 1 space
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

            value.CopyTo(output[BytesPending..]);
            BytesPending += value.Length;
        }

        internal void WritePropertyName(bool value)
        {
            Span<byte> utf8PropertyName = stackalloc byte[KdlConstants.MaximumFormatBooleanLength];

            bool result = Utf8Formatter.TryFormat(value, utf8PropertyName, out int bytesWritten);
            Debug.Assert(result);

            WritePropertyNameUnescaped(utf8PropertyName[..bytesWritten]);
        }
    }
}
