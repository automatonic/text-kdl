﻿using System.Diagnostics;
using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlArrayConverter : KdlConverter<KdlNode?>
    {
        public override void Write(KdlWriter writer, KdlNode? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            value.WriteTo(writer, options);
        }

        public override KdlNode? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case KdlTokenType.StartArray:
                    return ReadList(ref reader, options.GetNodeOptions());
                case KdlTokenType.Null:
                    return null;
                default:
                    Debug.Assert(false);
                    throw ThrowHelper.GetInvalidOperationException_ExpectedArray(reader.TokenType);
            }
        }

        public static KdlNode ReadList(ref KdlReader reader, KdlElementOptions? options = null)
        {
            KdlReadOnlyElement kroElement = KdlReadOnlyElement.ParseValue(ref reader);
            return new KdlNode(kroElement, options);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.Array };
    }
}
