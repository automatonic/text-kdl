using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.RandomAccess;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed partial class KdlNodeConverter
    {
        // public override void Write(KdlWriter writer, KdlNode? value, KdlSerializerOptions options)
        // {
        //     if (value is null)
        //     {
        //         writer.WriteNullValue();
        //         return;
        //     }

        //     value.WriteTo(writer, options);
        // }

        // public override KdlNode? Read(
        //     ref KdlReader reader,
        //     Type typeToConvert,
        //     KdlSerializerOptions options
        // )
        // {
        //     switch (reader.TokenType)
        //     {
        //         case KdlTokenType.StartChildrenBlock:
        //             return ReadList(ref reader, options.GetNodeOptions());
        //         case KdlTokenType.Null:
        //             return null;
        //         default:
        //             Debug.Assert(false);
        //             throw ThrowHelper.GetInvalidOperationException_ExpectedArray(reader.TokenType);
        //     }
        // }

        public static KdlNode ReadList(ref KdlReader reader, KdlElementOptions? options = null)
        {
            KdlReadOnlyElement kroElement = KdlReadOnlyElement.ParseValue(ref reader);
            return new KdlNode(kroElement, options);
        }

        // internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
        //     new() { Type = KdlSchemaType.Node };
    }
}
