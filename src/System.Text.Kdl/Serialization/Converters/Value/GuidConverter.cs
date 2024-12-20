// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class GuidConverter : KdlPrimitiveConverter<Guid>
    {
        public override Guid Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetGuid();
        }

        public override void Write(KdlWriter writer, Guid value, KdlSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        internal override Guid ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetGuidNoValidation();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, Guid value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            new() { Type = KdlSchemaType.String, Format = "uuid" };
    }
}
