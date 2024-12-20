// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Kdl.Schema;
using System.Text.Kdl.Nodes;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlDocumentConverter : KdlConverter<KdlDocument?>
    {
        public override bool HandleNull => true;

        public override KdlDocument Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return KdlDocument.ParseValue(ref reader);
        }

        public override void Write(KdlWriter writer, KdlDocument? value, KdlSerializerOptions options)
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
