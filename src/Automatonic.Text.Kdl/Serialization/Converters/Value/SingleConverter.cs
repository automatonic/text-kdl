using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class SingleConverter : KdlPrimitiveConverter<float>
    {

        public SingleConverter() => IsInternalConverterForNumberType = true;

        public override float Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetSingle();
        }

        public override void Write(KdlWriter writer, float value, KdlSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }

        internal override float ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetSingleWithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, float value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override float ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String)
            {
                if ((KdlNumberHandling.AllowReadingFromString & handling) != 0)
                {
                    return reader.GetSingleWithQuotes();
                }
                else if ((KdlNumberHandling.AllowNamedFloatingPointLiterals & handling) != 0)
                {
                    return reader.GetSingleFloatingPointConstant();
                }
            }

            return reader.GetSingle();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, float value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                writer.WriteNumberValueAsString(value);
            }
            else if ((KdlNumberHandling.AllowNamedFloatingPointLiterals & handling) != 0)
            {
                writer.WriteFloatingPointConstant(value);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Number, numberHandling, isIeeeFloatingPoint: true);
    }
}
