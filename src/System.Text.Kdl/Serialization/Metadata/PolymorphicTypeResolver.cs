// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Validates and indexes polymorphic type configuration,
    /// providing derived KdlTypeInfo resolution methods
    /// in both serialization and deserialization scenaria.
    /// </summary>
    internal sealed class PolymorphicTypeResolver
    {
        private readonly ConcurrentDictionary<Type, DerivedKdlTypeInfo?> _typeToDiscriminatorId = new();
        private readonly Dictionary<object, DerivedKdlTypeInfo>? _discriminatorIdtoType;
        private readonly KdlSerializerOptions _options;

        public PolymorphicTypeResolver(KdlSerializerOptions options, KdlPolymorphismOptions polymorphismOptions, Type baseType, bool converterCanHaveMetadata)
        {
            UnknownDerivedTypeHandling = polymorphismOptions.UnknownDerivedTypeHandling;
            IgnoreUnrecognizedTypeDiscriminators = polymorphismOptions.IgnoreUnrecognizedTypeDiscriminators;
            BaseType = baseType;
            _options = options;

            if (!IsSupportedPolymorphicBaseType(BaseType))
            {
                ThrowHelper.ThrowInvalidOperationException_TypeDoesNotSupportPolymorphism(BaseType);
            }

            bool containsDerivedTypes = false;
            foreach ((Type derivedType, object? typeDiscriminator) in polymorphismOptions.DerivedTypes)
            {
                Debug.Assert(typeDiscriminator is null or int or string);

                if (!IsSupportedDerivedType(BaseType, derivedType) ||
                    (derivedType.IsAbstract && UnknownDerivedTypeHandling != KdlUnknownDerivedTypeHandling.FallBackToNearestAncestor))
                {
                    ThrowHelper.ThrowInvalidOperationException_DerivedTypeNotSupported(BaseType, derivedType);
                }

                KdlTypeInfo derivedTypeInfo = options.GetTypeInfoInternal(derivedType);
                DerivedKdlTypeInfo derivedTypeInfoHolder = new(typeDiscriminator, derivedTypeInfo);

                if (!_typeToDiscriminatorId.TryAdd(derivedType, derivedTypeInfoHolder))
                {
                    ThrowHelper.ThrowInvalidOperationException_DerivedTypeIsAlreadySpecified(BaseType, derivedType);
                }

                if (typeDiscriminator is not null)
                {
                    if (!(_discriminatorIdtoType ??= new()).TryAdd(typeDiscriminator, derivedTypeInfoHolder))
                    {
                        ThrowHelper.ThrowInvalidOperationException_TypeDicriminatorIdIsAlreadySpecified(BaseType, typeDiscriminator);
                    }

                    UsesTypeDiscriminators = true;
                }

                containsDerivedTypes = true;
            }

            if (!containsDerivedTypes)
            {
                ThrowHelper.ThrowInvalidOperationException_PolymorphicTypeConfigurationDoesNotSpecifyDerivedTypes(BaseType);
            }

            if (UsesTypeDiscriminators)
            {
                Debug.Assert(_discriminatorIdtoType != null, "Discriminator index must have been populated.");

                if (!converterCanHaveMetadata)
                {
                    ThrowHelper.ThrowNotSupportedException_BaseConverterDoesNotSupportMetadata(BaseType);
                }

                string propertyName = polymorphismOptions.TypeDiscriminatorPropertyName;
                if (!propertyName.Equals(KdlSerializer.TypePropertyName, StringComparison.Ordinal))
                {
                    byte[] utf8EncodedName = Encoding.UTF8.GetBytes(propertyName);

                    // Check if the property name conflicts with other metadata property names
                    if ((KdlSerializer.GetMetadataPropertyName(utf8EncodedName, resolver: null) & ~MetadataPropertyName.Type) != 0)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidCustomTypeDiscriminatorPropertyName();
                    }

                    CustomTypeDiscriminatorPropertyNameUtf8 = utf8EncodedName;
                    CustomTypeDiscriminatorPropertyNameKdlEncoded = KdlEncodedText.Encode(propertyName, options.Encoder);
                }

                // Check if the discriminator property name conflicts with any derived property names.
                foreach (DerivedKdlTypeInfo derivedTypeInfo in _discriminatorIdtoType.Values)
                {
                    if (derivedTypeInfo.KdlTypeInfo.Kind is KdlTypeInfoKind.Object)
                    {
                        foreach (KdlPropertyInfo property in derivedTypeInfo.KdlTypeInfo.Properties)
                        {
                            if (property is { IsIgnored: false, IsExtensionData: false } && property.Name == propertyName)
                            {
                                ThrowHelper.ThrowInvalidOperationException_PropertyConflictsWithMetadataPropertyName(derivedTypeInfo.KdlTypeInfo.Type, propertyName);
                            }
                        }
                    }
                }
            }
        }

        public Type BaseType { get; }
        public KdlUnknownDerivedTypeHandling UnknownDerivedTypeHandling { get; }
        public bool UsesTypeDiscriminators { get; }
        public bool IgnoreUnrecognizedTypeDiscriminators { get; }
        public byte[]? CustomTypeDiscriminatorPropertyNameUtf8 { get; }
        public KdlEncodedText? CustomTypeDiscriminatorPropertyNameKdlEncoded { get; }

        public bool TryGetDerivedKdlTypeInfo(Type runtimeType, [NotNullWhen(true)] out KdlTypeInfo? jsonTypeInfo, out object? typeDiscriminator)
        {
            Debug.Assert(BaseType.IsAssignableFrom(runtimeType));

            if (!_typeToDiscriminatorId.TryGetValue(runtimeType, out DerivedKdlTypeInfo? result))
            {
                switch (UnknownDerivedTypeHandling)
                {
                    case KdlUnknownDerivedTypeHandling.FallBackToNearestAncestor:
                        // Calculate (and cache the result) of the nearest ancestor for given runtime type.
                        // A `null` result denotes no matching ancestor type, we also cache that.
                        result = CalculateNearestAncestor(runtimeType);
                        _typeToDiscriminatorId[runtimeType] = result;
                        break;
                    case KdlUnknownDerivedTypeHandling.FallBackToBaseType:
                        // Recover the polymorphic contract (i.e. any type discriminators) for the base type, if it exists.
                        _typeToDiscriminatorId.TryGetValue(BaseType, out result);
                        _typeToDiscriminatorId[runtimeType] = result;
                        break;

                    case KdlUnknownDerivedTypeHandling.FailSerialization:
                    default:
                        if (runtimeType != BaseType)
                        {
                            ThrowHelper.ThrowNotSupportedException_RuntimeTypeNotSupported(BaseType, runtimeType);
                        }
                        break;
                }
            }

            if (result is null)
            {
                jsonTypeInfo = null;
                typeDiscriminator = null;
                return false;
            }
            else
            {
                jsonTypeInfo = result.KdlTypeInfo;
                typeDiscriminator = result.TypeDiscriminator;
                return true;
            }
        }

        public bool TryGetDerivedKdlTypeInfo(object typeDiscriminator, [NotNullWhen(true)] out KdlTypeInfo? jsonTypeInfo)
        {
            Debug.Assert(typeDiscriminator is int or string);
            Debug.Assert(UsesTypeDiscriminators);
            Debug.Assert(_discriminatorIdtoType != null);

            if (_discriminatorIdtoType.TryGetValue(typeDiscriminator, out DerivedKdlTypeInfo? result))
            {
                Debug.Assert(typeDiscriminator.Equals(result.TypeDiscriminator));
                jsonTypeInfo = result.KdlTypeInfo;
                return true;
            }

            if (!IgnoreUnrecognizedTypeDiscriminators)
            {
                ThrowHelper.ThrowKdlException_UnrecognizedTypeDiscriminator(typeDiscriminator);
            }

            jsonTypeInfo = null;
            return false;
        }

        public static bool IsSupportedPolymorphicBaseType(Type? type) =>
            type != null &&
            (type.IsClass || type.IsInterface) &&
            !type.IsSealed &&
            !type.IsGenericTypeDefinition &&
            !type.IsPointer &&
            type != KdlTypeInfo.ObjectType;

        public static bool IsSupportedDerivedType(Type baseType, Type? derivedType) =>
            baseType.IsAssignableFrom(derivedType) && !derivedType.IsGenericTypeDefinition;

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern",
            Justification = "The call to GetInterfaces will cross-reference results with interface types " +
                            "already declared as derived types of the polymorphic base type.")]
        private DerivedKdlTypeInfo? CalculateNearestAncestor(Type type)
        {
            Debug.Assert(!type.IsAbstract);
            Debug.Assert(BaseType.IsAssignableFrom(type));
            Debug.Assert(UnknownDerivedTypeHandling == KdlUnknownDerivedTypeHandling.FallBackToNearestAncestor);

            if (type == BaseType)
            {
                return null;
            }

            DerivedKdlTypeInfo? result = null;

            // First, walk up the class hierarchy for any supported types.
            for (Type? candidate = type.BaseType; BaseType.IsAssignableFrom(candidate); candidate = candidate.BaseType)
            {
                Debug.Assert(candidate != null);

                if (_typeToDiscriminatorId.TryGetValue(candidate, out result))
                {
                    break;
                }
            }

            // Interface hierarchies admit the possibility of diamond ambiguities in type discriminators.
            // Examine all interface implementations and identify potential conflicts.
            if (BaseType.IsInterface)
            {
                foreach (Type interfaceTy in type.GetInterfaces())
                {
                    if (interfaceTy != BaseType && BaseType.IsAssignableFrom(interfaceTy) &&
                        _typeToDiscriminatorId.TryGetValue(interfaceTy, out DerivedKdlTypeInfo? interfaceResult) &&
                        interfaceResult is not null)
                    {
                        if (result is null)
                        {
                            result = interfaceResult;
                        }
                        else
                        {
                            ThrowHelper.ThrowNotSupportedException_RuntimeTypeDiamondAmbiguity(BaseType, type, result.KdlTypeInfo.Type, interfaceResult.KdlTypeInfo.Type);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Walks the type hierarchy above the current type for any types that use polymorphic configuration.
        /// </summary>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075:UnrecognizedReflectionPattern",
            Justification = "The call to GetInterfaces will cross-reference results with interface types " +
                            "already declared as derived types of the polymorphic base type.")]
        internal static KdlTypeInfo? FindNearestPolymorphicBaseType(KdlTypeInfo typeInfo)
        {
            Debug.Assert(typeInfo.IsConfigured);

            if (typeInfo.PolymorphismOptions != null)
            {
                // Type defines its own polymorphic configuration.
                return null;
            }

            KdlTypeInfo? matchingResult = null;

            // First, walk up the class hierarchy for any supported types.
            for (Type? candidate = typeInfo.Type.BaseType; candidate != null; candidate = candidate.BaseType)
            {
                KdlTypeInfo? candidateInfo = ResolveAncestorTypeInfo(candidate, typeInfo.Options);
                if (candidateInfo?.PolymorphismOptions != null)
                {
                    // stop on the first ancestor that has a match
                    matchingResult = candidateInfo;
                    break;
                }
            }

            // Now, walk the interface hierarchy for any polymorphic interface declarations.
            foreach (Type interfaceType in typeInfo.Type.GetInterfaces())
            {
                KdlTypeInfo? candidateInfo = ResolveAncestorTypeInfo(interfaceType, typeInfo.Options);
                if (candidateInfo?.PolymorphismOptions != null)
                {
                    if (matchingResult != null)
                    {
                        // Resolve any conflicting matches.
                        if (matchingResult.Type.IsAssignableFrom(interfaceType))
                        {
                            // interface is more derived than previous match, replace it.
                            matchingResult = candidateInfo;
                        }
                        else if (interfaceType.IsAssignableFrom(matchingResult.Type))
                        {
                            // interface is less derived than previous match, keep the previous one.
                            continue;
                        }
                        else
                        {
                            // Diamond ambiguity, do not report any ancestors.
                            return null;
                        }
                    }
                    else
                    {
                        matchingResult = candidateInfo;
                    }
                }
            }

            return matchingResult;

            static KdlTypeInfo? ResolveAncestorTypeInfo(Type type, KdlSerializerOptions options)
            {
                try
                {
                    return options.GetTypeInfoInternal(type, ensureNotNull: null);
                }
                catch
                {
                    // The resolver produced an exception when resolving the ancestor type.
                    // Eat the exception and report no result instead.
                    return null;
                }
            }
        }

        /// <summary>
        /// KdlTypeInfo result holder for a derived type.
        /// </summary>
        private sealed class DerivedKdlTypeInfo
        {
            public DerivedKdlTypeInfo(object? typeDiscriminator, KdlTypeInfo derivedTypeInfo)
            {
                Debug.Assert(typeDiscriminator is null or int or string);

                TypeDiscriminator = typeDiscriminator;
                KdlTypeInfo = derivedTypeInfo;
            }

            public object? TypeDiscriminator { get; }
            public KdlTypeInfo KdlTypeInfo { get; }
        }
    }
}
