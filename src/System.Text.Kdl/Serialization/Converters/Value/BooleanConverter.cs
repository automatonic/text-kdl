// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Text;
using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class BooleanConverter : KdlPrimitiveConverter<bool>
    {
        public override bool Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetBoolean();
        }

        public override void Write(KdlWriter writer, bool value, KdlSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }

        internal override bool ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            ReadOnlySpan<byte> propertyName = reader.GetUnescapedSpan();
            if (!(Utf8Parser.TryParse(propertyName, out bool value, out int bytesConsumed)
                  && propertyName.Length == bytesConsumed))
            {
                ThrowHelper.ThrowFormatException(DataType.Boolean);
            }

            return value;
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, bool value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.Boolean };
    }
}
