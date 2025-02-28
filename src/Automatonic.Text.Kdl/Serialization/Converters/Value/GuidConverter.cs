using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class GuidConverter : KdlPrimitiveConverter<Guid>
    {
        public override Guid Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetGuid();
        }

        public override void Write(KdlWriter writer, Guid value, KdlSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        internal override Guid ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetGuidNoValidation();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, Guid value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            new() { Type = KdlSchemaType.String, Format = "uuid" };
    }
}
