// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        internal const string SerializationUnreferencedCodeMessage = "KDL serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a KdlTypeInfo or KdlSerializerContext, or make sure all of the required types are preserved.";
        internal const string SerializationRequiresDynamicCodeMessage = "KDL serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Kdl source generation for native AOT applications.";

        /// <summary>
        /// Indicates whether unconfigured <see cref="KdlSerializerOptions"/> instances
        /// should be set to use the reflection-based <see cref="DefaultKdlTypeInfoResolver"/>.
        /// </summary>
        /// <remarks>
        /// The value of the property is backed by the "System.Text.Kdl.KdlSerializer.IsReflectionEnabledByDefault"
        /// <see cref="AppContext"/> setting and defaults to <see langword="true"/> if unset.
        /// </remarks>
        [FeatureSwitchDefinition("System.Text.Kdl.KdlSerializer.IsReflectionEnabledByDefault")]
        public static bool IsReflectionEnabledByDefault { get; } =
            AppContext.TryGetSwitch(
                switchName: "System.Text.Kdl.KdlSerializer.IsReflectionEnabledByDefault",
                isEnabled: out bool value)
            ? value : true;

        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        private static KdlTypeInfo GetTypeInfo(KdlSerializerOptions? options, Type inputType)
        {
            Debug.Assert(inputType != null);

            options ??= KdlSerializerOptions.Default;
            options.MakeReadOnly(populateMissingResolver: true);

            // In order to improve performance of polymorphic root-level object serialization,
            // we bypass GetTypeInfoForRootType and cache KdlTypeInfo<object> in a dedicated property.
            // This lets any derived types take advantage of the cache in GetTypeInfoForRootType themselves.
            return inputType == KdlTypeInfo.ObjectType
                ? options.ObjectTypeInfo
                : options.GetTypeInfoForRootType(inputType);
        }

        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        private static KdlTypeInfo<T> GetTypeInfo<T>(KdlSerializerOptions? options)
            => (KdlTypeInfo<T>)GetTypeInfo(options, typeof(T));

        private static KdlTypeInfo GetTypeInfo(KdlSerializerContext context, Type inputType)
        {
            Debug.Assert(context != null);
            Debug.Assert(inputType != null);

            KdlTypeInfo? info = context.GetTypeInfo(inputType);
            if (info is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NoMetadataForType(inputType, context);
            }

            info.EnsureConfigured();
            return info;
        }

        private static void ValidateInputType(object? value, Type inputType)
        {
            if (inputType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(inputType));
            }

            if (value is not null)
            {
                Type runtimeType = value.GetType();
                if (!inputType.IsAssignableFrom(runtimeType))
                {
                    ThrowHelper.ThrowArgumentException_DeserializeWrongType(inputType, value);
                }
            }
        }

        internal static bool IsValidNumberHandlingValue(KdlNumberHandling handling) =>
            KdlHelpers.IsInRangeInclusive((int)handling, 0,
                (int)(
                KdlNumberHandling.Strict |
                KdlNumberHandling.AllowReadingFromString |
                KdlNumberHandling.WriteAsString |
                KdlNumberHandling.AllowNamedFloatingPointLiterals));

        internal static bool IsValidCreationHandlingValue(KdlObjectCreationHandling handling) =>
            handling is KdlObjectCreationHandling.Replace or KdlObjectCreationHandling.Populate;

        internal static bool IsValidUnmappedMemberHandlingValue(KdlUnmappedMemberHandling handling) =>
            handling is KdlUnmappedMemberHandling.Skip or KdlUnmappedMemberHandling.Disallow;

        [return: NotNullIfNotNull(nameof(value))]
        internal static T? UnboxOnRead<T>(object? value)
        {
            if (value is null)
            {
                if (default(T) is not null)
                {
                    // Casting null values to a non-nullable struct throws NullReferenceException.
                    ThrowUnableToCastValue(value);
                }

                return default;
            }

            if (value is T typedValue)
            {
                return typedValue;
            }

            ThrowUnableToCastValue(value);
            return default!;

            static void ThrowUnableToCastValue(object? value)
            {
                if (value is null)
                {
                    ThrowHelper.ThrowInvalidOperationException_DeserializeUnableToAssignNull(declaredType: typeof(T));
                }
                else
                {
                    ThrowHelper.ThrowInvalidCastException_DeserializeUnableToAssignValue(typeOfValue: value.GetType(), declaredType: typeof(T));
                }
            }
        }

        [return: NotNullIfNotNull(nameof(value))]
        internal static T? UnboxOnWrite<T>(object? value)
        {
            if (default(T) is not null && value is null)
            {
                // Casting null values to a non-nullable struct throws NullReferenceException.
                ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(typeof(T));
            }

            return (T?)value;
        }
    }
}
