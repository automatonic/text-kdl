using System.Diagnostics;
using Automatonic.Text.Kdl.Graph;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class KdlNodeConverterFactory : KdlConverterFactory
    {
        public override KdlConverter? CreateConverter(
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            if (typeof(KdlValue).IsAssignableFrom(typeToConvert))
            {
                return KdlElementConverter.ValueConverter;
            }

            if (typeof(KdlNode) == typeToConvert)
            {
                return KdlElementConverter.NodeConverter;
            }

            Debug.Assert(typeof(KdlElement) == typeToConvert);
            return KdlElementConverter.Instance;
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeof(KdlElement).IsAssignableFrom(typeToConvert);
    }
}
