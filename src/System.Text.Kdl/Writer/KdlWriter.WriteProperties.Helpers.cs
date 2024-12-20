using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidatePropertyNameAndDepth(ReadOnlySpan<char> propertyName)
        {
            if (propertyName.Length > KdlConstants.MaxCharacterTokenSize || CurrentDepth >= _options.MaxDepth)
                ThrowHelper.ThrowInvalidOperationOrArgumentException(propertyName, _currentDepth, _options.MaxDepth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidatePropertyNameAndDepth(ReadOnlySpan<byte> utf8PropertyName)
        {
            if (utf8PropertyName.Length > KdlConstants.MaxUnescapedTokenSize || CurrentDepth >= _options.MaxDepth)
                ThrowHelper.ThrowInvalidOperationOrArgumentException(utf8PropertyName, _currentDepth, _options.MaxDepth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateDepth()
        {
            if (CurrentDepth >= _options.MaxDepth)
                ThrowHelper.ThrowInvalidOperationException(_currentDepth, _options.MaxDepth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateWritingProperty()
        {
            if (!_options.SkipValidation)
            {
                if (!_inObject || _tokenType == KdlTokenType.PropertyName)
                {
                    Debug.Assert(_tokenType != KdlTokenType.StartObject);
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotWritePropertyWithinArray, currentDepth: default, maxDepth: _options.MaxDepth, token: default, _tokenType);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateWritingProperty(byte token)
        {
            if (!_options.SkipValidation)
            {
                if (!_inObject || _tokenType == KdlTokenType.PropertyName)
                {
                    Debug.Assert(_tokenType != KdlTokenType.StartObject);
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotWritePropertyWithinArray, currentDepth: default, maxDepth: _options.MaxDepth, token: default, _tokenType);
                }
                UpdateBitStackOnStart(token);
            }
        }

        private void WritePropertyNameMinimized(ReadOnlySpan<byte> escapedPropertyName, byte token)
        {
            Debug.Assert(escapedPropertyName.Length < int.MaxValue - 5);

            int minRequired = escapedPropertyName.Length + 4; // 2 quotes, 1 colon, and 1 start token
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

            escapedPropertyName.CopyTo(output.Slice(BytesPending));
            BytesPending += escapedPropertyName.Length;

            output[BytesPending++] = KdlConstants.Quote;
            output[BytesPending++] = KdlConstants.KeyValueSeparator;
            output[BytesPending++] = token;
        }

        private void WritePropertyNameIndented(ReadOnlySpan<byte> escapedPropertyName, byte token)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(escapedPropertyName.Length < int.MaxValue - indent - 6 - _newLineLength);

            int minRequired = indent + escapedPropertyName.Length + 5; // 2 quotes, 1 colon, 1 space, and 1 start token
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

            WriteIndentation(output.Slice(BytesPending), indent);
            BytesPending += indent;

            output[BytesPending++] = KdlConstants.Quote;

            escapedPropertyName.CopyTo(output.Slice(BytesPending));
            BytesPending += escapedPropertyName.Length;

            output[BytesPending++] = KdlConstants.Quote;

            output[BytesPending++] = KdlConstants.KeyValueSeparator;
            output[BytesPending++] = KdlConstants.Space;
            output[BytesPending++] = token;
        }

        private void WritePropertyNameMinimized(ReadOnlySpan<char> escapedPropertyName, byte token)
        {
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding) - 5);

            // All ASCII, 2 quotes, 1 colon, and 1 start token => escapedPropertyName.Length + 4
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) + 5;

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
            output[BytesPending++] = token;
        }

        private void WritePropertyNameIndented(ReadOnlySpan<char> escapedPropertyName, byte token)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / KdlConstants.MaxExpansionFactorWhileTranscoding) - indent - 6 - _newLineLength);

            // All ASCII, 2 quotes, 1 colon, 1 space, and 1 start token => indent + escapedPropertyName.Length + 5
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) + 6 + _newLineLength;

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

            WriteIndentation(output.Slice(BytesPending), indent);
            BytesPending += indent;

            output[BytesPending++] = KdlConstants.Quote;

            TranscodeAndWrite(escapedPropertyName, output);

            output[BytesPending++] = KdlConstants.Quote;

            output[BytesPending++] = KdlConstants.KeyValueSeparator;
            output[BytesPending++] = KdlConstants.Space;
            output[BytesPending++] = token;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TranscodeAndWrite(ReadOnlySpan<char> escapedPropertyName, Span<byte> output)
        {
            OperationStatus status = KdlWriterHelper.ToUtf8(escapedPropertyName, output.Slice(BytesPending), out int written);
            Debug.Assert(status == OperationStatus.Done);
            BytesPending += written;
        }
    }
}
