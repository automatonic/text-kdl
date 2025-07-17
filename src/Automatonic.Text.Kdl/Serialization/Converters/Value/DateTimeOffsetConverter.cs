using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class DateTimeOffsetConverter : KdlPrimitiveConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            return reader.GetDateTimeOffset();
        }

        public override void Write(
            KdlWriter writer,
            DateTimeOffset value,
            KdlSerializerOptions options
        )
        {
            writer.WriteStringValue(value);
        }

        internal override DateTimeOffset ReadAsPropertyNameCore(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetDateTimeOffsetNoValidation();
        }

        internal override void WriteAsPropertyNameCore(
            KdlWriter writer,
            DateTimeOffset value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty
        )
        {
            writer.WritePropertyName(value);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new() { Type = KdlSchemaType.String, Format = "date-time" };
    }
}
