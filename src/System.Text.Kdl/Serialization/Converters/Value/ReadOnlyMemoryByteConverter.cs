using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class ReadOnlyMemoryByteConverter : KdlConverter<ReadOnlyMemory<byte>>
    {
        public override bool HandleNull => true;

        public override ReadOnlyMemory<byte> Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.TokenType is KdlTokenType.Null ? default : reader.GetBytesFromBase64();
        }

        public override void Write(KdlWriter writer, ReadOnlyMemory<byte> value, KdlSerializerOptions options)
        {
            writer.WriteBase64StringValue(value.Span);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.String };
    }
}
