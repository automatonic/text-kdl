using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Kdl.Reflection;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
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
            bool createExtensionProperty = true)
        {
            KdlTypeInfo jsonTypeInfo = state.Current.KdlTypeInfo;
            useExtensionProperty = false;

            KdlPropertyInfo? jsonPropertyInfo = jsonTypeInfo.GetProperty(
                unescapedPropertyName,
                ref state.Current,
                out byte[] utf8PropertyName);

            // Increment PropertyIndex so GetProperty() checks the next property first when called again.
            state.Current.PropertyIndex++;

            // For case insensitive and missing property support of KdlPath, remember the value on the temporary stack.
            state.Current.KdlPropertyName = utf8PropertyName;

            // Handle missing properties
            if (jsonPropertyInfo is null)
            {
                if (jsonTypeInfo.EffectiveUnmappedMemberHandling is KdlUnmappedMemberHandling.Disallow)
                {
                    Debug.Assert(jsonTypeInfo.ExtensionDataProperty is null, "jsonTypeInfo.Configure() should have caught conflicting configuration.");
                    string stringPropertyName = KdlHelpers.Utf8GetString(unescapedPropertyName);
                    ThrowHelper.ThrowKdlException_UnmappedKdlProperty(jsonTypeInfo.Type, stringPropertyName);
                }

                // Determine if we should use the extension property.
                if (jsonTypeInfo.ExtensionDataProperty is KdlPropertyInfo { HasGetter: true, HasSetter: true } dataExtProperty)
                {
                    state.Current.KdlPropertyNameAsString = KdlHelpers.Utf8GetString(unescapedPropertyName);

                    if (createExtensionProperty)
                    {
                        Debug.Assert(obj != null, "obj is null");
                        CreateExtensionDataProperty(obj, dataExtProperty, options);
                    }

                    jsonPropertyInfo = dataExtProperty;
                    useExtensionProperty = true;
                }
                else
                {
                    // Populate with a placeholder value required by KdlPath calculations
                    jsonPropertyInfo = KdlPropertyInfo.s_missingProperty;
                }
            }

            state.Current.KdlPropertyInfo = jsonPropertyInfo;
            state.Current.NumberHandling = jsonPropertyInfo.EffectiveNumberHandling;
            return jsonPropertyInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> GetPropertyName(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlSerializerOptions options,
            out bool isAlreadyReadMetadataProperty)
        {
            ReadOnlySpan<byte> propertyName = reader.GetUnescapedSpan();
            isAlreadyReadMetadataProperty = false;

            if (state.Current.CanContainMetadata)
            {
                if (IsMetadataPropertyName(propertyName, state.Current.BaseKdlTypeInfo.PolymorphicTypeResolver))
                {
                    if (options.AllowOutOfOrderMetadataProperties)
                    {
                        isAlreadyReadMetadataProperty = true;
                    }
                    else
                    {
                        ThrowHelper.ThrowUnexpectedMetadataException(propertyName, ref reader, ref state);
                    }
                }
            }

            return propertyName;
        }

        internal static void CreateExtensionDataProperty(
            object obj,
            KdlPropertyInfo jsonPropertyInfo,
            KdlSerializerOptions options)
        {
            Debug.Assert(jsonPropertyInfo != null);

            object? extensionData = jsonPropertyInfo.GetValueAsObject(obj);
            if (extensionData == null)
            {
                // Create the appropriate dictionary type. We already verified the types.
#if DEBUG
                Type underlyingIDictionaryType = jsonPropertyInfo.PropertyType.GetCompatibleGenericInterface(typeof(IDictionary<,>))!;
                Type[] genericArgs = underlyingIDictionaryType.GetGenericArguments();

                Debug.Assert(underlyingIDictionaryType.IsGenericType);
                Debug.Assert(genericArgs.Length == 2);
                Debug.Assert(genericArgs[0].UnderlyingSystemType == typeof(string));
                Debug.Assert(
                    genericArgs[1].UnderlyingSystemType == KdlTypeInfo.ObjectType ||
                    genericArgs[1].UnderlyingSystemType == typeof(KdlElement) ||
                    genericArgs[1].UnderlyingSystemType == typeof(Nodes.KdlVertex));
#endif

                Func<object>? createObjectForExtensionDataProp = jsonPropertyInfo.KdlTypeInfo.CreateObject
                    ?? jsonPropertyInfo.KdlTypeInfo.CreateObjectForExtensionDataProperty;

                if (createObjectForExtensionDataProp == null)
                {
                    // Avoid a reference to the KdlVertex type for trimming
                    if (jsonPropertyInfo.PropertyType.FullName == KdlTypeInfo.KdlObjectTypeName)
                    {
                        ThrowHelper.ThrowInvalidOperationException_NodeKdlObjectCustomConverterNotAllowedOnExtensionProperty();
                    }
                    else
                    {
                        ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(jsonPropertyInfo.PropertyType);
                    }
                }

                extensionData = createObjectForExtensionDataProp();
                Debug.Assert(jsonPropertyInfo.Set != null);
                jsonPropertyInfo.Set(obj, extensionData);
            }

            // We don't add the value to the dictionary here because we need to support the read-ahead functionality for Streams.
        }
    }
}
