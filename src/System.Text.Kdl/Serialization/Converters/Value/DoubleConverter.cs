// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class DoubleConverter : KdlPrimitiveConverter<double>
    {
        public DoubleConverter()
        {
            IsInternalConverterForNumberType = true;
        }

        public override double Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return reader.GetDouble();
        }

        public override void Write(KdlWriter writer, double value, KdlSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }

        internal override double ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return reader.GetDoubleWithQuotes();
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, double value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            writer.WritePropertyName(value);
        }

        internal override double ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String)
            {
                if ((KdlNumberHandling.AllowReadingFromString & handling) != 0)
                {
                    return reader.GetDoubleWithQuotes();
                }
                else if ((KdlNumberHandling.AllowNamedFloatingPointLiterals & handling) != 0)
                {
                    return reader.GetDoubleFloatingPointConstant();
                }
            }

            return reader.GetDouble();
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, double value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                writer.WriteNumberValueAsString(value);
            }
            else if ((KdlNumberHandling.AllowNamedFloatingPointLiterals & handling) != 0)
            {
                writer.WriteFloatingPointConstant(value);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
                GetSchemaForNumericType(KdlSchemaType.Number, numberHandling, isIeeeFloatingPoint: true);
    }
}
