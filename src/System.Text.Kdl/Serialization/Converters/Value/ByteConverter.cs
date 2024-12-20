// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class ByteConverter : KdlPrimitiveConverter<byte>
    {
        public ByteConverter()
        {
            IsInternalConverterForNumberType = true;
        }

        public override byte Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetByte();
        }

        public override void Write(KdlWriter writer, byte value, KdlSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }

        internal override byte ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetByteWithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, byte value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override byte ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String && (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return reader.GetByteWithQuotes();
            }

            return reader.GetByte();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, byte value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                writer.WriteNumberValueAsString(value);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Integer, numberHandling);
    }
}
