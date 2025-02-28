using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class UInt16Converter : KdlPrimitiveConverter<ushort>
    {
        public UInt16Converter() => IsInternalConverterForNumberType = true;

        public override ushort Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetUInt16();
        }

        public override void Write(KdlWriter writer, ushort value, KdlSerializerOptions options)
        {
            // For performance, lift up the writer implementation.
            writer.WriteNumberValue((long)value);
        }

        internal override ushort ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetUInt16WithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, ushort value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override ushort ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return reader.GetUInt16WithQuotes();
            }

            return reader.GetUInt16();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, ushort value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                writer.WriteNumberValueAsString(value);
            }
            else
            {
                // For performance, lift up the writer implementation.
                writer.WriteNumberValue((long)value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Integer, numberHandling);
    }
}
