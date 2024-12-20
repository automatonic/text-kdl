using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlArrayConverter : KdlConverter<KdlArray?>
    {
        public override void Write(KdlWriter writer, KdlArray? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            value.WriteTo(writer, options);
        }

        public override KdlArray? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
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

        public static KdlArray ReadList(ref KdlReader reader, KdlNodeOptions? options = null)
        {
            KdlElement jElement = KdlElement.ParseValue(ref reader);
            return new KdlArray(jElement, options);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new() { Type = KdlSchemaType.Array };
    }
}
