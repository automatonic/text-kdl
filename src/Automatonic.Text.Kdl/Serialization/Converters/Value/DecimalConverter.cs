using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class DecimalConverter : KdlPrimitiveConverter<decimal>
    {
        public DecimalConverter() => IsInternalConverterForNumberType = true;

        public override decimal Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetDecimal();
        }

        public override void Write(KdlWriter writer, decimal value, KdlSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }

        internal override decimal ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetDecimalWithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, decimal value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override decimal ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return reader.GetDecimalWithQuotes();
            }

            return reader.GetDecimal();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, decimal value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                writer.WriteNumberValueAsString(value);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Number, numberHandling);
    }
}
