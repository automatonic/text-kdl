using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Automatonic.Text.Kdl.Reflection;
using Automatonic.Text.Kdl.Serialization.Converters;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    public partial class DefaultKdlTypeInfoResolver
    {
        private static Dictionary<Type, KdlConverter>? s_defaultSimpleConverters;
        private static KdlConverterFactory[]? s_defaultFactoryConverters;

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlConverterFactory[] GetDefaultFactoryConverters()
        {
            return
            [
                // Check for disallowed types.
                new UnsupportedTypeConverterFactory(),
                // Nullable converter should always be next since it forwards to any nullable type.
                new NullableConverterFactory(),
                new EnumConverterFactory(),
                new KdlNodeConverterFactory(),
                new FSharpTypeConverterFactory(),
                new MemoryConverterFactory(),
                // IAsyncEnumerable takes precedence over IEnumerable.
                new IAsyncEnumerableConverterFactory(),
                // IEnumerable should always be second to last since they can convert any IEnumerable.
                new IEnumerableConverterFactory(),
                // Object should always be last since it converts any type.
                new ObjectConverterFactory(),
            ];
        }

        private static Dictionary<Type, KdlConverter> GetDefaultSimpleConverters()
        {
            const int NumberOfSimpleConverters = 31;
            var converters = new Dictionary<Type, KdlConverter>(NumberOfSimpleConverters);

            // Use a dictionary for simple converters.
            // When adding to this, update NumberOfSimpleConverters above.
            Add(KdlMetadataServices.BooleanConverter);
            Add(KdlMetadataServices.ByteConverter);
            Add(KdlMetadataServices.ByteArrayConverter);
            Add(KdlMetadataServices.CharConverter);
            Add(KdlMetadataServices.DateTimeConverter);
            Add(KdlMetadataServices.DateTimeOffsetConverter);
#if NET
            Add(KdlMetadataServices.DateOnlyConverter);
            Add(KdlMetadataServices.TimeOnlyConverter);
            Add(KdlMetadataServices.HalfConverter);
#endif
            Add(KdlMetadataServices.DoubleConverter);
            Add(KdlMetadataServices.DecimalConverter);
            Add(KdlMetadataServices.GuidConverter);
            Add(KdlMetadataServices.Int16Converter);
            Add(KdlMetadataServices.Int32Converter);
            Add(KdlMetadataServices.Int64Converter);
            Add(KdlMetadataServices.KdlElementConverter);
            Add(KdlMetadataServices.KdlDocumentConverter);
            Add(KdlMetadataServices.MemoryByteConverter);
            Add(KdlMetadataServices.ReadOnlyMemoryByteConverter);
            Add(KdlMetadataServices.ObjectConverter);
            Add(KdlMetadataServices.SByteConverter);
            Add(KdlMetadataServices.SingleConverter);
            Add(KdlMetadataServices.StringConverter);
            Add(KdlMetadataServices.TimeSpanConverter);
            Add(KdlMetadataServices.UInt16Converter);
            Add(KdlMetadataServices.UInt32Converter);
            Add(KdlMetadataServices.UInt64Converter);
#if NET
            Add(KdlMetadataServices.Int128Converter);
            Add(KdlMetadataServices.UInt128Converter);
#endif
            Add(KdlMetadataServices.UriConverter);
            Add(KdlMetadataServices.VersionConverter);

            Debug.Assert(converters.Count <= NumberOfSimpleConverters);

            return converters;

            void Add(KdlConverter converter) => converters.Add(converter.Type!, converter);
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlConverter GetBuiltInConverter(Type typeToConvert)
        {
            s_defaultSimpleConverters ??= GetDefaultSimpleConverters();
            s_defaultFactoryConverters ??= GetDefaultFactoryConverters();

            if (s_defaultSimpleConverters.TryGetValue(typeToConvert, out KdlConverter? converter))
            {
                return converter;
            }
            else
            {
                foreach (KdlConverterFactory factory in s_defaultFactoryConverters)
                {
                    if (factory.CanConvert(typeToConvert))
                    {
                        converter = factory;
                        break;
                    }
                }

                // Since the object and IEnumerable converters cover all types, we should have a converter.
                Debug.Assert(converter != null);
                return converter;
            }
        }

        internal static bool TryGetDefaultSimpleConverter(
            Type typeToConvert,
            [NotNullWhen(true)] out KdlConverter? converter
        )
        {
            if (s_defaultSimpleConverters is null)
            {
                converter = null;
                return false;
            }

            return s_defaultSimpleConverters.TryGetValue(typeToConvert, out converter);
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlConverter? GetCustomConverterForMember(
            Type typeToConvert,
            MemberInfo memberInfo,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(memberInfo is FieldInfo or PropertyInfo);
            Debug.Assert(typeToConvert != null);

            KdlConverterAttribute? converterAttribute =
                memberInfo.GetUniqueCustomAttribute<KdlConverterAttribute>(inherit: false);
            return converterAttribute is null
                ? null
                : GetConverterFromAttribute(converterAttribute, typeToConvert, memberInfo, options);
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal static KdlConverter GetConverterForType(
            Type typeToConvert,
            KdlSerializerOptions options,
            bool resolveKdlConverterAttribute = true
        )
        {
            // Priority 1: Attempt to get custom converter from the Converters list.
            KdlConverter? converter = options.GetConverterFromList(typeToConvert);

            // Priority 2: Attempt to get converter from [KdlConverter] on the type being converted.
            if (resolveKdlConverterAttribute && converter == null)
            {
                KdlConverterAttribute? converterAttribute =
                    typeToConvert.GetUniqueCustomAttribute<KdlConverterAttribute>(inherit: false);
                if (converterAttribute != null)
                {
                    converter = GetConverterFromAttribute(
                        converterAttribute,
                        typeToConvert: typeToConvert,
                        memberInfo: null,
                        options
                    );
                }
            }

            // Priority 3: Query the built-in converters.
            converter ??= GetBuiltInConverter(typeToConvert);

            // Expand if factory converter & validate.
            converter = options.ExpandConverterFactory(converter, typeToConvert);
            if (!converter.Type!.IsInSubtypeRelationshipWith(typeToConvert))
            {
                ThrowHelper.ThrowInvalidOperationException_SerializationConverterNotCompatible(
                    converter.GetType(),
                    typeToConvert
                );
            }

            KdlSerializerOptions.CheckConverterNullabilityIsSameAsPropertyType(
                converter,
                typeToConvert
            );
            return converter;
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlConverter GetConverterFromAttribute(
            KdlConverterAttribute converterAttribute,
            Type typeToConvert,
            MemberInfo? memberInfo,
            KdlSerializerOptions options
        )
        {
            KdlConverter? converter;

            Type declaringType = memberInfo?.DeclaringType ?? typeToConvert;
            Type? converterType = converterAttribute.ConverterType;
            if (converterType == null)
            {
                // Allow the attribute to create the converter.
                converter = converterAttribute.CreateConverter(typeToConvert);
                if (converter == null)
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(
                        declaringType,
                        memberInfo,
                        typeToConvert
                    );
                }
            }
            else
            {
                ConstructorInfo? ctor = converterType.GetConstructor(Type.EmptyTypes);
                if (
                    !typeof(KdlConverter).IsAssignableFrom(converterType)
                    || ctor == null
                    || !ctor.IsPublic
                )
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializationConverterOnAttributeInvalid(
                        declaringType,
                        memberInfo
                    );
                }

                converter = (KdlConverter)Activator.CreateInstance(converterType)!;
            }

            Debug.Assert(converter != null);
            if (!converter.CanConvert(typeToConvert))
            {
                Type? underlyingType = Nullable.GetUnderlyingType(typeToConvert);
                if (underlyingType != null && converter.CanConvert(underlyingType))
                {
                    if (converter is KdlConverterFactory converterFactory)
                    {
                        converter = converterFactory.GetConverterInternal(underlyingType, options);
                    }

                    // Allow nullable handling to forward to the underlying type's converter.
                    return NullableConverterFactory.CreateValueConverter(underlyingType, converter);
                }

                ThrowHelper.ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(
                    declaringType,
                    memberInfo,
                    typeToConvert
                );
            }

            return converter;
        }
    }
}
