using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlNodeConverterFactory : KdlConverterFactory
    {
        public override KdlConverter? CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            if (typeof(KdlValue).IsAssignableFrom(typeToConvert))
            {
                return KdlNodeConverter.ValueConverter;
            }

            if (typeof(KdlObject) == typeToConvert)
            {
                return KdlNodeConverter.ObjectConverter;
            }

            if (typeof(KdlArray) == typeToConvert)
            {
                return KdlNodeConverter.ArrayConverter;
            }

            Debug.Assert(typeof(KdlNode) == typeToConvert);
            return KdlNodeConverter.Instance;
        }

        public override bool CanConvert(Type typeToConvert) => typeof(KdlNode).IsAssignableFrom(typeToConvert);
    }
}
