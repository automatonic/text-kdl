using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlReadOnlyDocumentConverter : KdlConverter<KdlReadOnlyDocument?>
    {
        public override bool HandleNull => true;

        public override KdlReadOnlyDocument Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            return KdlReadOnlyDocument.ParseValue(ref reader);
        }

        public override void Write(
            KdlWriter writer,
            KdlReadOnlyDocument? value,
            KdlSerializerOptions options
        )
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            value.WriteTo(writer);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => KdlSchema.CreateTrueSchema();
    }
}
