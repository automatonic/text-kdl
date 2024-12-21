using System.Diagnostics;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class SByteConverter : KdlPrimitiveConverter<sbyte>
    {
        public SByteConverter() => IsInternalConverterForNumberType = true;

        public override sbyte Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetSByte();
        }

        public override void Write(KdlWriter writer, sbyte value, KdlSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }

        internal override sbyte ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetSByteWithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, sbyte value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override sbyte ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return reader.GetSByteWithQuotes();
            }

            return reader.GetSByte();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, sbyte value, KdlNumberHandling handling)
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
            GetSchemaForNumericType(KdlSchemaType.Integer, numberHandling);
    }
}
