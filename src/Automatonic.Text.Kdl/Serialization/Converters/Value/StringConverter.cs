using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class StringConverter : KdlPrimitiveConverter<string?>
    {
        public override string? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetString();
        }

        public override void Write(KdlWriter writer, string? value, KdlSerializerOptions options)
        {
            // For performance, lift up the writer implementation.
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.AsSpan());
            }
        }

        internal override string ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetString()!;
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, string value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

            if (options.DictionaryKeyPolicy != null && !isWritingExtensionDataProperty)
            {
                value = options.DictionaryKeyPolicy.ConvertName(value);

                if (value == null)
                {
                    ThrowHelper.ThrowInvalidOperationException_NamingPolicyReturnNull(options.DictionaryKeyPolicy);
                }
            }

            writer.WritePropertyName(value);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.String };
    }
}
