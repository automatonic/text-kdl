// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Kdl.Schema;
using System.Text.Kdl.Nodes;

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
