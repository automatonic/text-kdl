using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Kdl.Reflection;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter factory for all object-based types (non-enumerable and non-primitive).
    /// </summary>
    [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class ObjectConverterFactory : KdlConverterFactory
    {
        // Need to toggle this behavior when generating converters for F# struct records.
        private readonly bool _useDefaultConstructorInUnannotatedStructs;

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        public ObjectConverterFactory(bool useDefaultConstructorInUnannotatedStructs = true)
        {
            _useDefaultConstructorInUnannotatedStructs = useDefaultConstructorInUnannotatedStructs;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            // This is the last built-in factory converter, so if the IEnumerableConverterFactory doesn't
            // support it, then it is not IEnumerable.
            Debug.Assert(!typeof(IEnumerable).IsAssignableFrom(typeToConvert));
            return true;
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "The ctor is marked RequiresUnreferencedCode.")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern",
            Justification = "The ctor is marked RequiresUnreferencedCode.")]
        public override KdlConverter CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            KdlConverter converter;
            Type converterType;

            bool useDefaultConstructorInUnannotatedStructs = _useDefaultConstructorInUnannotatedStructs && !typeToConvert.IsKeyValuePair();
            if (!typeToConvert.TryGetDeserializationConstructor(useDefaultConstructorInUnannotatedStructs, out ConstructorInfo? constructor))
            {
                ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute<KdlConstructorAttribute>(typeToConvert);
            }

            ParameterInfo[]? parameters = constructor?.GetParameters();

            if (constructor == null || typeToConvert.IsAbstract || parameters!.Length == 0)
            {
                converterType = typeof(ObjectDefaultConverter<>).MakeGenericType(typeToConvert);
            }
            else
            {
                int parameterCount = parameters.Length;

                foreach (ParameterInfo parameter in parameters)
                {
                    // Every argument must be of supported type.
                    KdlTypeInfo.ValidateType(parameter.ParameterType);
                }

                if (parameterCount <= KdlConstants.UnboxedParameterCountThreshold)
                {
                    Type placeHolderType = KdlTypeInfo.ObjectType;
                    Type[] typeArguments = new Type[KdlConstants.UnboxedParameterCountThreshold + 1];

                    typeArguments[0] = typeToConvert;
                    for (int i = 0; i < KdlConstants.UnboxedParameterCountThreshold; i++)
                    {
                        if (i < parameterCount)
                        {
                            typeArguments[i + 1] = parameters[i].ParameterType;
                        }
                        else
                        {
                            // Use placeholder arguments if there are less args than the threshold.
                            typeArguments[i + 1] = placeHolderType;
                        }
                    }

                    converterType = typeof(SmallObjectWithParameterizedConstructorConverter<,,,,>).MakeGenericType(typeArguments);
                }
                else
                {
                    converterType = typeof(LargeObjectWithParameterizedConstructorConverterWithReflection<>).MakeGenericType(typeToConvert);
                }
            }

            converter = (KdlConverter)Activator.CreateInstance(
                    converterType,
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: null,
                    culture: null)!;

            converter.ConstructorInfo = constructor!;
            return converter;
        }
    }
}
