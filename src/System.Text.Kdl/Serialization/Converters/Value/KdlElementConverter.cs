using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlElementConverter : KdlConverter<KdlReadOnlyElement>
    {
        public override KdlReadOnlyElement Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return KdlReadOnlyElement.ParseValue(ref reader);
        }

        public override void Write(KdlWriter writer, KdlReadOnlyElement value, KdlSerializerOptions options)
        {
            value.WriteTo(writer);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => KdlSchema.CreateTrueSchema();
    }
}
