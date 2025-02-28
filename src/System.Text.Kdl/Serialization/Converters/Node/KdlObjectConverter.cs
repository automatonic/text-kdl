using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlObjectConverter : KdlConverter<KdlNode?>
    {
        internal override void ConfigureKdlTypeInfo(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            jsonTypeInfo.CreateObjectForExtensionDataProperty = () => new KdlNode(options.GetNodeOptions());
        }

        internal override void ReadElementAndSetProperty(
            object obj,
            string propertyName,
            ref KdlReader reader,
            KdlSerializerOptions options,
            scoped ref ReadStack state)
        {
            bool success = KdlVertexConverter.Instance.TryRead(ref reader, typeof(KdlElement), options, ref state, out KdlElement? value, out _);
            Debug.Assert(success); // Node converters are not resumable.

            Debug.Assert(obj is KdlNode);
            KdlNode jObject = (KdlNode)obj;
            jObject[propertyName] = value;
        }

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
                case KdlTokenType.StartObject:
                    return ReadObject(ref reader, options.GetNodeOptions());
                case KdlTokenType.Null:
                    return null;
                default:
                    Debug.Assert(false);
                    throw ThrowHelper.GetInvalidOperationException_ExpectedObject(reader.TokenType);
            }
        }

        public static KdlNode ReadObject(ref KdlReader reader, KdlElementOptions? options)
        {
            KdlReadOnlyElement jElement = KdlReadOnlyElement.ParseValue(ref reader);
            KdlNode jObject = new KdlNode(jElement, options);
            return jObject;
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.Object };
    }
}
