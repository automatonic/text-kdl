using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlValueConverter : KdlConverter<KdlValue?>
    {
        public override void Write(KdlWriter writer, KdlValue? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            value.WriteTo(writer, options);
        }

        public override KdlValue? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType is KdlTokenType.Null)
            {
                return null;
            }

            KdlElement element = KdlElement.ParseValue(ref reader);
            return KdlValue.CreateFromElement(ref element, options.GetNodeOptions());
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => KdlSchema.CreateTrueSchema();
    }
}
