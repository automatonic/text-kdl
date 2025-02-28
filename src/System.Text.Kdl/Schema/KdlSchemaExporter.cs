using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Schema
{
    /// <summary>
    /// Functionality for exporting KDL schema from serialization contracts defined in <see cref="KdlTypeInfo"/>.
    /// </summary>
    public static class KdlSchemaExporter
    {
        /// <summary>
        /// Gets the KDL schema for <paramref name="type"/> as a <see cref="KdlElement"/> document.
        /// </summary>
        /// <param name="options">The options declaring the contract for the type.</param>
        /// <param name="type">The type for which to resolve a schema.</param>
        /// <param name="exporterOptions">The options object governing the export operation.</param>
        /// <returns>A KDL object containing the schema for <paramref name="type"/>.</returns>
        public static KdlElement GetKdlSchemaAsNode(this KdlSerializerOptions options, Type type, KdlSchemaExporterOptions? exporterOptions = null)
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            if (type is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(type));
            }

            ValidateOptions(options);
            KdlTypeInfo typeInfo = options.GetTypeInfoInternal(type);
            return typeInfo.GetKdlSchemaAsNode(exporterOptions);
        }

        /// <summary>
        /// Gets the KDL schema for <paramref name="typeInfo"/> as a <see cref="KdlElement"/> document.
        /// </summary>
        /// <param name="typeInfo">The contract from which to resolve the KDL schema.</param>
        /// <param name="exporterOptions">The options object governing the export operation.</param>
        /// <returns>A KDL object containing the schema for <paramref name="typeInfo"/>.</returns>
        public static KdlElement GetKdlSchemaAsNode(this KdlTypeInfo typeInfo, KdlSchemaExporterOptions? exporterOptions = null)
        {
            if (typeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(typeInfo));
            }

            ValidateOptions(typeInfo.Options);
            exporterOptions ??= KdlSchemaExporterOptions.Default;

            typeInfo.EnsureConfigured();
            GenerationState state = new(typeInfo.Options, exporterOptions);
            KdlSchema schema = MapKdlSchemaCore(ref state, typeInfo);
            return schema.ToKdlNode(exporterOptions);
        }

        private static KdlSchema MapKdlSchemaCore(
            ref GenerationState state,
            KdlTypeInfo typeInfo,
            KdlPropertyInfo? propertyInfo = null,
            KdlConverter? customConverter = null,
            KdlNumberHandling? customNumberHandling = null,
            KdlTypeInfo? parentPolymorphicTypeInfo = null,
            bool parentPolymorphicTypeContainsTypesWithoutDiscriminator = false,
            bool parentPolymorphicTypeIsNonNullable = false,
            KeyValuePair<string, KdlSchema>? typeDiscriminator = null,
            bool cacheResult = true)
        {
            Debug.Assert(typeInfo.IsConfigured);

            KdlSchemaExporterContext exporterContext = state.CreateContext(typeInfo, propertyInfo, parentPolymorphicTypeInfo);

            if (cacheResult && typeInfo.Kind is not KdlTypeInfoKind.None &&
                state.TryGetExistingKdlPointer(exporterContext, out string? existingKdlPointer))
            {
                // The schema context has already been generated in the schema document, return a reference to it.
                return CompleteSchema(ref state, new KdlSchema { Ref = existingKdlPointer });
            }

            KdlConverter effectiveConverter = customConverter ?? typeInfo.Converter;
            KdlNumberHandling effectiveNumberHandling = customNumberHandling ?? typeInfo.NumberHandling ?? typeInfo.Options.NumberHandling;
            if (effectiveConverter.GetSchema(effectiveNumberHandling) is { } schema)
            {
                // A schema has been provided by the converter.
                return CompleteSchema(ref state, schema);
            }

            if (parentPolymorphicTypeInfo is null && typeInfo.PolymorphismOptions is { DerivedTypes.Count: > 0 } polyOptions)
            {
                // This is the base type of a polymorphic type hierarchy. The schema for this type
                // will include an "anyOf" property with the schemas for all derived types.
                string typeDiscriminatorKey = polyOptions.TypeDiscriminatorPropertyName;
                List<KdlDerivedType> derivedTypes = [.. polyOptions.DerivedTypes];

                if (!typeInfo.Type.IsAbstract && !IsPolymorphicTypeThatSpecifiesItselfAsDerivedType(typeInfo))
                {
                    // For non-abstract base types that haven't been explicitly configured,
                    // add a trivial schema to the derived types since we should support it.
                    derivedTypes.Add(new KdlDerivedType(typeInfo.Type));
                }

                bool containsTypesWithoutDiscriminator = derivedTypes.Exists(static derivedTypes => derivedTypes.TypeDiscriminator is null);
                KdlSchemaType schemaType = KdlSchemaType.Any;
                List<KdlSchema>? anyOf = new(derivedTypes.Count);

                state.PushSchemaNode(KdlSchema.AnyOfPropertyName);

                foreach (KdlDerivedType derivedType in derivedTypes)
                {
                    Debug.Assert(derivedType.TypeDiscriminator is null or int or string);

                    KeyValuePair<string, KdlSchema>? derivedTypeDiscriminator = null;
                    if (derivedType.TypeDiscriminator is { } discriminatorValue)
                    {
                        KdlElement discriminatorNode = discriminatorValue switch
                        {
                            string stringId => (KdlElement)stringId,
                            _ => (KdlElement)(int)discriminatorValue,
                        };

                        KdlSchema discriminatorSchema = new() { Constant = discriminatorNode };
                        derivedTypeDiscriminator = new(typeDiscriminatorKey, discriminatorSchema);
                    }

                    KdlTypeInfo derivedTypeInfo = typeInfo.Options.GetTypeInfoInternal(derivedType.DerivedType);

                    state.PushSchemaNode(anyOf.Count.ToString(CultureInfo.InvariantCulture));
                    KdlSchema derivedSchema = MapKdlSchemaCore(
                        ref state,
                        derivedTypeInfo,
                        parentPolymorphicTypeInfo: typeInfo,
                        typeDiscriminator: derivedTypeDiscriminator,
                        parentPolymorphicTypeContainsTypesWithoutDiscriminator: containsTypesWithoutDiscriminator,
                        parentPolymorphicTypeIsNonNullable: propertyInfo is { IsGetNullable: false, IsSetNullable: false },
                        cacheResult: false);

                    state.PopSchemaNode();

                    // Determine if all derived schemas have the same type.
                    if (anyOf.Count == 0)
                    {
                        schemaType = derivedSchema.Type;
                    }
                    else if (schemaType != derivedSchema.Type)
                    {
                        schemaType = KdlSchemaType.Any;
                    }

                    anyOf.Add(derivedSchema);
                }

                state.PopSchemaNode();

                if (schemaType is not KdlSchemaType.Any)
                {
                    // If all derived types have the same schema type, we can simplify the schema
                    // by moving the type keyword to the base schema and removing it from the derived schemas.
                    foreach (KdlSchema derivedSchema in anyOf)
                    {
                        derivedSchema.Type = KdlSchemaType.Any;

                        if (derivedSchema.KeywordCount == 0)
                        {
                            // if removing the type results in an empty schema,
                            // remove the anyOf array entirely since it's always true.
                            anyOf = null;
                            break;
                        }
                    }
                }

                return CompleteSchema(ref state, new()
                {
                    Type = schemaType,
                    AnyOf = anyOf,
                    // If all derived types have a discriminator, we can require it in the base schema.
                    Required = containsTypesWithoutDiscriminator ? null : [typeDiscriminatorKey]
                });
            }

            if (effectiveConverter.NullableElementConverter is { } elementConverter)
            {
                KdlTypeInfo elementTypeInfo = typeInfo.Options.GetTypeInfo(elementConverter.Type!);
                schema = MapKdlSchemaCore(ref state, elementTypeInfo, customConverter: elementConverter, cacheResult: false);

                if (schema.Enum != null)
                {
                    Debug.Assert(elementTypeInfo.Type.IsEnum, "The enum keyword should only be populated by schemas for enum types.");
                    schema.Enum.Add("", null); // Append null to the enum array.
                }

                return CompleteSchema(ref state, schema);
            }

            switch (typeInfo.Kind)
            {
                case KdlTypeInfoKind.Object:
                    List<KeyValuePair<string, KdlSchema>>? properties = null;
                    List<string>? required = null;
                    KdlSchema? additionalProperties = null;

                    KdlUnmappedMemberHandling effectiveUnmappedMemberHandling = typeInfo.UnmappedMemberHandling ?? typeInfo.Options.UnmappedMemberHandling;
                    if (effectiveUnmappedMemberHandling is KdlUnmappedMemberHandling.Disallow)
                    {
                        additionalProperties = KdlSchema.CreateFalseSchema();
                    }

                    if (typeDiscriminator is { } typeDiscriminatorPair)
                    {
                        (properties ??= []).Add(typeDiscriminatorPair);
                        if (parentPolymorphicTypeContainsTypesWithoutDiscriminator)
                        {
                            // Require the discriminator here since it's not common to all derived types.
                            (required ??= []).Add(typeDiscriminatorPair.Key);
                        }
                    }

                    state.PushSchemaNode(KdlSchema.PropertiesPropertyName);
                    foreach (KdlPropertyInfo property in typeInfo.Properties)
                    {
                        if (property is { Get: null, Set: null } or { IsExtensionData: true })
                        {
                            continue; // Skip KdlIgnored properties and extension data
                        }

                        state.PushSchemaNode(property.Name);
                        KdlSchema propertySchema = MapKdlSchemaCore(
                            ref state,
                            property.KdlTypeInfo,
                            propertyInfo: property,
                            customConverter: property.EffectiveConverter,
                            customNumberHandling: property.EffectiveNumberHandling);

                        state.PopSchemaNode();

                        if (property.AssociatedParameter is { HasDefaultValue: true } parameterInfo)
                        {
                            KdlSchema.EnsureMutable(ref propertySchema);
                            propertySchema.DefaultValue = KdlSerializer.SerializeToNode(parameterInfo.DefaultValue, property.KdlTypeInfo);
                            propertySchema.HasDefaultValue = true;
                        }

                        (properties ??= []).Add(new(property.Name, propertySchema));

                        // Mark as required if either the property is required or the associated constructor parameter is non-optional.
                        // While the latter implies the former in cases where the KdlSerializerOptions.RespectRequiredConstructorParameters
                        // setting has been enabled, for the case of the schema exporter we always mark non-optional constructor parameters as required.
                        if (property is { IsRequired: true } or { AssociatedParameter.IsRequiredParameter: true })
                        {
                            (required ??= []).Add(property.Name);
                        }
                    }

                    state.PopSchemaNode();
                    return CompleteSchema(ref state, new()
                    {
                        Type = KdlSchemaType.Object,
                        Properties = properties,
                        Required = required,
                        AdditionalProperties = additionalProperties,
                    });

                case KdlTypeInfoKind.Enumerable:
                    Debug.Assert(typeInfo.ElementTypeInfo != null);

                    if (typeDiscriminator is null)
                    {
                        state.PushSchemaNode(KdlSchema.ItemsPropertyName);
                        KdlSchema items = MapKdlSchemaCore(ref state, typeInfo.ElementTypeInfo, customNumberHandling: effectiveNumberHandling);
                        state.PopSchemaNode();

                        return CompleteSchema(ref state, new()
                        {
                            Type = KdlSchemaType.Array,
                            Items = items.IsTrue ? null : items,
                        });
                    }
                    else
                    {
                        // Polymorphic enumerable types are represented using a wrapping object:
                        // { "$type" : "discriminator", "$values" : [element1, element2, ...] }
                        // Which corresponds to the schema
                        // { "properties" : { "$type" : { "const" : "discriminator" }, "$values" : { "type" : "array", "items" : { ... } } } }
                        const string ValuesKeyword = KdlSerializer.ValuesPropertyName;

                        state.PushSchemaNode(KdlSchema.PropertiesPropertyName);
                        state.PushSchemaNode(ValuesKeyword);
                        state.PushSchemaNode(KdlSchema.ItemsPropertyName);

                        KdlSchema items = MapKdlSchemaCore(ref state, typeInfo.ElementTypeInfo, customNumberHandling: effectiveNumberHandling);

                        state.PopSchemaNode();
                        state.PopSchemaNode();
                        state.PopSchemaNode();

                        return CompleteSchema(ref state, new()
                        {
                            Type = KdlSchemaType.Object,
                            Properties =
                            [
                                typeDiscriminator.Value,
                                new(ValuesKeyword,
                                    new KdlSchema()
                                    {
                                        Type = KdlSchemaType.Array,
                                        Items = items.IsTrue ? null : items,
                                    }),
                            ],
                            Required = parentPolymorphicTypeContainsTypesWithoutDiscriminator ? [typeDiscriminator.Value.Key] : null,
                        });
                    }

                case KdlTypeInfoKind.Dictionary:
                    Debug.Assert(typeInfo.ElementTypeInfo != null);

                    List<KeyValuePair<string, KdlSchema>>? dictProps = null;
                    List<string>? dictRequired = null;

                    if (typeDiscriminator is { } dictDiscriminator)
                    {
                        dictProps = [dictDiscriminator];
                        if (parentPolymorphicTypeContainsTypesWithoutDiscriminator)
                        {
                            // Require the discriminator here since it's not common to all derived types.
                            dictRequired = [dictDiscriminator.Key];
                        }
                    }

                    state.PushSchemaNode(KdlSchema.AdditionalPropertiesPropertyName);
                    KdlSchema valueSchema = MapKdlSchemaCore(ref state, typeInfo.ElementTypeInfo, customNumberHandling: effectiveNumberHandling);
                    state.PopSchemaNode();

                    return CompleteSchema(ref state, new()
                    {
                        Type = KdlSchemaType.Object,
                        Properties = dictProps,
                        Required = dictRequired,
                        AdditionalProperties = valueSchema.IsTrue ? null : valueSchema,
                    });

                default:
                    Debug.Assert(typeInfo.Kind is KdlTypeInfoKind.None);
                    // Return a `true` schema for types with user-defined converters.
                    return CompleteSchema(ref state, KdlSchema.CreateTrueSchema());
            }

            KdlSchema CompleteSchema(ref GenerationState state, KdlSchema schema)
            {
                if (schema.Ref is null)
                {
                    // A schema is marked as nullable if either
                    // 1. We have a schema for a property where either the getter or setter are marked as nullable.
                    // 2. We have a schema for a reference type, unless we're explicitly treating null-oblivious types as non-nullable.
                    bool isNullableSchema = propertyInfo != null
                        ? propertyInfo.IsGetNullable || propertyInfo.IsSetNullable
                        : typeInfo.CanBeNull && !parentPolymorphicTypeIsNonNullable && !state.ExporterOptions.TreatNullObliviousAsNonNullable;

                    if (isNullableSchema)
                    {
                        schema.MakeNullable();
                    }
                }

                if (state.ExporterOptions.TransformSchemaNode != null)
                {
                    // Prime the schema for invocation by the KdlVertex transformer.
                    schema.ExporterContext = exporterContext;
                }

                return schema;
            }
        }

        private static void ValidateOptions(KdlSerializerOptions options)
        {
            if (options.ReferenceHandler == ReferenceHandler.Preserve)
            {
                ThrowHelper.ThrowNotSupportedException_KdlSchemaExporterDoesNotSupportReferenceHandlerPreserve();
            }

            options.MakeReadOnly();
        }

        private static bool IsPolymorphicTypeThatSpecifiesItselfAsDerivedType(KdlTypeInfo typeInfo)
        {
            Debug.Assert(typeInfo.PolymorphismOptions is not null);

            foreach (KdlDerivedType derivedType in typeInfo.PolymorphismOptions.DerivedTypes)
            {
                if (derivedType.DerivedType == typeInfo.Type)
                {
                    return true;
                }
            }

            return false;
        }

        private readonly ref struct GenerationState(KdlSerializerOptions options, KdlSchemaExporterOptions exporterOptions)
        {
            private readonly List<string> _currentPath = [];
            private readonly Dictionary<(KdlTypeInfo, KdlPropertyInfo?), string[]> _generated = [];

            public int CurrentDepth => _currentPath.Count;
            public KdlSerializerOptions Options { get; } = options;
            public KdlSchemaExporterOptions ExporterOptions { get; } = exporterOptions;

            public void PushSchemaNode(string nodeId)
            {
                if (CurrentDepth == Options.EffectiveMaxDepth)
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlSchemaExporterDepthTooLarge();
                }

                _currentPath.Add(nodeId);
            }

            public void PopSchemaNode()
            {
                Debug.Assert(CurrentDepth > 0);
                _currentPath.RemoveAt(_currentPath.Count - 1);
            }

            /// <summary>
            /// Registers the current schema node generation context; if it has already been generated return a KDL pointer to its location.
            /// </summary>
            public bool TryGetExistingKdlPointer(in KdlSchemaExporterContext context, [NotNullWhen(true)] out string? existingKdlPointer)
            {
                (KdlTypeInfo TypeInfo, KdlPropertyInfo? PropertyInfo) key = (context.TypeInfo, context.PropertyInfo);
#if NET
                ref string[]? pathToSchema = ref CollectionsMarshal.GetValueRefOrAddDefault(_generated, key, out bool exists);
#else
                bool exists = _generated.TryGetValue(key, out string[]? pathToSchema);
#endif
                if (exists)
                {
                    existingKdlPointer = FormatKdlPointer(pathToSchema);
                    return true;
                }
#if NET
                pathToSchema = context._path;
#else
                _generated[key] = context._path;
#endif
                existingKdlPointer = null;
                return false;
            }

            public KdlSchemaExporterContext CreateContext(KdlTypeInfo typeInfo, KdlPropertyInfo? propertyInfo, KdlTypeInfo? baseTypeInfo)
            {
                return new KdlSchemaExporterContext(typeInfo, propertyInfo, baseTypeInfo, [.. _currentPath]);
            }

            private static string FormatKdlPointer(ReadOnlySpan<string> path)
            {
                if (path.IsEmpty)
                {
                    return "#";
                }

                using ValueStringBuilder sb = new(initialCapacity: path.Length * 10);
                sb.Append('#');

                foreach (string segment in path)
                {
                    ReadOnlySpan<char> span = segment.AsSpan();
                    sb.Append('/');

                    do
                    {
                        // Per RFC 6901 the characters '~' and '/' must be escaped.
                        int pos = span.IndexOfAny('~', '/');
                        if (pos < 0)
                        {
                            sb.Append(span);
                            break;
                        }

                        sb.Append(span[..pos]);

                        if (span[pos] == '~')
                        {
                            sb.Append("~0");
                        }
                        else
                        {
                            Debug.Assert(span[pos] == '/');
                            sb.Append("~1");
                        }

                        span = span[(pos + 1)..];
                    }
                    while (!span.IsEmpty);
                }

                return sb.ToString();
            }
        }
    }
}
