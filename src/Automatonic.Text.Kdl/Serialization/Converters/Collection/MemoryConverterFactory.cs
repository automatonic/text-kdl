using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class MemoryConverterFactory : KdlConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || !typeToConvert.IsValueType)
            {
                return false;
            }

            Type typeDef = typeToConvert.GetGenericTypeDefinition();
            return typeDef == typeof(Memory<>) || typeDef == typeof(ReadOnlyMemory<>);
        }

        public override KdlConverter? CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(CanConvert(typeToConvert));

            Type converterType = typeToConvert.GetGenericTypeDefinition() == typeof(Memory<>) ?
                typeof(MemoryConverter<>) : typeof(ReadOnlyMemoryConverter<>);

            Type elementType = typeToConvert.GetGenericArguments()[0];

            return (KdlConverter)Activator.CreateInstance(
                converterType.MakeGenericType(elementType))!;
        }
    }
}
