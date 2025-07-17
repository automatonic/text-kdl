using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class UInt32Converter : KdlPrimitiveConverter<uint>
    {
        public UInt32Converter() => IsInternalConverterForNumberType = true;

        public override uint Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            return reader.GetUInt32();
        }

        public override void Write(KdlWriter writer, uint value, KdlSerializerOptions options)
        {
            // For performance, lift up the writer implementation.
            writer.WriteNumberValue((ulong)value);
        }

        internal override uint ReadAsPropertyNameCore(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetUInt32WithQuotes();
        }

        internal override void WriteAsPropertyNameCore(
            KdlWriter writer,
            uint value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty
        )
        {
            writer.WritePropertyName(value);
        }

        internal override uint ReadNumberWithCustomHandling(
            ref KdlReader reader,
            KdlNumberHandling handling,
            KdlSerializerOptions options
        )
        {
            if (
                reader.TokenType == KdlTokenType.String
                && (KdlNumberHandling.AllowReadingFromString & handling) != 0
            )
            {
                return reader.GetUInt32WithQuotes();
            }

            return reader.GetUInt32();
        }

        internal override void WriteNumberWithCustomHandling(
            KdlWriter writer,
            uint value,
            KdlNumberHandling handling
        )
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                writer.WriteNumberValueAsString(value);
            }
            else
            {
                // For performance, lift up the writer implementation.
                writer.WriteNumberValue((ulong)value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Integer, numberHandling);
    }
}
