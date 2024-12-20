// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Kdl.Reflection;
using System.Threading;

namespace System.Text.Kdl.Serialization.Metadata
{
    public partial class DefaultKdlTypeInfoResolver
    {
        internal static MemberAccessor MemberAccessor
        {
            [RequiresUnreferencedCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
            [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
            get
            {
                return s_memberAccessor ?? Initialize();
                static MemberAccessor Initialize()
                {
                    MemberAccessor value =
#if NET
                        // if dynamic code isn't supported, fallback to reflection
                        RuntimeFeature.IsDynamicCodeSupported ?
                            new ReflectionEmitCachingMemberAccessor() :
                            new ReflectionMemberAccessor();
#elif NETFRAMEWORK
                            new ReflectionEmitCachingMemberAccessor();
#else
                            new ReflectionMemberAccessor();
#endif
                    return Interlocked.CompareExchange(ref s_memberAccessor, value, null) ?? value;
                }
            }
        }

        internal static void ClearMemberAccessorCaches() => s_memberAccessor?.Clear();
        private static MemberAccessor? s_memberAccessor;

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlTypeInfo CreateTypeInfoCore(Type type, KdlConverter converter, KdlSerializerOptions options)
        {
            KdlTypeInfo typeInfo = KdlTypeInfo.CreateKdlTypeInfo(type, converter, options);

            if (GetNumberHandlingForType(typeInfo.Type) is { } numberHandling)
            {
                typeInfo.NumberHandling = numberHandling;
            }

            if (GetObjectCreationHandlingForType(typeInfo.Type) is { } creationHandling)
            {
                typeInfo.PreferredPropertyObjectCreationHandling = creationHandling;
            }

            if (GetUnmappedMemberHandling(typeInfo.Type) is { } unmappedMemberHandling)
            {
                typeInfo.UnmappedMemberHandling = unmappedMemberHandling;
            }

            typeInfo.PopulatePolymorphismMetadata();
            typeInfo.MapInterfaceTypesToCallbacks();

            Func<object>? createObject = DetermineCreateObjectDelegate(type, converter);
            typeInfo.SetCreateObjectIfCompatible(createObject);
            typeInfo.CreateObjectForExtensionDataProperty = createObject;

            if (typeInfo is { Kind: KdlTypeInfoKind.Object, IsNullable: false })
            {
                NullabilityInfoContext nullabilityCtx = new();

                if (converter.ConstructorIsParameterized)
                {
                    // NB parameter metadata must be populated *before* property metadata
                    // so that properties can be linked to their associated parameters.
                    PopulateParameterInfoValues(typeInfo, nullabilityCtx);
                }

                PopulateProperties(typeInfo, nullabilityCtx);

                typeInfo.ConstructorAttributeProvider = typeInfo.Converter.ConstructorInfo;
            }

            // Plug in any converter configuration -- should be run last.
            converter.ConfigureKdlTypeInfo(typeInfo, options);
            converter.ConfigureKdlTypeInfoUsingReflection(typeInfo, options);
            return typeInfo;
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static void PopulateProperties(KdlTypeInfo typeInfo, NullabilityInfoContext nullabilityCtx)
        {
            Debug.Assert(!typeInfo.IsReadOnly);
            Debug.Assert(typeInfo.Kind is KdlTypeInfoKind.Object);

            // SetsRequiredMembersAttribute means that all required members are assigned by constructor and therefore there is no enforcement
            bool constructorHasSetsRequiredMembersAttribute =
                typeInfo.Converter.ConstructorInfo?.HasSetsRequiredMembersAttribute() ?? false;

            KdlTypeInfo.PropertyHierarchyResolutionState state = new(typeInfo.Options);

            // Walk the type hierarchy starting from the current type up to the base type(s)
            foreach (Type currentType in typeInfo.Type.GetSortedTypeHierarchy())
            {
                if (currentType == KdlTypeInfo.ObjectType ||
                    currentType == typeof(ValueType))
                {
                    // Don't process any members for typeof(object) or System.ValueType
                    break;
                }

                AddMembersDeclaredBySuperType(
                    typeInfo,
                    currentType,
                    nullabilityCtx,
                    constructorHasSetsRequiredMembersAttribute,
                    ref state);
            }

            if (state.IsPropertyOrderSpecified)
            {
                typeInfo.PropertyList.SortProperties();
            }
        }

        private const BindingFlags AllInstanceMembers =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;


        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static void AddMembersDeclaredBySuperType(
            KdlTypeInfo typeInfo,
            Type currentType,
            NullabilityInfoContext nullabilityCtx,
            bool constructorHasSetsRequiredMembersAttribute,
            ref KdlTypeInfo.PropertyHierarchyResolutionState state)
        {
            Debug.Assert(!typeInfo.IsReadOnly);
            Debug.Assert(currentType.IsAssignableFrom(typeInfo.Type));

            // Compiler adds RequiredMemberAttribute to type if any of the members are marked with 'required' keyword.
            bool shouldCheckMembersForRequiredMemberAttribute =
                !constructorHasSetsRequiredMembersAttribute && currentType.HasRequiredMemberAttribute();

            foreach (PropertyInfo propertyInfo in currentType.GetProperties(AllInstanceMembers))
            {
                // Ignore indexers and virtual properties that have overrides that were [KdlIgnore]d.
                if (propertyInfo.GetIndexParameters().Length > 0 ||
                    PropertyIsOverriddenAndIgnored(propertyInfo, state.IgnoredProperties))
                {
                    continue;
                }

                bool hasKdlIncludeAttribute = propertyInfo.GetCustomAttribute<KdlIncludeAttribute>(inherit: false) != null;

                // Only include properties that either have a public getter or a public setter or have the KdlIncludeAttribute set.
                if (propertyInfo.GetMethod?.IsPublic == true ||
                    propertyInfo.SetMethod?.IsPublic == true ||
                    hasKdlIncludeAttribute)
                {
                    AddMember(
                        typeInfo,
                        typeToConvert: propertyInfo.PropertyType,
                        memberInfo: propertyInfo,
                        nullabilityCtx,
                        shouldCheckMembersForRequiredMemberAttribute,
                        hasKdlIncludeAttribute,
                        ref state);
                }
            }

            foreach (FieldInfo fieldInfo in currentType.GetFields(AllInstanceMembers))
            {
                bool hasKdlIncludeAttribute = fieldInfo.GetCustomAttribute<KdlIncludeAttribute>(inherit: false) != null;
                if (hasKdlIncludeAttribute || (fieldInfo.IsPublic && typeInfo.Options.IncludeFields))
                {
                    AddMember(
                        typeInfo,
                        typeToConvert: fieldInfo.FieldType,
                        memberInfo: fieldInfo,
                        nullabilityCtx,
                        shouldCheckMembersForRequiredMemberAttribute,
                        hasKdlIncludeAttribute,
                        ref state);
                }
            }
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static void AddMember(
            KdlTypeInfo typeInfo,
            Type typeToConvert,
            MemberInfo memberInfo,
            NullabilityInfoContext nullabilityCtx,
            bool shouldCheckForRequiredKeyword,
            bool hasKdlIncludeAttribute,
            ref KdlTypeInfo.PropertyHierarchyResolutionState state)
        {
            KdlPropertyInfo? jsonPropertyInfo = CreatePropertyInfo(typeInfo, typeToConvert, memberInfo, nullabilityCtx, typeInfo.Options, shouldCheckForRequiredKeyword, hasKdlIncludeAttribute);
            if (jsonPropertyInfo == null)
            {
                // ignored invalid property
                return;
            }

            Debug.Assert(jsonPropertyInfo.Name != null);
            typeInfo.PropertyList.AddPropertyWithConflictResolution(jsonPropertyInfo, ref state);
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlPropertyInfo? CreatePropertyInfo(
            KdlTypeInfo typeInfo,
            Type typeToConvert,
            MemberInfo memberInfo,
            NullabilityInfoContext nullabilityCtx,
            KdlSerializerOptions options,
            bool shouldCheckForRequiredKeyword,
            bool hasKdlIncludeAttribute)
        {
            KdlIgnoreCondition? ignoreCondition = memberInfo.GetCustomAttribute<KdlIgnoreAttribute>(inherit: false)?.Condition;

            if (KdlTypeInfo.IsInvalidForSerialization(typeToConvert))
            {
                if (ignoreCondition == KdlIgnoreCondition.Always)
                    return null;

                ThrowHelper.ThrowInvalidOperationException_CannotSerializeInvalidType(typeToConvert, memberInfo.DeclaringType, memberInfo);
            }

            // Resolve any custom converters on the attribute level.
            KdlConverter? customConverter;
            try
            {
                customConverter = GetCustomConverterForMember(typeToConvert, memberInfo, options);
            }
            catch (InvalidOperationException) when (ignoreCondition == KdlIgnoreCondition.Always)
            {
                // skip property altogether if attribute is invalid and the property is ignored
                return null;
            }

            KdlPropertyInfo jsonPropertyInfo = typeInfo.CreatePropertyUsingReflection(typeToConvert, declaringType: memberInfo.DeclaringType);
            PopulatePropertyInfo(jsonPropertyInfo, memberInfo, customConverter, ignoreCondition, nullabilityCtx, shouldCheckForRequiredKeyword, hasKdlIncludeAttribute);
            return jsonPropertyInfo;
        }

        private static KdlNumberHandling? GetNumberHandlingForType(Type type)
        {
            KdlNumberHandlingAttribute? numberHandlingAttribute = type.GetUniqueCustomAttribute<KdlNumberHandlingAttribute>(inherit: false);
            return numberHandlingAttribute?.Handling;
        }

        private static KdlObjectCreationHandling? GetObjectCreationHandlingForType(Type type)
        {
            KdlObjectCreationHandlingAttribute? creationHandlingAttribute = type.GetUniqueCustomAttribute<KdlObjectCreationHandlingAttribute>(inherit: false);
            return creationHandlingAttribute?.Handling;
        }

        private static KdlUnmappedMemberHandling? GetUnmappedMemberHandling(Type type)
        {
            KdlUnmappedMemberHandlingAttribute? numberHandlingAttribute = type.GetUniqueCustomAttribute<KdlUnmappedMemberHandlingAttribute>(inherit: false);
            return numberHandlingAttribute?.UnmappedMemberHandling;
        }

        private static bool PropertyIsOverriddenAndIgnored(PropertyInfo propertyInfo, Dictionary<string, KdlPropertyInfo>? ignoredMembers)
        {
            return propertyInfo.IsVirtual() &&
                ignoredMembers?.TryGetValue(propertyInfo.Name, out KdlPropertyInfo? ignoredMember) == true &&
                ignoredMember.IsVirtual &&
                propertyInfo.PropertyType == ignoredMember.PropertyType;
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static void PopulateParameterInfoValues(KdlTypeInfo typeInfo, NullabilityInfoContext nullabilityCtx)
        {
            Debug.Assert(typeInfo.Converter.ConstructorInfo != null);
            ParameterInfo[] parameters = typeInfo.Converter.ConstructorInfo.GetParameters();
            int parameterCount = parameters.Length;
            KdlParameterInfoValues[] jsonParameters = new KdlParameterInfoValues[parameterCount];

            for (int i = 0; i < parameterCount; i++)
            {
                ParameterInfo reflectionInfo = parameters[i];

                // Trimmed parameter names are reported as null in CoreCLR or "" in Mono.
                if (string.IsNullOrEmpty(reflectionInfo.Name))
                {
                    Debug.Assert(typeInfo.Converter.ConstructorInfo.DeclaringType != null);
                    ThrowHelper.ThrowNotSupportedException_ConstructorContainsNullParameterNames(typeInfo.Converter.ConstructorInfo.DeclaringType);
                }

                KdlParameterInfoValues jsonInfo = new()
                {
                    Name = reflectionInfo.Name,
                    ParameterType = reflectionInfo.ParameterType,
                    Position = reflectionInfo.Position,
                    HasDefaultValue = reflectionInfo.HasDefaultValue,
                    DefaultValue = reflectionInfo.GetDefaultValue(),
                    IsNullable = DetermineParameterNullability(reflectionInfo, nullabilityCtx) is not NullabilityState.NotNull,
                };

                jsonParameters[i] = jsonInfo;
            }

            typeInfo.PopulateParameterInfoValues(jsonParameters);
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static void PopulatePropertyInfo(
            KdlPropertyInfo jsonPropertyInfo,
            MemberInfo memberInfo,
            KdlConverter? customConverter,
            KdlIgnoreCondition? ignoreCondition,
            NullabilityInfoContext nullabilityCtx,
            bool shouldCheckForRequiredKeyword,
            bool hasKdlIncludeAttribute)
        {
            Debug.Assert(jsonPropertyInfo.AttributeProvider == null);

            switch (jsonPropertyInfo.AttributeProvider = memberInfo)
            {
                case PropertyInfo propertyInfo:
                    jsonPropertyInfo.MemberName = propertyInfo.Name;
                    jsonPropertyInfo.IsVirtual = propertyInfo.IsVirtual();
                    jsonPropertyInfo.MemberType = MemberTypes.Property;
                    break;
                case FieldInfo fieldInfo:
                    jsonPropertyInfo.MemberName = fieldInfo.Name;
                    jsonPropertyInfo.MemberType = MemberTypes.Field;
                    break;
                default:
                    Debug.Fail("Only FieldInfo and PropertyInfo members are supported.");
                    break;
            }

            jsonPropertyInfo.CustomConverter = customConverter;
            DeterminePropertyPolicies(jsonPropertyInfo, memberInfo);
            DeterminePropertyName(jsonPropertyInfo, memberInfo);
            DeterminePropertyIsRequired(jsonPropertyInfo, memberInfo, shouldCheckForRequiredKeyword);
            DeterminePropertyNullability(jsonPropertyInfo, memberInfo, nullabilityCtx);

            if (ignoreCondition != KdlIgnoreCondition.Always)
            {
                jsonPropertyInfo.DetermineReflectionPropertyAccessors(memberInfo, useNonPublicAccessors: hasKdlIncludeAttribute);
            }

            jsonPropertyInfo.IgnoreCondition = ignoreCondition;
            jsonPropertyInfo.IsExtensionData = memberInfo.GetCustomAttribute<KdlExtensionDataAttribute>(inherit: false) != null;
        }

        private static void DeterminePropertyPolicies(KdlPropertyInfo propertyInfo, MemberInfo memberInfo)
        {
            KdlPropertyOrderAttribute? orderAttr = memberInfo.GetCustomAttribute<KdlPropertyOrderAttribute>(inherit: false);
            propertyInfo.Order = orderAttr?.Order ?? 0;

            KdlNumberHandlingAttribute? numberHandlingAttr = memberInfo.GetCustomAttribute<KdlNumberHandlingAttribute>(inherit: false);
            propertyInfo.NumberHandling = numberHandlingAttr?.Handling;

            KdlObjectCreationHandlingAttribute? objectCreationHandlingAttr = memberInfo.GetCustomAttribute<KdlObjectCreationHandlingAttribute>(inherit: false);
            propertyInfo.ObjectCreationHandling = objectCreationHandlingAttr?.Handling;
        }

        private static void DeterminePropertyName(KdlPropertyInfo propertyInfo, MemberInfo memberInfo)
        {
            KdlPropertyNameAttribute? nameAttribute = memberInfo.GetCustomAttribute<KdlPropertyNameAttribute>(inherit: false);
            string? name;
            if (nameAttribute != null)
            {
                name = nameAttribute.Name;
            }
            else if (propertyInfo.Options.PropertyNamingPolicy != null)
            {
                name = propertyInfo.Options.PropertyNamingPolicy.ConvertName(memberInfo.Name);
            }
            else
            {
                name = memberInfo.Name;
            }

            if (name == null)
            {
                ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameNull(propertyInfo);
            }

            propertyInfo.Name = name;
        }

        private static void DeterminePropertyIsRequired(KdlPropertyInfo propertyInfo, MemberInfo memberInfo, bool shouldCheckForRequiredKeyword)
        {
            propertyInfo.IsRequired =
                memberInfo.GetCustomAttribute<KdlRequiredAttribute>(inherit: false) != null
                || (shouldCheckForRequiredKeyword && memberInfo.HasRequiredMemberAttribute());
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal static void DeterminePropertyAccessors<T>(KdlPropertyInfo<T> jsonPropertyInfo, MemberInfo memberInfo, bool useNonPublicAccessors)
        {
            Debug.Assert(memberInfo is FieldInfo or PropertyInfo);

            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    MethodInfo? getMethod = propertyInfo.GetMethod;
                    if (getMethod != null && (getMethod.IsPublic || useNonPublicAccessors))
                    {
                        jsonPropertyInfo.Get = MemberAccessor.CreatePropertyGetter<T>(propertyInfo);
                    }

                    MethodInfo? setMethod = propertyInfo.SetMethod;
                    if (setMethod != null && (setMethod.IsPublic || useNonPublicAccessors))
                    {
                        jsonPropertyInfo.Set = MemberAccessor.CreatePropertySetter<T>(propertyInfo);
                    }

                    break;

                case FieldInfo fieldInfo:
                    Debug.Assert(fieldInfo.IsPublic || useNonPublicAccessors);

                    jsonPropertyInfo.Get = MemberAccessor.CreateFieldGetter<T>(fieldInfo);

                    if (!fieldInfo.IsInitOnly)
                    {
                        jsonPropertyInfo.Set = MemberAccessor.CreateFieldSetter<T>(fieldInfo);
                    }

                    break;

                default:
                    Debug.Fail($"Invalid MemberInfo type: {memberInfo.MemberType}");
                    break;
            }
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static Func<object>? DetermineCreateObjectDelegate(Type type, KdlConverter converter)
        {
            ConstructorInfo? defaultCtor = null;

            if (converter.ConstructorInfo != null && !converter.ConstructorIsParameterized)
            {
                // A parameterless constructor has been resolved by the converter
                // (e.g. it might be a non-public ctor with KdlConverterAttribute).
                defaultCtor = converter.ConstructorInfo;
            }

            // Fall back to resolving any public constructors on the type.
            defaultCtor ??= type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);

            return MemberAccessor.CreateParameterlessConstructor(type, defaultCtor);
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static void DeterminePropertyNullability(KdlPropertyInfo propertyInfo, MemberInfo memberInfo, NullabilityInfoContext nullabilityCtx)
        {
            if (!propertyInfo.PropertyTypeCanBeNull)
            {
                return;
            }

            NullabilityInfo nullabilityInfo;
            if (propertyInfo.MemberType is MemberTypes.Property)
            {
                nullabilityInfo = nullabilityCtx.Create((PropertyInfo)memberInfo);
            }
            else
            {
                Debug.Assert(propertyInfo.MemberType is MemberTypes.Field);
                nullabilityInfo = nullabilityCtx.Create((FieldInfo)memberInfo);
            }

            propertyInfo.IsGetNullable = nullabilityInfo.ReadState is not NullabilityState.NotNull;
            propertyInfo.IsSetNullable = nullabilityInfo.WriteState is not NullabilityState.NotNull;
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static NullabilityState DetermineParameterNullability(ParameterInfo parameterInfo, NullabilityInfoContext nullabilityCtx)
        {
            if (!parameterInfo.ParameterType.IsNullableType())
            {
                return NullabilityState.NotNull;
            }
#if NET8_0
            // Workaround for https://github.com/dotnet/runtime/issues/92487
            // The fix has been incorporated into .NET 9 (and the polyfilled implementations in netfx).
            // Should be removed once .NET 8 support is dropped.
            if (parameterInfo.GetGenericParameterDefinition() is { ParameterType: { IsGenericParameter: true } typeParam })
            {
                // Step 1. Look for nullable annotations on the type parameter.
                if (GetNullableFlags(typeParam) is byte[] flags)
                {
                    return TranslateByte(flags[0]);
                }

                // Step 2. Look for nullable annotations on the generic method declaration.
                if (typeParam.DeclaringMethod != null && GetNullableContextFlag(typeParam.DeclaringMethod) is byte flag)
                {
                    return TranslateByte(flag);
                }

                // Step 3. Look for nullable annotations on the generic type declaration.
                if (GetNullableContextFlag(typeParam.DeclaringType!) is byte flag2)
                {
                    return TranslateByte(flag2);
                }

                // Default to nullable.
                return NullabilityState.Nullable;

                static byte[]? GetNullableFlags(MemberInfo member)
                {
                    foreach (CustomAttributeData attr in member.GetCustomAttributesData())
                    {
                        Type attrType = attr.AttributeType;
                        if (attrType.Name == "NullableAttribute" && attrType.Namespace == "System.Runtime.CompilerServices")
                        {
                            foreach (CustomAttributeTypedArgument ctorArg in attr.ConstructorArguments)
                            {
                                switch (ctorArg.Value)
                                {
                                    case byte flag:
                                        return [flag];
                                    case byte[] flags:
                                        return flags;
                                }
                            }
                        }
                    }

                    return null;
                }

                static byte? GetNullableContextFlag(MemberInfo member)
                {
                    foreach (CustomAttributeData attr in member.GetCustomAttributesData())
                    {
                        Type attrType = attr.AttributeType;
                        if (attrType.Name == "NullableContextAttribute" && attrType.Namespace == "System.Runtime.CompilerServices")
                        {
                            foreach (CustomAttributeTypedArgument ctorArg in attr.ConstructorArguments)
                            {
                                if (ctorArg.Value is byte flag)
                                {
                                    return flag;
                                }
                            }
                        }
                    }

                    return null;
                }

                static NullabilityState TranslateByte(byte b) =>
                    b switch
                    {
                        1 => NullabilityState.NotNull,
                        2 => NullabilityState.Nullable,
                        _ => NullabilityState.Unknown
                    };
            }
#endif
            NullabilityInfo nullability = nullabilityCtx.Create(parameterInfo);
            return nullability.WriteState;
        }
    }
}
