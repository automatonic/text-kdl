using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization.Metadata;
using FoundProperty = System.ValueTuple<
    Automatonic.Text.Kdl.Serialization.Metadata.KdlPropertyInfo,
    Automatonic.Text.Kdl.KdlReaderState,
    long,
    byte[]?,
    string?
>;
using FoundPropertyAsync = System.ValueTuple<
    Automatonic.Text.Kdl.Serialization.Metadata.KdlPropertyInfo,
    object?,
    string?
>;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Implementation of <cref>KdlObjectConverter{T}</cref> that supports the deserialization
    /// of KDL objects using parameterized constructors.
    /// </summary>
    internal abstract partial class ObjectWithParameterizedConstructorConverter<T>
        : ObjectDefaultConverter<T>
        where T : notnull
    {
        internal sealed override bool ConstructorIsParameterized => true;

        internal sealed override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            [MaybeNullWhen(false)] out T value
        )
        {
            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;

            if (!kdlTypeInfo.UsesParameterizedConstructor || state.Current.IsPopulating)
            {
                // Fall back to default object converter in following cases:
                // - if user configuration has invalidated the parameterized constructor
                // - we're continuing populating an object.
                return base.OnTryRead(ref reader, typeToConvert, options, ref state, out value);
            }

            object obj;
            ArgumentState argumentState = state.Current.CtorArgumentState!;

            if (!state.SupportContinuation && !state.Current.CanContainMetadata)
            {
                // Fast path that avoids maintaining state variables.

                if (reader.TokenType != KdlTokenType.StartChildrenBlock)
                {
                    ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                }

                if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
                {
                    object populatedObject = state.Current.ReturnValue!;
                    PopulatePropertiesFastPath(
                        populatedObject,
                        kdlTypeInfo,
                        options,
                        ref reader,
                        ref state
                    );
                    value = (T)populatedObject;
                    return true;
                }

                ReadOnlySpan<byte> originalSpan = reader.OriginalSpan;
                ReadOnlySequence<byte> originalSequence = reader.OriginalSequence;

                ReadConstructorArguments(ref state, ref reader, options);

                // We've read all ctor parameters and properties,
                // validate that all required parameters were provided
                // before calling the constructor which may throw.
                state.Current.ValidateAllRequiredPropertiesAreRead(kdlTypeInfo);

                obj = (T)CreateObject(ref state.Current);

                kdlTypeInfo.OnDeserializing?.Invoke(obj);

                if (argumentState.FoundPropertyCount > 0)
                {
                    KdlReader tempReader;

                    FoundProperty[]? properties = argumentState.FoundProperties;
                    Debug.Assert(properties != null);

                    for (int i = 0; i < argumentState.FoundPropertyCount; i++)
                    {
                        KdlPropertyInfo kdlPropertyInfo = properties[i].Item1;
                        long resumptionByteIndex = properties[i].Item3;
                        byte[]? propertyNameArray = properties[i].Item4;
                        string? dataExtKey = properties[i].Item5;

                        tempReader = originalSequence.IsEmpty
                            ? new KdlReader(
                                originalSpan[checked((int)resumptionByteIndex)..],
                                isFinalBlock: true,
                                state: properties[i].Item2
                            )
                            : new KdlReader(
                                originalSequence.Slice(resumptionByteIndex),
                                isFinalBlock: true,
                                state: properties[i].Item2
                            );

                        Debug.Assert(tempReader.TokenType == KdlTokenType.PropertyName);

                        state.Current.KdlPropertyName = propertyNameArray;
                        state.Current.KdlPropertyInfo = kdlPropertyInfo;
                        state.Current.NumberHandling = kdlPropertyInfo.EffectiveNumberHandling;

                        bool useExtensionProperty = dataExtKey != null;

                        if (useExtensionProperty)
                        {
                            Debug.Assert(
                                kdlPropertyInfo == state.Current.KdlTypeInfo.ExtensionDataProperty
                            );
                            state.Current.KdlPropertyNameAsString = dataExtKey;
                            KdlSerializer.CreateExtensionDataProperty(
                                obj,
                                kdlPropertyInfo,
                                options
                            );
                        }

                        ReadPropertyValue(
                            obj,
                            ref state,
                            ref tempReader,
                            kdlPropertyInfo,
                            useExtensionProperty
                        );
                    }

                    FoundProperty[] toReturn = argumentState.FoundProperties!;
                    argumentState.FoundProperties = null;
                    ArrayPool<FoundProperty>.Shared.Return(toReturn, clearArray: true);
                }
            }
            else
            {
                // Slower path that supports continuation and metadata reads.

                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    if (reader.TokenType != KdlTokenType.StartChildrenBlock)
                    {
                        ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                    }

                    state.Current.ObjectState = StackFrameObjectState.StartToken;
                }

                // Read any metadata properties.
                if (
                    state.Current.CanContainMetadata
                    && state.Current.ObjectState < StackFrameObjectState.ReadMetadata
                )
                {
                    if (!KdlSerializer.TryReadMetadata(this, kdlTypeInfo, ref reader, ref state))
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
                if (
                    (state.Current.MetadataPropertyNames & MetadataPropertyName.Type) != 0
                    && state.Current.PolymorphicSerializationState
                        != PolymorphicSerializationState.PolymorphicReEntryStarted
                    && ResolvePolymorphicConverter(kdlTypeInfo, ref state)
                        is KdlConverter polymorphicConverter
                )
                {
                    Debug.Assert(!IsValueType);
                    bool success = polymorphicConverter.OnTryReadAsObject(
                        ref reader,
                        polymorphicConverter.Type!,
                        options,
                        ref state,
                        out object? objectResult
                    );
                    value = (T)objectResult!;
                    state.ExitPolymorphicConverter(success);
                    return success;
                }

                // We need to populate before we started reading constructor arguments.
                // Metadata is disallowed with Populate option and therefore ordering here is irrelevant.
                // Since state.Current.IsPopulating is being checked early on in this method the continuation
                // will be handled there.
                if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
                {
                    object populatedObject = state.Current.ReturnValue!;

                    kdlTypeInfo.OnDeserializing?.Invoke(populatedObject);
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                    state.Current.InitializeRequiredPropertiesValidationState(kdlTypeInfo);
                    return base.OnTryRead(ref reader, typeToConvert, options, ref state, out value);
                }

                // Handle metadata post polymorphic dispatch
                if (state.Current.ObjectState < StackFrameObjectState.ConstructorArguments)
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

                    BeginRead(ref state, options);

                    state.Current.ObjectState = StackFrameObjectState.ConstructorArguments;
                }

                if (!ReadConstructorArgumentsWithContinuation(ref state, ref reader, options))
                {
                    value = default;
                    return false;
                }

                // We've read all ctor parameters and properties,
                // validate that all required parameters were provided
                // before calling the constructor which may throw.
                state.Current.ValidateAllRequiredPropertiesAreRead(kdlTypeInfo);

                obj = (T)CreateObject(ref state.Current);

                if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Id) != 0)
                {
                    Debug.Assert(state.ReferenceId != null);
                    Debug.Assert(
                        options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve
                    );
                    state.ReferenceResolver.AddReference(state.ReferenceId, obj);
                    state.ReferenceId = null;
                }

                kdlTypeInfo.OnDeserializing?.Invoke(obj);

                if (argumentState.FoundPropertyCount > 0)
                {
                    for (int i = 0; i < argumentState.FoundPropertyCount; i++)
                    {
                        KdlPropertyInfo kdlPropertyInfo = argumentState
                            .FoundPropertiesAsync![i]
                            .Item1;
                        object? propValue = argumentState.FoundPropertiesAsync![i].Item2;
                        string? dataExtKey = argumentState.FoundPropertiesAsync![i].Item3;

                        if (dataExtKey == null)
                        {
                            Debug.Assert(kdlPropertyInfo.Set != null);

                            if (
                                propValue is not null
                                || !kdlPropertyInfo.IgnoreNullTokensOnRead
                                || default(T) is not null
                            )
                            {
                                kdlPropertyInfo.Set(obj, propValue);
                            }
                        }
                        else
                        {
                            Debug.Assert(
                                kdlPropertyInfo == state.Current.KdlTypeInfo.ExtensionDataProperty
                            );

                            KdlSerializer.CreateExtensionDataProperty(
                                obj,
                                kdlPropertyInfo,
                                options
                            );
                            object extDictionary = kdlPropertyInfo.GetValueAsObject(obj)!;

                            if (extDictionary is IDictionary<string, KdlReadOnlyElement> dict)
                            {
                                dict[dataExtKey] = (KdlReadOnlyElement)propValue!;
                            }
                            else
                            {
                                ((IDictionary<string, object>)extDictionary)[dataExtKey] =
                                    propValue!;
                            }
                        }
                    }

                    FoundPropertyAsync[] toReturn = argumentState.FoundPropertiesAsync!;
                    argumentState.FoundPropertiesAsync = null;
                    ArrayPool<FoundPropertyAsync>.Shared.Return(toReturn, clearArray: true);
                }
            }

            kdlTypeInfo.OnDeserialized?.Invoke(obj);

            // Unbox
            Debug.Assert(obj != null);
            value = (T)obj;

            // Check if we are trying to update the UTF-8 property cache.
            if (state.Current.PropertyRefCacheBuilder != null)
            {
                kdlTypeInfo.UpdateUtf8PropertyCache(ref state.Current);
            }

            return true;
        }

        protected abstract void InitializeConstructorArgumentCaches(
            ref ReadStack state,
            KdlSerializerOptions options
        );

        protected abstract bool ReadAndCacheConstructorArgument(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlParameterInfo kdlParameterInfo
        );

        protected abstract object CreateObject(ref ReadStackFrame frame);

        /// <summary>
        /// Performs a full first pass of the KDL input and deserializes the ctor args.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadConstructorArguments(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlSerializerOptions options
        )
        {
            BeginRead(ref state, options);

            while (true)
            {
                // Read the next property name or EndObject.
                reader.ReadWithVerify();

                KdlTokenType tokenType = reader.TokenType;

                if (tokenType == KdlTokenType.EndChildrenBlock)
                {
                    return;
                }

                // Read method would have thrown if otherwise.
                Debug.Assert(tokenType == KdlTokenType.PropertyName);

                ReadOnlySpan<byte> unescapedPropertyName = KdlSerializer.GetPropertyName(
                    ref state,
                    ref reader,
                    options,
                    out bool isAlreadyReadMetadataProperty
                );
                if (isAlreadyReadMetadataProperty)
                {
                    Debug.Assert(options.AllowOutOfOrderMetadataProperties);
                    reader.SkipWithVerify();
                    state.Current.EndProperty();
                    continue;
                }

                if (
                    TryLookupConstructorParameter(
                        unescapedPropertyName,
                        ref state,
                        options,
                        out KdlPropertyInfo kdlPropertyInfo,
                        out KdlParameterInfo? kdlParameterInfo
                    )
                )
                {
                    // Set the property value.
                    reader.ReadWithVerify();

                    if (!kdlParameterInfo.ShouldDeserialize)
                    {
                        // The KdlReader.Skip() method will fail fast if it detects that we're reading
                        // from a partially read buffer, regardless of whether the next value is available.
                        // This can result in erroneous failures in cases where a custom converter is calling
                        // into a built-in converter (cf. https://github.com/dotnet/runtime/issues/74108).
                        // For this reason we need to call the TrySkip() method instead -- the serializer
                        // should guarantee sufficient read-ahead has been performed for the current object.
                        bool success = reader.TrySkip();
                        Debug.Assert(
                            success,
                            "Serializer should guarantee sufficient read-ahead has been done."
                        );

                        state.Current.EndConstructorParameter();
                        continue;
                    }

                    Debug.Assert(kdlParameterInfo.MatchingProperty != null);
                    ReadAndCacheConstructorArgument(ref state, ref reader, kdlParameterInfo);

                    state.Current.EndConstructorParameter();
                }
                else
                {
                    if (kdlPropertyInfo.CanDeserialize)
                    {
                        ArgumentState argumentState = state.Current.CtorArgumentState!;

                        if (argumentState.FoundProperties == null)
                        {
                            argumentState.FoundProperties = ArrayPool<FoundProperty>.Shared.Rent(
                                Math.Max(1, state.Current.KdlTypeInfo.PropertyCache.Length)
                            );
                        }
                        else if (
                            argumentState.FoundPropertyCount == argumentState.FoundProperties.Length
                        )
                        {
                            // Rare case where we can't fit all the KDL properties in the rented pool; we have to grow.
                            // This could happen if there are duplicate properties in the KDL.

                            var newCache = ArrayPool<FoundProperty>.Shared.Rent(
                                argumentState.FoundProperties.Length * 2
                            );

                            argumentState.FoundProperties.CopyTo(newCache, 0);

                            FoundProperty[] toReturn = argumentState.FoundProperties;
                            argumentState.FoundProperties = newCache!;

                            ArrayPool<FoundProperty>.Shared.Return(toReturn, clearArray: true);
                        }

                        argumentState.FoundProperties[argumentState.FoundPropertyCount++] = (
                            kdlPropertyInfo,
                            reader.CurrentState,
                            reader.BytesConsumed,
                            state.Current.KdlPropertyName,
                            state.Current.KdlPropertyNameAsString
                        );
                    }

                    reader.SkipWithVerify();
                    state.Current.EndProperty();
                }
            }
        }

        private bool ReadConstructorArgumentsWithContinuation(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlSerializerOptions options
        )
        {
            // Process all properties.
            while (true)
            {
                // Determine the property.
                if (state.Current.PropertyState == StackFramePropertyState.None)
                {
                    if (!reader.Read())
                    {
                        return false;
                    }

                    state.Current.PropertyState = StackFramePropertyState.ReadName;
                }

                KdlParameterInfo? kdlParameterInfo;
                KdlPropertyInfo? kdlPropertyInfo;

                if (state.Current.PropertyState < StackFramePropertyState.Name)
                {
                    KdlTokenType tokenType = reader.TokenType;

                    if (tokenType == KdlTokenType.EndChildrenBlock)
                    {
                        return true;
                    }

                    // Read method would have thrown if otherwise.
                    Debug.Assert(tokenType == KdlTokenType.PropertyName);

                    ReadOnlySpan<byte> unescapedPropertyName = KdlSerializer.GetPropertyName(
                        ref state,
                        ref reader,
                        options,
                        out bool isAlreadyReadMetadataProperty
                    );
                    if (isAlreadyReadMetadataProperty)
                    {
                        Debug.Assert(options.AllowOutOfOrderMetadataProperties);
                        reader.SkipWithVerify();
                        state.Current.EndProperty();
                        continue;
                    }

                    if (
                        TryLookupConstructorParameter(
                            unescapedPropertyName,
                            ref state,
                            options,
                            out kdlPropertyInfo,
                            out kdlParameterInfo
                        )
                    )
                    {
                        kdlPropertyInfo = null;
                    }

                    state.Current.PropertyState = StackFramePropertyState.Name;
                }
                else
                {
                    kdlParameterInfo = state.Current.CtorArgumentState!.KdlParameterInfo;
                    kdlPropertyInfo = state.Current.KdlPropertyInfo;
                }

                if (kdlParameterInfo != null)
                {
                    Debug.Assert(kdlPropertyInfo == null);

                    if (
                        !HandleConstructorArgumentWithContinuation(
                            ref state,
                            ref reader,
                            kdlParameterInfo
                        )
                    )
                    {
                        return false;
                    }
                }
                else
                {
                    if (!HandlePropertyWithContinuation(ref state, ref reader, kdlPropertyInfo!))
                    {
                        return false;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleConstructorArgumentWithContinuation(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlParameterInfo kdlParameterInfo
        )
        {
            if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
            {
                if (!kdlParameterInfo.ShouldDeserialize)
                {
                    if (!reader.TrySkipPartial(targetDepth: state.Current.OriginalDepth + 1))
                    {
                        return false;
                    }

                    state.Current.EndConstructorParameter();
                    return true;
                }

                if (
                    !reader.TryAdvanceWithOptionalReadAhead(
                        kdlParameterInfo.EffectiveConverter.RequiresReadAhead
                    )
                )
                {
                    return false;
                }

                state.Current.PropertyState = StackFramePropertyState.ReadValue;
            }

            if (!ReadAndCacheConstructorArgument(ref state, ref reader, kdlParameterInfo))
            {
                return false;
            }

            state.Current.EndConstructorParameter();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HandlePropertyWithContinuation(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
            {
                if (!kdlPropertyInfo.CanDeserialize)
                {
                    if (!reader.TrySkipPartial(targetDepth: state.Current.OriginalDepth + 1))
                    {
                        return false;
                    }

                    state.Current.EndProperty();
                    return true;
                }

                if (!ReadAheadPropertyValue(ref state, ref reader, kdlPropertyInfo))
                {
                    return false;
                }

                state.Current.PropertyState = StackFramePropertyState.ReadValue;
            }

            object? propValue;

            if (state.Current.UseExtensionProperty)
            {
                if (
                    !kdlPropertyInfo.ReadKdlExtensionDataValue(ref state, ref reader, out propValue)
                )
                {
                    return false;
                }
            }
            else
            {
                if (!kdlPropertyInfo.ReadKdlAsObject(ref state, ref reader, out propValue))
                {
                    return false;
                }
            }

            Debug.Assert(kdlPropertyInfo.CanDeserialize);

            // Ensure that the cache has enough capacity to add this property.

            ArgumentState argumentState = state.Current.CtorArgumentState!;

            if (argumentState.FoundPropertiesAsync == null)
            {
                argumentState.FoundPropertiesAsync = ArrayPool<FoundPropertyAsync>.Shared.Rent(
                    Math.Max(1, state.Current.KdlTypeInfo.PropertyCache.Length)
                );
            }
            else if (argumentState.FoundPropertyCount == argumentState.FoundPropertiesAsync!.Length)
            {
                // Rare case where we can't fit all the KDL properties in the rented pool; we have to grow.
                // This could happen if there are duplicate properties in the KDL.
                var newCache = ArrayPool<FoundPropertyAsync>.Shared.Rent(
                    argumentState.FoundPropertiesAsync!.Length * 2
                );

                argumentState.FoundPropertiesAsync!.CopyTo(newCache, 0);

                FoundPropertyAsync[] toReturn = argumentState.FoundPropertiesAsync!;
                argumentState.FoundPropertiesAsync = newCache!;

                ArrayPool<FoundPropertyAsync>.Shared.Return(toReturn, clearArray: true);
            }

            // Cache the property name and value.
            argumentState.FoundPropertiesAsync![argumentState.FoundPropertyCount++] = (
                kdlPropertyInfo,
                propValue,
                state.Current.KdlPropertyNameAsString
            );

            state.Current.EndProperty();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginRead(scoped ref ReadStack state, KdlSerializerOptions options)
        {
            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;

            kdlTypeInfo.ValidateCanBeUsedForPropertyMetadataSerialization();

            if (kdlTypeInfo.ParameterCount != kdlTypeInfo.ParameterCache.Length)
            {
                ThrowHelper.ThrowInvalidOperationException_ConstructorParameterIncompleteBinding(
                    Type
                );
            }

            state.Current.InitializeRequiredPropertiesValidationState(kdlTypeInfo);

            // Set current KdlPropertyInfo to null to avoid conflicts on push.
            state.Current.KdlPropertyInfo = null;

            Debug.Assert(state.Current.CtorArgumentState != null);

            InitializeConstructorArgumentCaches(ref state, options);
        }

        /// <summary>
        /// Lookup the constructor parameter given its name in the reader.
        /// </summary>
        protected static bool TryLookupConstructorParameter(
            scoped ReadOnlySpan<byte> unescapedPropertyName,
            scoped ref ReadStack state,
            KdlSerializerOptions options,
            out KdlPropertyInfo kdlPropertyInfo,
            [NotNullWhen(true)] out KdlParameterInfo? kdlParameterInfo
        )
        {
            Debug.Assert(state.Current.KdlTypeInfo.Kind is KdlTypeInfoKind.Object);
            Debug.Assert(state.Current.CtorArgumentState != null);

            kdlPropertyInfo = KdlSerializer.LookupProperty(
                obj: null,
                unescapedPropertyName,
                ref state,
                options,
                out bool useExtensionProperty,
                createExtensionProperty: false
            );

            // Mark the property as read from the payload if required.
            state.Current.MarkRequiredPropertyAsRead(kdlPropertyInfo);

            kdlParameterInfo = kdlPropertyInfo.AssociatedParameter;
            if (kdlParameterInfo != null)
            {
                state.Current.KdlPropertyInfo = null;
                state.Current.CtorArgumentState!.KdlParameterInfo = kdlParameterInfo;
                state.Current.NumberHandling = kdlParameterInfo.NumberHandling;
                return true;
            }
            else
            {
                state.Current.UseExtensionProperty = useExtensionProperty;
                return false;
            }
        }
    }
}
