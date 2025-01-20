using System.Diagnostics;
using System.Text.Kdl.Nodes;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlNodeConverterFactory : KdlConverterFactory
    {
        public override KdlConverter? CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            if (typeof(KdlValue).IsAssignableFrom(typeToConvert))
            {
                return KdlVertexConverter.ValueConverter;
            }

            if (typeof(KdlNode) == typeToConvert)
            {
                return KdlVertexConverter.ObjectConverter;
            }

            if (typeof(KdlNode) == typeToConvert)
            {
                return KdlVertexConverter.ArrayConverter;
            }

            Debug.Assert(typeof(KdlVertex) == typeToConvert);
            return KdlVertexConverter.Instance;
        }

        public override bool CanConvert(Type typeToConvert) => typeof(KdlVertex).IsAssignableFrom(typeToConvert);
    }
}
