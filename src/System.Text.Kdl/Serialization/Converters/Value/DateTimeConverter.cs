using System.Diagnostics;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class DateTimeConverter : KdlPrimitiveConverter<DateTime>
    {
        public override DateTime Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetDateTime();
        }

        public override void Write(KdlWriter writer, DateTime value, KdlSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        internal override DateTime ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetDateTimeNoValidation();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, DateTime value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.String, Format = "date-time" };
    }
}
