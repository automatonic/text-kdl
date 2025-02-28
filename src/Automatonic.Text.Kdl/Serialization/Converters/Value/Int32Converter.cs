using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class Int32Converter : KdlPrimitiveConverter<int>
    {
        public Int32Converter() => IsInternalConverterForNumberType = true;

        public override int Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetInt32();
        }

        public override void Write(KdlWriter writer, int value, KdlSerializerOptions options)
        {
            // For performance, lift up the writer implementation.
            writer.WriteNumberValue((long)value);
        }

        internal override int ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetInt32WithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, int value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override int ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return reader.GetInt32WithQuotes();
            }

            return reader.GetInt32();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, int value, KdlNumberHandling handling)
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
