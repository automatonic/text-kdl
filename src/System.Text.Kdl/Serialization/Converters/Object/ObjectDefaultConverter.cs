using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Default base class implementation of <cref>KdlObjectConverter{T}</cref>.
    /// </summary>
    internal class ObjectDefaultConverter<T> : KdlObjectConverter<T> where T : notnull
    {
        internal override bool CanHaveMetadata => true;
        internal override bool SupportsCreateObjectDelegate => true;

        internal override bool OnTryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, [MaybeNullWhen(false)] out T value)
        {
            KdlTypeInfo jsonTypeInfo = state.Current.KdlTypeInfo;

            object obj;

            if (!state.SupportContinuation && !state.Current.CanContainMetadata)
            {
                // Fast path that avoids maintaining state variables and dealing with preserved references.

                if (reader.TokenType != KdlTokenType.StartObject)
                {
                    ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                }

                if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
                {
                    obj = state.Current.ReturnValue!;
                }
                else
                {
                    if (jsonTypeInfo.CreateObject == null)
                    {
                        ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(jsonTypeInfo, ref reader, ref state);
                    }

                    obj = jsonTypeInfo.CreateObject();
                }

                PopulatePropertiesFastPath(obj, jsonTypeInfo, options, ref reader, ref state);
                Debug.Assert(obj != null);
                value = (T)obj;
                return true;
            }
            else
            {
                // Slower path that supports continuation and reading metadata.

                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    if (reader.TokenType != KdlTokenType.StartObject)
                    {
                        ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                    }

                    state.Current.ObjectState = StackFrameObjectState.StartToken;
                }

                // Handle the metadata properties.
                if (state.Current.CanContainMetadata && state.Current.ObjectState < StackFrameObjectState.ReadMetadata)
                {
                    if (!KdlSerializer.TryReadMetadata(this, jsonTypeInfo, ref reader, ref state))
                    {
                        value = default;
                        return false;
                    }

                    if (state.Current.MetadataPropertyNames == MetadataPropertyName.Ref)
                    {
                        value = KdlSerializer.ResolveReferenceId<T>(ref state);
                        return true;
                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadMetadata;
                }

                // Dispatch to any polymorphic converters: should always be entered regardless of ObjectState progress
                if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Type) != 0 &&
                    state.Current.PolymorphicSerializationState != PolymorphicSerializationState.PolymorphicReEntryStarted &&
                    ResolvePolymorphicConverter(jsonTypeInfo, ref state) is KdlConverter polymorphicConverter)
                {
                    Debug.Assert(!IsValueType);
                    bool success = polymorphicConverter.OnTryReadAsObject(ref reader, polymorphicConverter.Type!, options, ref state, out object? objectResult);
                    value = (T)objectResult!;
                    state.ExitPolymorphicConverter(success);
                    return success;
                }

                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    if (state.Current.CanContainMetadata)
                    {
                        KdlSerializer.ValidateMetadataForObjectConverter(ref state);
                    }

                    if (state.Current.MetadataPropertyNames == MetadataPropertyName.Ref)
                    {
                        value = KdlSerializer.ResolveReferenceId<T>(ref state);
                        return true;
                    }

                    if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
                    {
                        obj = state.Current.ReturnValue!;
                    }
                    else
                    {
                        if (jsonTypeInfo.CreateObject == null)
                        {
                            ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(jsonTypeInfo, ref reader, ref state);
                        }

                        obj = jsonTypeInfo.CreateObject();
                    }

                    if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Id) != 0)
                    {
                        Debug.Assert(state.ReferenceId != null);
                        Debug.Assert(options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve);
                        state.ReferenceResolver.AddReference(state.ReferenceId, obj);
                        state.ReferenceId = null;
                    }

                    jsonTypeInfo.OnDeserializing?.Invoke(obj);

                    state.Current.ReturnValue = obj;
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                    state.Current.InitializeRequiredPropertiesValidationState(jsonTypeInfo);
                }
                else
                {
                    obj = state.Current.ReturnValue!;
                    Debug.Assert(obj != null);
                }

                // Process all properties.
                while (true)
                {
                    // Determine the property.
                    if (state.Current.PropertyState == StackFramePropertyState.None)
                    {
                        if (!reader.Read())
                        {
                            state.Current.ReturnValue = obj;
                            value = default;
                            return false;
                        }

                        state.Current.PropertyState = StackFramePropertyState.ReadName;
                    }

                    KdlPropertyInfo jsonPropertyInfo;

                    if (state.Current.PropertyState < StackFramePropertyState.Name)
                    {
                        KdlTokenType tokenType = reader.TokenType;
                        if (tokenType == KdlTokenType.EndObject)
                        {
                            break;
                        }

                        // Read method would have thrown if otherwise.
                        Debug.Assert(tokenType == KdlTokenType.PropertyName);

                        jsonTypeInfo.ValidateCanBeUsedForPropertyMetadataSerialization();
                        ReadOnlySpan<byte> unescapedPropertyName = KdlSerializer.GetPropertyName(ref state, ref reader, options, out bool isAlreadyReadMetadataProperty);
                        if (isAlreadyReadMetadataProperty)
                        {
                            Debug.Assert(options.AllowOutOfOrderMetadataProperties);
                            reader.SkipWithVerify();
                            state.Current.EndProperty();
                            continue;
                        }

                        jsonPropertyInfo = KdlSerializer.LookupProperty(
                            obj,
                            unescapedPropertyName,
                            ref state,
                            options,
                            out bool useExtensionProperty);

                        state.Current.UseExtensionProperty = useExtensionProperty;
                        state.Current.PropertyState = StackFramePropertyState.Name;
                    }
                    else
                    {
                        Debug.Assert(state.Current.KdlPropertyInfo != null);
                        jsonPropertyInfo = state.Current.KdlPropertyInfo!;
                    }

                    if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                    {
                        if (!jsonPropertyInfo.CanDeserializeOrPopulate)
                        {
                            if (!reader.TrySkipPartial(targetDepth: state.Current.OriginalDepth + 1))
                            {
                                state.Current.ReturnValue = obj;
                                value = default;
                                return false;
                            }

                            state.Current.EndProperty();
                            continue;
                        }

                        if (!ReadAheadPropertyValue(ref state, ref reader, jsonPropertyInfo))
                        {
                            state.Current.ReturnValue = obj;
                            value = default;
                            return false;
                        }

                        state.Current.PropertyState = StackFramePropertyState.ReadValue;
                    }

                    if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                    {
                        // Obtain the CLR value from the KDL and set the member.
                        if (!state.Current.UseExtensionProperty)
                        {
                            if (!jsonPropertyInfo.ReadKdlAndSetMember(obj, ref state, ref reader))
                            {
                                state.Current.ReturnValue = obj;
                                value = default;
                                return false;
                            }
                        }
                        else
                        {
                            if (!jsonPropertyInfo.ReadKdlAndAddExtensionProperty(obj, ref state, ref reader))
                            {
                                // No need to set 'value' here since KdlElement must be read in full.
                                state.Current.ReturnValue = obj;
                                value = default;
                                return false;
                            }
                        }

                        state.Current.EndProperty();
                    }
                }
            }

            jsonTypeInfo.OnDeserialized?.Invoke(obj);
            state.Current.ValidateAllRequiredPropertiesAreRead(jsonTypeInfo);

            // Unbox
            Debug.Assert(obj != null);
            value = (T)obj;

            // Check if we are trying to update the UTF-8 property cache.
            if (state.Current.PropertyRefCacheBuilder != null)
            {
                jsonTypeInfo.UpdateUtf8PropertyCache(ref state.Current);
            }

            return true;
        }

        // This method is using aggressive inlining to avoid extra stack frame for deep object graphs.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void PopulatePropertiesFastPath(object obj, KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options, ref KdlReader reader, scoped ref ReadStack state)
        {
            jsonTypeInfo.OnDeserializing?.Invoke(obj);
            state.Current.InitializeRequiredPropertiesValidationState(jsonTypeInfo);

            // Process all properties.
            while (true)
            {
                // Read the property name or EndObject.
                reader.ReadWithVerify();

                KdlTokenType tokenType = reader.TokenType;

                if (tokenType == KdlTokenType.EndObject)
                {
                    break;
                }

                // Read method would have thrown if otherwise.
                Debug.Assert(tokenType == KdlTokenType.PropertyName);

                ReadOnlySpan<byte> unescapedPropertyName = KdlSerializer.GetPropertyName(ref state, ref reader, options, out bool isAlreadyReadMetadataProperty);
                Debug.Assert(!isAlreadyReadMetadataProperty, "Only possible for types that can read metadata, which do not call into the fast-path method.");

                jsonTypeInfo.ValidateCanBeUsedForPropertyMetadataSerialization();
                KdlPropertyInfo jsonPropertyInfo = KdlSerializer.LookupProperty(
                    obj,
                    unescapedPropertyName,
                    ref state,
                    options,
                    out bool useExtensionProperty);

                ReadPropertyValue(obj, ref state, ref reader, jsonPropertyInfo, useExtensionProperty);
            }

            jsonTypeInfo.OnDeserialized?.Invoke(obj);
            state.Current.ValidateAllRequiredPropertiesAreRead(jsonTypeInfo);

            // Check if we are trying to update the UTF-8 property cache.
            if (state.Current.PropertyRefCacheBuilder != null)
            {
                jsonTypeInfo.UpdateUtf8PropertyCache(ref state.Current);
            }
        }

        internal sealed override bool OnTryWrite(
            KdlWriter writer,
            T value,
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            KdlTypeInfo jsonTypeInfo = state.Current.KdlTypeInfo;
            jsonTypeInfo.ValidateCanBeUsedForPropertyMetadataSerialization();

            object obj = value; // box once

            if (!state.SupportContinuation)
            {
                jsonTypeInfo.OnSerializing?.Invoke(obj);

                writer.WriteStartObject();

                if (state.CurrentContainsMetadata && CanHaveMetadata)
                {
                    KdlSerializer.WriteMetadataForObject(this, ref state, writer);
                }

                foreach (KdlPropertyInfo jsonPropertyInfo in jsonTypeInfo.PropertyCache)
                {
                    if (jsonPropertyInfo.CanSerialize)
                    {
                        // Remember the current property for KdlPath support if an exception is thrown.
                        state.Current.KdlPropertyInfo = jsonPropertyInfo;
                        state.Current.NumberHandling = jsonPropertyInfo.EffectiveNumberHandling;

                        bool success = jsonPropertyInfo.GetMemberAndWriteKdl(obj, ref state, writer);
                        // Converters only return 'false' when out of data which is not possible in fast path.
                        Debug.Assert(success);

                        state.Current.EndProperty();
                    }
                }

                // Write extension data after the normal properties.
                KdlPropertyInfo? extensionDataProperty = jsonTypeInfo.ExtensionDataProperty;
                if (extensionDataProperty?.CanSerialize == true)
                {
                    // Remember the current property for KdlPath support if an exception is thrown.
                    state.Current.KdlPropertyInfo = extensionDataProperty;
                    state.Current.NumberHandling = extensionDataProperty.EffectiveNumberHandling;

                    bool success = extensionDataProperty.GetMemberAndWriteKdlExtensionData(obj, ref state, writer);
                    Debug.Assert(success);

                    state.Current.EndProperty();
                }

                writer.WriteEndObject();
            }
            else
            {
                if (!state.Current.ProcessedStartToken)
                {
                    writer.WriteStartObject();

                    if (state.CurrentContainsMetadata && CanHaveMetadata)
                    {
                        KdlSerializer.WriteMetadataForObject(this, ref state, writer);
                    }

                    jsonTypeInfo.OnSerializing?.Invoke(obj);

                    state.Current.ProcessedStartToken = true;
                }

                ReadOnlySpan<KdlPropertyInfo> propertyCache = jsonTypeInfo.PropertyCache;
                while (state.Current.EnumeratorIndex < propertyCache.Length)
                {
                    KdlPropertyInfo jsonPropertyInfo = propertyCache[state.Current.EnumeratorIndex];
                    if (jsonPropertyInfo.CanSerialize)
                    {
                        state.Current.KdlPropertyInfo = jsonPropertyInfo;
                        state.Current.NumberHandling = jsonPropertyInfo.EffectiveNumberHandling;

                        if (!jsonPropertyInfo.GetMemberAndWriteKdl(obj!, ref state, writer))
                        {
                            Debug.Assert(jsonPropertyInfo.EffectiveConverter.ConverterStrategy != ConverterStrategy.Value);
                            return false;
                        }

                        state.Current.EndProperty();
                        state.Current.EnumeratorIndex++;

                        if (ShouldFlush(ref state, writer))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        state.Current.EnumeratorIndex++;
                    }
                }

                // Write extension data after the normal properties.
                if (state.Current.EnumeratorIndex == propertyCache.Length)
                {
                    KdlPropertyInfo? extensionDataProperty = jsonTypeInfo.ExtensionDataProperty;
                    if (extensionDataProperty?.CanSerialize == true)
                    {
                        // Remember the current property for KdlPath support if an exception is thrown.
                        state.Current.KdlPropertyInfo = extensionDataProperty;
                        state.Current.NumberHandling = extensionDataProperty.EffectiveNumberHandling;

                        if (!extensionDataProperty.GetMemberAndWriteKdlExtensionData(obj, ref state, writer))
                        {
                            return false;
                        }

                        state.Current.EndProperty();
                        state.Current.EnumeratorIndex++;

                        if (ShouldFlush(ref state, writer))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        state.Current.EnumeratorIndex++;
                    }
                }

                if (!state.Current.ProcessedEndToken)
                {
                    state.Current.ProcessedEndToken = true;
                    writer.WriteEndObject();
                }
            }

            jsonTypeInfo.OnSerialized?.Invoke(obj);

            return true;
        }

        // AggressiveInlining since this method is only called from two locations and is on a hot path.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void ReadPropertyValue(
            object obj,
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlPropertyInfo jsonPropertyInfo,
            bool useExtensionProperty)
        {
            // Skip the property if not found.
            if (!jsonPropertyInfo.CanDeserializeOrPopulate)
            {
                // The KdlReader.Skip() method will fail fast if it detects that we're reading
                // from a partially read buffer, regardless of whether the next value is available.
                // This can result in erroneous failures in cases where a custom converter is calling
                // into a built-in converter (cf. https://github.com/dotnet/runtime/issues/74108).
                // For this reason we need to call the TrySkip() method instead -- the serializer
                // should guarantee sufficient read-ahead has been performed for the current object.
                bool success = reader.TrySkip();
                Debug.Assert(success, "Serializer should guarantee sufficient read-ahead has been done.");
            }
            else
            {
                // Set the property value.
                reader.ReadWithVerify();

                if (!useExtensionProperty)
                {
                    jsonPropertyInfo.ReadKdlAndSetMember(obj, ref state, ref reader);
                }
                else
                {
                    jsonPropertyInfo.ReadKdlAndAddExtensionProperty(obj, ref state, ref reader);
                }
            }

            // Ensure any exception thrown in the next read does not have a property in its KdlPath.
            state.Current.EndProperty();
        }

        protected static bool ReadAheadPropertyValue(scoped ref ReadStack state, ref KdlReader reader, KdlPropertyInfo jsonPropertyInfo)
        {
            // Extension properties can use the KdlElement converter and thus require read-ahead.
            bool requiresReadAhead = jsonPropertyInfo.EffectiveConverter.RequiresReadAhead || state.Current.UseExtensionProperty;
            return reader.TryAdvanceWithOptionalReadAhead(requiresReadAhead);
        }
    }
}
