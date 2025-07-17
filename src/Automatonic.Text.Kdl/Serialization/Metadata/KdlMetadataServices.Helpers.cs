using System.Diagnostics;
using System.Reflection;
using Automatonic.Text.Kdl.Serialization.Converters;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    public static partial class KdlMetadataServices
    {
        /// <summary>
        /// Creates serialization metadata for a type using a simple converter.
        /// </summary>
        private static KdlTypeInfo<T> CreateCore<T>(
            KdlConverter converter,
            KdlSerializerOptions options
        )
        {
            var typeInfo = new KdlTypeInfo<T>(converter, options);
            typeInfo.PopulatePolymorphismMetadata();
            typeInfo.MapInterfaceTypesToCallbacks();

            // Plug in any converter configuration -- should be run last.
            converter.ConfigureKdlTypeInfo(typeInfo, options);
            typeInfo.IsCustomized = false;
            return typeInfo;
        }

        /// <summary>
        /// Creates serialization metadata for an object.
        /// </summary>
        private static KdlTypeInfo<T> CreateCore<T>(
            KdlSerializerOptions options,
            KdlObjectInfoValues<T> objectInfo
        )
        {
            KdlConverter<T> converter = GetConverter(objectInfo);
            var typeInfo = new KdlTypeInfo<T>(converter, options);
            if (objectInfo.ObjectWithParameterizedConstructorCreator != null)
            {
                // NB parameter metadata must be populated *before* property metadata
                // so that properties can be linked to their associated parameters.
                typeInfo.CreateObjectWithArgs =
                    objectInfo.ObjectWithParameterizedConstructorCreator;
                PopulateParameterInfoValues(
                    typeInfo,
                    objectInfo.ConstructorParameterMetadataInitializer
                );
            }
            else
            {
                typeInfo.SetCreateObjectIfCompatible(objectInfo.ObjectCreator);
                typeInfo.CreateObjectForExtensionDataProperty = (
                    (KdlTypeInfo)typeInfo
                ).CreateObject;
            }

            if (objectInfo.PropertyMetadataInitializer != null)
            {
                typeInfo.SourceGenDelayedPropertyInitializer =
                    objectInfo.PropertyMetadataInitializer;
            }
            else
            {
                typeInfo.PropertyMetadataSerializationNotSupported = true;
            }

            typeInfo.ConstructorAttributeProviderFactory =
                objectInfo.ConstructorAttributeProviderFactory;
            typeInfo.SerializeHandler = objectInfo.SerializeHandler;
            typeInfo.NumberHandling = objectInfo.NumberHandling;
            typeInfo.PopulatePolymorphismMetadata();
            typeInfo.MapInterfaceTypesToCallbacks();

            // Plug in any converter configuration -- should be run last.
            converter.ConfigureKdlTypeInfo(typeInfo, options);
            typeInfo.IsCustomized = false;
            return typeInfo;
        }

        /// <summary>
        /// Creates serialization metadata for a collection.
        /// </summary>
        private static KdlTypeInfo<T> CreateCore<T>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<T> collectionInfo,
            KdlConverter<T> converter,
            object? createObjectWithArgs = null,
            object? addFunc = null
        )
        {
            if (collectionInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(collectionInfo));
            }

            converter =
                collectionInfo.SerializeHandler != null
                    ? new KdlMetadataServicesConverter<T>(converter)
                    : converter;

            KdlTypeInfo<T> typeInfo = new KdlTypeInfo<T>(converter, options)
            {
                KeyTypeInfo = collectionInfo.KeyInfo,
                ElementTypeInfo = collectionInfo.ElementInfo,
            };
            Debug.Assert(typeInfo.Kind != KdlTypeInfoKind.None);
            typeInfo.NumberHandling = collectionInfo.NumberHandling;
            typeInfo.SerializeHandler = collectionInfo.SerializeHandler;
            typeInfo.CreateObjectWithArgs = createObjectWithArgs;
            typeInfo.AddMethodDelegate = addFunc;
            typeInfo.SetCreateObjectIfCompatible(collectionInfo.ObjectCreator);
            typeInfo.PopulatePolymorphismMetadata();
            typeInfo.MapInterfaceTypesToCallbacks();

            // Plug in any converter configuration -- should be run last.
            converter.ConfigureKdlTypeInfo(typeInfo, options);
            typeInfo.IsCustomized = false;
            return typeInfo;
        }

        private static KdlConverter<T> GetConverter<T>(KdlObjectInfoValues<T> objectInfo)
        {
#pragma warning disable CS8714 // Nullability of type argument 'T' doesn't match 'notnull' constraint.
            KdlConverter<T> converter =
                objectInfo.ObjectWithParameterizedConstructorCreator != null
                    ? new LargeObjectWithParameterizedConstructorConverter<T>()
                    : new ObjectDefaultConverter<T>();
#pragma warning restore CS8714

            return objectInfo.SerializeHandler != null
                ? new KdlMetadataServicesConverter<T>(converter)
                : converter;
        }

        private static void PopulateParameterInfoValues(
            KdlTypeInfo typeInfo,
            Func<KdlParameterInfoValues[]?>? paramFactory
        )
        {
            Debug.Assert(typeInfo.Kind is KdlTypeInfoKind.Object);
            Debug.Assert(!typeInfo.IsReadOnly);

            if (paramFactory?.Invoke() is KdlParameterInfoValues[] parameterInfoValues)
            {
                typeInfo.PopulateParameterInfoValues(parameterInfoValues);
            }
            else
            {
                typeInfo.PropertyMetadataSerializationNotSupported = true;
            }
        }

        internal static void PopulateProperties(
            KdlTypeInfo typeInfo,
            KdlTypeInfo.KdlPropertyInfoList propertyList,
            Func<KdlSerializerContext, KdlPropertyInfo[]> propInitFunc
        )
        {
            Debug.Assert(typeInfo.Kind is KdlTypeInfoKind.Object);
            Debug.Assert(!typeInfo.IsConfigured);
            Debug.Assert(typeInfo.Type != KdlTypeInfo.ObjectType);
            Debug.Assert(typeInfo.Converter.ElementType is null);

            KdlSerializerContext? context =
                typeInfo.Options.TypeInfoResolver as KdlSerializerContext;
            KdlPropertyInfo[] properties = propInitFunc(context!);

            // Regardless of the source generator we need to re-run the naming conflict resolution algorithm
            // at run time since it is possible that the naming policy or other configs can be different then.
            KdlTypeInfo.PropertyHierarchyResolutionState state = new(typeInfo.Options);

            foreach (KdlPropertyInfo kdlPropertyInfo in properties)
            {
                if (!kdlPropertyInfo.SrcGen_IsPublic)
                {
                    if (kdlPropertyInfo.SrcGen_HasKdlInclude)
                    {
                        Debug.Assert(
                            kdlPropertyInfo.MemberName != null,
                            "MemberName is not set by source gen"
                        );
                        ThrowHelper.ThrowInvalidOperationException_KdlIncludeOnInaccessibleProperty(
                            kdlPropertyInfo.MemberName,
                            kdlPropertyInfo.DeclaringType
                        );
                    }

                    continue;
                }

                if (
                    kdlPropertyInfo.MemberType == MemberTypes.Field
                    && !kdlPropertyInfo.SrcGen_HasKdlInclude
                    && !typeInfo.Options.IncludeFields
                )
                {
                    continue;
                }

                propertyList.AddPropertyWithConflictResolution(kdlPropertyInfo, ref state);
            }

            if (state.IsPropertyOrderSpecified)
            {
                propertyList.SortProperties();
            }
        }

        private static KdlPropertyInfo<T> CreatePropertyInfoCore<T>(
            KdlPropertyInfoValues<T> propertyInfoValues,
            KdlSerializerOptions options
        )
        {
            var propertyInfo = new KdlPropertyInfo<T>(
                propertyInfoValues.DeclaringType,
                declaringTypeInfo: null,
                options
            );

            DeterminePropertyName(
                propertyInfo,
                declaredPropertyName: propertyInfoValues.PropertyName,
                declaredKdlPropertyName: propertyInfoValues.KdlPropertyName
            );

            propertyInfo.MemberName = propertyInfoValues.PropertyName;
            propertyInfo.MemberType = propertyInfoValues.IsProperty
                ? MemberTypes.Property
                : MemberTypes.Field;
            propertyInfo.SrcGen_IsPublic = propertyInfoValues.IsPublic;
            propertyInfo.SrcGen_HasKdlInclude = propertyInfoValues.HasKdlInclude;
            propertyInfo.IsExtensionData = propertyInfoValues.IsExtensionData;
            propertyInfo.CustomConverter = propertyInfoValues.Converter;

            if (propertyInfo.IgnoreCondition != KdlIgnoreCondition.Always)
            {
                propertyInfo.Get = propertyInfoValues.Getter!;
                propertyInfo.Set = propertyInfoValues.Setter;
            }

            propertyInfo.IgnoreCondition = propertyInfoValues.IgnoreCondition;
            propertyInfo.KdlTypeInfo = propertyInfoValues.PropertyTypeInfo;
            propertyInfo.NumberHandling = propertyInfoValues.NumberHandling;
            propertyInfo.AttributeProviderFactory = propertyInfoValues.AttributeProviderFactory;

            return propertyInfo;
        }

        private static void DeterminePropertyName(
            KdlPropertyInfo propertyInfo,
            string declaredPropertyName,
            string? declaredKdlPropertyName
        )
        {
            string? name;

            // Property name settings.
            if (declaredKdlPropertyName != null)
            {
                name = declaredKdlPropertyName;
            }
            else if (propertyInfo.Options.PropertyNamingPolicy == null)
            {
                name = declaredPropertyName;
            }
            else
            {
                name = propertyInfo.Options.PropertyNamingPolicy.ConvertName(declaredPropertyName);
            }

            // Compat: We need to do validation before we assign Name so that we get InvalidOperationException rather than ArgumentNullException
            if (name == null)
            {
                ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameNull(propertyInfo);
            }

            propertyInfo.Name = name;
        }
    }
}
