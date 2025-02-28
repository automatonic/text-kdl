using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Reflection;
using Automatonic.Text.Kdl.Serialization.Converters;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Converter for streaming <see cref="IAsyncEnumerable{T}" /> values.
    /// </summary>
    [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class IAsyncEnumerableConverterFactory : KdlConverterFactory
    {
        public IAsyncEnumerableConverterFactory() { }

        public override bool CanConvert(Type typeToConvert) => GetAsyncEnumerableInterface(typeToConvert) is not null;

        public override KdlConverter CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            Type? asyncEnumerableInterface = GetAsyncEnumerableInterface(typeToConvert);
            Debug.Assert(asyncEnumerableInterface is not null, $"{typeToConvert} not supported by converter.");

            Type elementType = asyncEnumerableInterface.GetGenericArguments()[0];
            Type converterType = typeof(IAsyncEnumerableOfTConverter<,>).MakeGenericType(typeToConvert, elementType);
            return (KdlConverter)Activator.CreateInstance(converterType)!;
        }

        private static Type? GetAsyncEnumerableInterface(Type type)
            => type.GetCompatibleGenericInterface(typeof(IAsyncEnumerable<>));
    }
}
