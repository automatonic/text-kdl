using System.Diagnostics;
using System.Runtime.CompilerServices;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Reflection;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Lookup the property given its name (obtained from the reader) and return it.
        /// Also sets state.Current.KdlPropertyInfo to a non-null value.
        /// </summary>
        internal static KdlPropertyInfo LookupProperty(
            object? obj,
            ReadOnlySpan<byte> unescapedPropertyName,
            ref ReadStack state,
            KdlSerializerOptions options,
            out bool useExtensionProperty,
            bool createExtensionProperty = true
        )
        {
            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;
            useExtensionProperty = false;

            KdlPropertyInfo? kdlPropertyInfo = kdlTypeInfo.GetProperty(
                unescapedPropertyName,
                ref state.Current,
                out byte[] utf8PropertyName
            );

            // Increment PropertyIndex so GetProperty() checks the next property first when called again.
            state.Current.PropertyIndex++;

            // For case insensitive and missing property support of KdlPath, remember the value on the temporary stack.
            state.Current.KdlPropertyName = utf8PropertyName;

            // Handle missing properties
            if (kdlPropertyInfo is null)
            {
                if (
                    kdlTypeInfo.EffectiveUnmappedMemberHandling
                    is KdlUnmappedMemberHandling.Disallow
                )
                {
                    Debug.Assert(
                        kdlTypeInfo.ExtensionDataProperty is null,
                        "kdlTypeInfo.Configure() should have caught conflicting configuration."
                    );
                    string stringPropertyName = KdlHelpers.Utf8GetString(unescapedPropertyName);
                    ThrowHelper.ThrowKdlException_UnmappedKdlProperty(
                        kdlTypeInfo.Type,
                        stringPropertyName
                    );
                }

                // Determine if we should use the extension property.
                if (
                    kdlTypeInfo.ExtensionDataProperty is KdlPropertyInfo
                    {
                        HasGetter: true,
                        HasSetter: true
                    } dataExtProperty
                )
                {
                    state.Current.KdlPropertyNameAsString = KdlHelpers.Utf8GetString(
                        unescapedPropertyName
                    );

                    if (createExtensionProperty)
                    {
                        Debug.Assert(obj != null, "obj is null");
                        CreateExtensionDataProperty(obj, dataExtProperty, options);
                    }

                    kdlPropertyInfo = dataExtProperty;
                    useExtensionProperty = true;
                }
                else
                {
                    // Populate with a placeholder value required by KdlPath calculations
                    kdlPropertyInfo = KdlPropertyInfo.s_missingProperty;
                }
            }

            state.Current.KdlPropertyInfo = kdlPropertyInfo;
            state.Current.NumberHandling = kdlPropertyInfo.EffectiveNumberHandling;
            return kdlPropertyInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> GetPropertyName(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlSerializerOptions options,
            out bool isAlreadyReadMetadataProperty
        )
        {
            ReadOnlySpan<byte> propertyName = reader.GetUnescapedSpan();
            isAlreadyReadMetadataProperty = false;

            if (state.Current.CanContainMetadata)
            {
                if (
                    IsMetadataPropertyName(
                        propertyName,
                        state.Current.BaseKdlTypeInfo.PolymorphicTypeResolver
                    )
                )
                {
                    if (options.AllowOutOfOrderMetadataProperties)
                    {
                        isAlreadyReadMetadataProperty = true;
                    }
                    else
                    {
                        ThrowHelper.ThrowUnexpectedMetadataException(
                            propertyName,
                            ref reader,
                            ref state
                        );
                    }
                }
            }

            return propertyName;
        }

        internal static void CreateExtensionDataProperty(
            object obj,
            KdlPropertyInfo kdlPropertyInfo,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(kdlPropertyInfo != null);

            object? extensionData = kdlPropertyInfo.GetValueAsObject(obj);
            if (extensionData == null)
            {
                // Create the appropriate dictionary type. We already verified the types.
#if DEBUG
                Type underlyingIDictionaryType =
                    kdlPropertyInfo.PropertyType.GetCompatibleGenericInterface(
                        typeof(IDictionary<,>)
                    )!;
                Type[] genericArgs = underlyingIDictionaryType.GetGenericArguments();

                Debug.Assert(underlyingIDictionaryType.IsGenericType);
                Debug.Assert(genericArgs.Length == 2);
                Debug.Assert(genericArgs[0].UnderlyingSystemType == typeof(string));
                Debug.Assert(
                    genericArgs[1].UnderlyingSystemType == KdlTypeInfo.ObjectType
                        || genericArgs[1].UnderlyingSystemType == typeof(KdlReadOnlyElement)
                        || genericArgs[1].UnderlyingSystemType == typeof(Graph.KdlElement)
                );
#endif

                Func<object>? createObjectForExtensionDataProp =
                    kdlPropertyInfo.KdlTypeInfo.CreateObject
                    ?? kdlPropertyInfo.KdlTypeInfo.CreateObjectForExtensionDataProperty;

                if (createObjectForExtensionDataProp == null)
                {
                    // Avoid a reference to the KdlVertex type for trimming
                    if (kdlPropertyInfo.PropertyType.FullName == KdlTypeInfo.KdlObjectTypeName)
                    {
                        ThrowHelper.ThrowInvalidOperationException_NodeKdlObjectCustomConverterNotAllowedOnExtensionProperty();
                    }
                    else
                    {
                        ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(
                            kdlPropertyInfo.PropertyType
                        );
                    }
                }

                extensionData = createObjectForExtensionDataProp();
                Debug.Assert(kdlPropertyInfo.Set != null);
                kdlPropertyInfo.Set(obj, extensionData);
            }

            // We don't add the value to the dictionary here because we need to support the read-ahead functionality for Streams.
        }
    }
}
