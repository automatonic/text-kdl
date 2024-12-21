using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlElementConverter : KdlConverter<KdlElement>
    {
        public override KdlElement Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return KdlElement.ParseValue(ref reader);
        }

        public override void Write(KdlWriter writer, KdlElement value, KdlSerializerOptions options)
        {
            value.WriteTo(writer);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => KdlSchema.CreateTrueSchema();
    }
}
