using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class ByteArrayConverter : KdlConverter<byte[]?>
    {
        public override byte[]? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.Null)
            {
                return null;
            }

            return reader.GetBytesFromBase64();
        }

        public override void Write(KdlWriter writer, byte[]? value, KdlSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteBase64StringValue(value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.String };
    }
}
