using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class CharConverter : KdlPrimitiveConverter<char>
    {
        private const int MaxEscapedCharacterLength = KdlConstants.MaxExpansionFactorWhileEscaping;

        public override char Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType is not (KdlTokenType.String or KdlTokenType.PropertyName))
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedString(reader.TokenType);
            }

            if (!KdlHelpers.IsInRangeInclusive(reader.ValueLength, 1, MaxEscapedCharacterLength))
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedChar(reader.TokenType);
            }

            Span<char> buffer = stackalloc char[MaxEscapedCharacterLength];
            int charsWritten = reader.CopyString(buffer);

            if (charsWritten != 1)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedChar(reader.TokenType);
            }

            return buffer[0];
        }

        public override void Write(KdlWriter writer, char value, KdlSerializerOptions options)
        {
            writer.WriteStringValue(
#if NET
                new ReadOnlySpan<char>(in value)
#else
                value.ToString()
#endif
                );
        }

        internal override char ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return Read(ref reader, typeToConvert, options);
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, char value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(
#if NET
                new ReadOnlySpan<char>(in value)
#else
                value.ToString()
#endif
                );
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new() { Type = KdlSchemaType.String, MinLength = 1, MaxLength = 1 };
    }
}
