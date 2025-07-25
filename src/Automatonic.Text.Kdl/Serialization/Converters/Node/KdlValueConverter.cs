﻿using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
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

        public override KdlValue? Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            if (reader.TokenType is KdlTokenType.Null)
            {
                return null;
            }

            KdlReadOnlyElement element = KdlReadOnlyElement.ParseValue(ref reader);
            return KdlValue.CreateFromElement(ref element, options.GetNodeOptions());
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => KdlSchema.CreateTrueSchema();
    }
}
