using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the input as KDL content. It is expected that the input content is a single complete KDL value.
        /// </summary>
        /// <param name="kdl">The raw KDL content to write.</param>
        /// <param name="skipInputValidation">Whether to validate if the input is an RFC 8259-compliant KDL payload.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="kdl"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the length of the input is zero or greater than 715,827,882 (<see cref="int.MaxValue"/> / 3).</exception>
        /// <exception cref="KdlException">
        /// Thrown if <paramref name="skipInputValidation"/> is <see langword="false"/>, and the input
        /// is not a valid, complete, single KDL value according to the KDL RFC (https://tools.ietf.org/html/rfc8259)
        /// or the input KDL exceeds a recursive depth of 64.
        /// </exception>
        /// <remarks>
        /// When writing untrused KDL values, do not set <paramref name="skipInputValidation"/> to <see langword="true"/> as this can result in invalid KDL
        /// being written, and/or the overall payload being written to the writer instance being invalid.
        ///
        /// When using this method, the input content will be written to the writer destination as-is, unless validation fails (when it is enabled).
        ///
        /// The <see cref="KdlWriterOptions.SkipValidation"/> value for the writer instance is honored when using this method.
        ///
        /// The <see cref="KdlWriterOptions.Indented"/> and <see cref="KdlWriterOptions.Encoder"/> values for the writer instance are not applied when using this method.
        /// </remarks>
        public void WriteRawValue([StringSyntax(StringSyntaxAttribute.Json)] string kdl, bool skipInputValidation = false)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            if (kdl == null)
            {
                throw new ArgumentNullException(nameof(kdl));
            }

            TranscodeAndWriteRawValue(kdl.AsSpan(), skipInputValidation);
        }

        /// <summary>
        /// Writes the input as KDL content. It is expected that the input content is a single complete KDL value.
        /// </summary>
        /// <param name="kdl">The raw KDL content to write.</param>
        /// <param name="skipInputValidation">Whether to validate if the input is an RFC 8259-compliant KDL payload.</param>
        /// <exception cref="ArgumentException">Thrown if the length of the input is zero or greater than 715,827,882 (<see cref="int.MaxValue"/> / 3).</exception>
        /// <exception cref="KdlException">
        /// Thrown if <paramref name="skipInputValidation"/> is <see langword="false"/>, and the input
        /// is not a valid, complete, single KDL value according to the KDL RFC (https://tools.ietf.org/html/rfc8259)
        /// or the input KDL exceeds a recursive depth of 64.
        /// </exception>
        /// <remarks>
        /// When writing untrused KDL values, do not set <paramref name="skipInputValidation"/> to <see langword="true"/> as this can result in invalid KDL
        /// being written, and/or the overall payload being written to the writer instance being invalid.
        ///
        /// When using this method, the input content will be written to the writer destination as-is, unless validation fails (when it is enabled).
        ///
        /// The <see cref="KdlWriterOptions.SkipValidation"/> value for the writer instance is honored when using this method.
        ///
        /// The <see cref="KdlWriterOptions.Indented"/> and <see cref="KdlWriterOptions.Encoder"/> values for the writer instance are not applied when using this method.
        /// </remarks>
        public void WriteRawValue([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> kdl, bool skipInputValidation = false)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            TranscodeAndWriteRawValue(kdl, skipInputValidation);
        }

        /// <summary>
        /// Writes the input as KDL content. It is expected that the input content is a single complete KDL value.
        /// </summary>
        /// <param name="utf8Kdl">The raw KDL content to write.</param>
        /// <param name="skipInputValidation">Whether to validate if the input is an RFC 8259-compliant KDL payload.</param>
        /// <exception cref="ArgumentException">Thrown if the length of the input is zero or greater than or equal to <see cref="int.MaxValue"/>.</exception>
        /// <exception cref="KdlException">
        /// Thrown if <paramref name="skipInputValidation"/> is <see langword="false"/>, and the input
        /// is not a valid, complete, single KDL value according to the KDL RFC (https://tools.ietf.org/html/rfc8259)
        /// or the input KDL exceeds a recursive depth of 64.
        /// </exception>
        /// <remarks>
        /// When writing untrused KDL values, do not set <paramref name="skipInputValidation"/> to <see langword="true"/> as this can result in invalid KDL
        /// being written, and/or the overall payload being written to the writer instance being invalid.
        ///
        /// When using this method, the input content will be written to the writer destination as-is, unless validation fails (when it is enabled).
        ///
        /// The <see cref="KdlWriterOptions.SkipValidation"/> value for the writer instance is honored when using this method.
        ///
        /// The <see cref="KdlWriterOptions.Indented"/> and <see cref="KdlWriterOptions.Encoder"/> values for the writer instance are not applied when using this method.
        /// </remarks>
        public void WriteRawValue(ReadOnlySpan<byte> utf8Kdl, bool skipInputValidation = false)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            if (utf8Kdl.Length == int.MaxValue)
            {
                ThrowHelper.ThrowArgumentException_ValueTooLarge(int.MaxValue);
            }

            WriteRawValueCore(utf8Kdl, skipInputValidation);
        }

        /// <summary>
        /// Writes the input as KDL content. It is expected that the input content is a single complete KDL value.
        /// </summary>
        /// <param name="utf8Kdl">The raw KDL content to write.</param>
        /// <param name="skipInputValidation">Whether to validate if the input is an RFC 8259-compliant KDL payload.</param>
        /// <exception cref="ArgumentException">Thrown if the length of the input is zero or equal to <see cref="int.MaxValue"/>.</exception>
        /// <exception cref="KdlException">
        /// Thrown if <paramref name="skipInputValidation"/> is <see langword="false"/>, and the input
        /// is not a valid, complete, single KDL value according to the KDL RFC (https://tools.ietf.org/html/rfc8259)
        /// or the input KDL exceeds a recursive depth of 64.
        /// </exception>
        /// <remarks>
        /// When writing untrused KDL values, do not set <paramref name="skipInputValidation"/> to <see langword="true"/> as this can result in invalid KDL
        /// being written, and/or the overall payload being written to the writer instance being invalid.
        ///
        /// When using this method, the input content will be written to the writer destination as-is, unless validation fails (when it is enabled).
        ///
        /// The <see cref="KdlWriterOptions.SkipValidation"/> value for the writer instance is honored when using this method.
        ///
        /// The <see cref="KdlWriterOptions.Indented"/> and <see cref="KdlWriterOptions.Encoder"/> values for the writer instance are not applied when using this method.
        /// </remarks>
        public void WriteRawValue(ReadOnlySequence<byte> utf8Kdl, bool skipInputValidation = false)
        {
            if (!_options.SkipValidation)
            {
                ValidateWritingValue();
            }

            long utf8KdlLen = utf8Kdl.Length;

            if (utf8KdlLen == 0)
            {
                ThrowHelper.ThrowArgumentException(SR.ExpectedKdlTokens);
            }
            if (utf8KdlLen >= int.MaxValue)
            {
                ThrowHelper.ThrowArgumentException_ValueTooLarge(utf8KdlLen);
            }

            if (skipInputValidation)
            {
                // Treat all unvalidated raw KDL value writes as string. If the payload is valid, this approach does
                // not affect structural validation since a string token is equivalent to a complete object, array,
                // or other complete KDL tokens when considering structural validation on subsequent writer calls.
                // If the payload is not valid, then we make no guarantees about the structural validation of the final payload.
                _tokenType = KdlTokenType.String;
            }
            else
            {
                // Utilize reader validation.
                KdlReader reader = new(utf8Kdl);
                while (reader.Read());
                _tokenType = reader.TokenType;
            }

            Debug.Assert(utf8KdlLen < int.MaxValue);
            int len = (int)utf8KdlLen;

            // TODO (https://github.com/dotnet/runtime/issues/29293):
            // investigate writing this in chunks, rather than requesting one potentially long, contiguous buffer.
            int maxRequired = len + 1; // Optionally, 1 list separator. We've guarded against integer overflow earlier in the call stack.

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            utf8Kdl.CopyTo(output.Slice(BytesPending));
            BytesPending += len;

            SetFlagToAddListSeparatorBeforeNextItem();
        }

        private void TranscodeAndWriteRawValue(ReadOnlySpan<char> kdl, bool skipInputValidation)
        {
            if (kdl.Length > KdlConstants.MaxUtf16RawValueLength)
            {
                ThrowHelper.ThrowArgumentException_ValueTooLarge(kdl.Length);
            }

            byte[]? tempArray = null;

            // For performance, avoid obtaining actual byte count unless memory usage is higher than the threshold.
            Span<byte> utf8Kdl =
                // Use stack memory
                kdl.Length <= (KdlConstants.StackallocByteThreshold / KdlConstants.MaxExpansionFactorWhileTranscoding) ? stackalloc byte[KdlConstants.StackallocByteThreshold] :
                // Use a pooled array
                kdl.Length <= (KdlConstants.ArrayPoolMaxSizeBeforeUsingNormalAlloc / KdlConstants.MaxExpansionFactorWhileTranscoding) ? tempArray = ArrayPool<byte>.Shared.Rent(kdl.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) :
                // Use a normal alloc since the pool would create a normal alloc anyway based on the threshold (per current implementation)
                // and by using a normal alloc we can avoid the Clear().
                new byte[KdlReaderHelper.GetUtf8ByteCount(kdl)];

            try
            {
                int actualByteCount = KdlReaderHelper.GetUtf8FromText(kdl, utf8Kdl);
                utf8Kdl = utf8Kdl.Slice(0, actualByteCount);
                WriteRawValueCore(utf8Kdl, skipInputValidation);
            }
            finally
            {
                if (tempArray != null)
                {
                    utf8Kdl.Clear();
                    ArrayPool<byte>.Shared.Return(tempArray);
                }
            }
        }

        private void WriteRawValueCore(ReadOnlySpan<byte> utf8Kdl, bool skipInputValidation)
        {
            int len = utf8Kdl.Length;

            if (len == 0)
            {
                ThrowHelper.ThrowArgumentException(SR.ExpectedKdlTokens);
            }

            // In the UTF-16-based entry point methods above, we validate that the payload length <= int.MaxValue /3.
            // The result of this division will be rounded down, so even if every input character needs to be transcoded
            // (with expansion factor of 3), the resulting payload would be less than int.MaxValue,
            // as (int.MaxValue/3) * 3 is less than int.MaxValue.
            Debug.Assert(len < int.MaxValue);

            if (skipInputValidation)
            {
                // Treat all unvalidated raw KDL value writes as string. If the payload is valid, this approach does
                // not affect structural validation since a string token is equivalent to a complete object, array,
                // or other complete KDL tokens when considering structural validation on subsequent writer calls.
                // If the payload is not valid, then we make no guarantees about the structural validation of the final payload.
                _tokenType = KdlTokenType.String;
            }
            else
            {
                // Utilize reader validation.
                KdlReader reader = new(utf8Kdl);
                while (reader.Read());
                _tokenType = reader.TokenType;
            }

            // TODO (https://github.com/dotnet/runtime/issues/29293):
            // investigate writing this in chunks, rather than requesting one potentially long, contiguous buffer.
            int maxRequired = len + 1; // Optionally, 1 list separator. We've guarded against integer overflow earlier in the call stack.

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            utf8Kdl.CopyTo(output.Slice(BytesPending));
            BytesPending += len;

            SetFlagToAddListSeparatorBeforeNextItem();
        }
    }
}
