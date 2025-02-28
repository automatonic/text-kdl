using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Automatonic.Text.Kdl.Reflection;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class NullableConverterFactory : KdlConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsNullableOfT();
        }

        public override KdlConverter CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(typeToConvert.IsNullableOfT());

            Type valueTypeToConvert = typeToConvert.GetGenericArguments()[0];
            KdlConverter valueConverter = options.GetConverterInternal(valueTypeToConvert);

            // If the value type has an interface or object converter, just return that converter directly.
            if (!valueConverter.Type!.IsValueType && valueTypeToConvert.IsValueType)
            {
                return valueConverter;
            }

            return CreateValueConverter(valueTypeToConvert, valueConverter);
        }

        public static KdlConverter CreateValueConverter(Type valueTypeToConvert, KdlConverter valueConverter)
        {
            Debug.Assert(valueTypeToConvert.IsValueType && !valueTypeToConvert.IsNullableOfT());
            return (KdlConverter)Activator.CreateInstance(
                GetNullableConverterType(valueTypeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [valueConverter],
                culture: null)!;
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2071:UnrecognizedReflectionPattern",
            Justification = "'NullableConverter<T> where T : struct' implies 'T : new()', so the trimmer is warning calling MakeGenericType here because valueTypeToConvert's constructors are not annotated. " +
            "But NullableConverter doesn't call new T(), so this is safe.")]
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        private static Type GetNullableConverterType(Type valueTypeToConvert) => typeof(NullableConverter<>).MakeGenericType(valueTypeToConvert);
    }
}
