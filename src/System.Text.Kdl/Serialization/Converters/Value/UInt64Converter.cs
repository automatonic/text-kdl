// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class UInt64Converter : KdlPrimitiveConverter<ulong>
    {
        public UInt64Converter()
        {
            IsInternalConverterForNumberType = true;
        }

        public override ulong Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetUInt64();
        }

        public override void Write(KdlWriter writer, ulong value, KdlSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }

        internal override ulong ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetUInt64WithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, ulong value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override ulong ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return reader.GetUInt64WithQuotes();
            }

            return reader.GetUInt64();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, ulong value, KdlNumberHandling handling)
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
