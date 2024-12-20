// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlObjectConverter : KdlConverter<KdlObject?>
    {
        internal override void ConfigureKdlTypeInfo(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            jsonTypeInfo.CreateObjectForExtensionDataProperty = () => new KdlObject(options.GetNodeOptions());
        }

        internal override void ReadElementAndSetProperty(
            object obj,
            string propertyName,
            ref KdlReader reader,
            KdlSerializerOptions options,
            scoped ref ReadStack state)
        {
            bool success = KdlNodeConverter.Instance.TryRead(ref reader, typeof(KdlNode), options, ref state, out KdlNode? value, out _);
            Debug.Assert(success); // Node converters are not resumable.

            Debug.Assert(obj is KdlObject);
            KdlObject jObject = (KdlObject)obj;

            Debug.Assert(value == null || value is KdlNode);
            KdlNode? jNodeValue = value;

            jObject[propertyName] = jNodeValue;
        }

        public override void Write(KdlWriter writer, KdlObject? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            value.WriteTo(writer, options);
        }

        public override KdlObject? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case KdlTokenType.StartObject:
                    return ReadObject(ref reader, options.GetNodeOptions());
                case KdlTokenType.Null:
                    return null;
                default:
                    Debug.Assert(false);
                    throw ThrowHelper.GetInvalidOperationException_ExpectedObject(reader.TokenType);
            }
        }

        public static KdlObject ReadObject(ref KdlReader reader, KdlNodeOptions? options)
        {
            KdlElement jElement = KdlElement.ParseValue(ref reader);
            KdlObject jObject = new KdlObject(jElement, options);
            return jObject;
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.Object };
    }
}
