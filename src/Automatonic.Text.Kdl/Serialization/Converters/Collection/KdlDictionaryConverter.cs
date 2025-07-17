using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Base class for dictionary converters such as IDictionary, Hashtable, Dictionary{,} IDictionary{,} and SortedList.
    /// </summary>
    internal abstract class KdlDictionaryConverter<TDictionary> : KdlResumableConverter<TDictionary>
    {
        internal override bool SupportsCreateObjectDelegate => true;
        private protected sealed override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.Dictionary;

        protected internal abstract bool OnWriteResume(KdlWriter writer, TDictionary dictionary, KdlSerializerOptions options, ref WriteStack state);
    }

    /// <summary>
    /// Base class for dictionary converters such as IDictionary, Hashtable, Dictionary{,} IDictionary{,} and SortedList.
    /// </summary>
    internal abstract class KdlDictionaryConverter<TDictionary, TKey, TValue> : KdlDictionaryConverter<TDictionary>
        where TKey : notnull
    {
        /// <summary>
        /// When overridden, adds the value to the collection.
        /// </summary>
        protected abstract void Add(TKey key, in TValue value, KdlSerializerOptions options, ref ReadStack state);

        /// <summary>
        /// When overridden, converts the temporary collection held in state.Current.ReturnValue to the final collection.
        /// This is used with immutable collections.
        /// </summary>
        protected virtual void ConvertCollection(ref ReadStack state, KdlSerializerOptions options) { }

        /// <summary>
        /// When overridden, create the collection. It may be a temporary collection or the final collection.
        /// </summary>
        protected virtual void CreateCollection(ref KdlReader reader, scoped ref ReadStack state)
        {
            if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
            {
                return;
            }

            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;

            if (typeInfo.CreateObject is null)
            {
                ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(typeInfo, ref reader, ref state);
            }

            state.Current.ReturnValue = typeInfo.CreateObject();
            Debug.Assert(state.Current.ReturnValue is TDictionary);
        }

        internal override Type ElementType => typeof(TValue);

        internal override Type KeyType => typeof(TKey);


        protected KdlConverter<TKey>? _keyConverter;
        protected KdlConverter<TValue>? _valueConverter;

        protected static KdlConverter<T> GetConverter<T>(KdlTypeInfo typeInfo)
        {
            return ((KdlTypeInfo<T>)typeInfo).EffectiveConverter;
        }

        internal sealed override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            [MaybeNullWhen(false)] out TDictionary value)
        {
            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;
            KdlTypeInfo keyTypeInfo = kdlTypeInfo.KeyTypeInfo!;
            KdlTypeInfo elementTypeInfo = kdlTypeInfo.ElementTypeInfo!;

            if (!state.SupportContinuation && !state.Current.CanContainMetadata)
            {
                // Fast path that avoids maintaining state variables and dealing with preserved references.

                if (reader.TokenType != KdlTokenType.StartChildrenBlock)
                {
                    ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                }

                CreateCollection(ref reader, ref state);

                kdlTypeInfo.OnDeserializing?.Invoke(state.Current.ReturnValue!);

                _keyConverter ??= GetConverter<TKey>(keyTypeInfo);
                _valueConverter ??= GetConverter<TValue>(elementTypeInfo);

                if (_valueConverter.CanUseDirectReadOrWrite && state.Current.NumberHandling == null)
                {
                    // Process all elements.
                    while (true)
                    {
                        // Read the key name.
                        reader.ReadWithVerify();

                        if (reader.TokenType == KdlTokenType.EndChildrenBlock)
                        {
                            break;
                        }

                        // Read method would have thrown if otherwise.
                        Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);

                        state.Current.KdlPropertyInfo = keyTypeInfo.PropertyInfoForTypeInfo;
                        TKey key = ReadDictionaryKey(_keyConverter, ref reader, ref state, options);

                        // Read the value and add.
                        reader.ReadWithVerify();
                        state.Current.KdlPropertyInfo = elementTypeInfo.PropertyInfoForTypeInfo;
                        TValue? element = _valueConverter.Read(ref reader, ElementType, options);
                        Add(key, element!, options, ref state);
                    }
                }
                else
                {
                    // Process all elements.
                    while (true)
                    {
                        // Read the key name.
                        reader.ReadWithVerify();

                        if (reader.TokenType == KdlTokenType.EndChildrenBlock)
                        {
                            break;
                        }

                        // Read method would have thrown if otherwise.
                        Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
                        state.Current.KdlPropertyInfo = keyTypeInfo.PropertyInfoForTypeInfo;
                        TKey key = ReadDictionaryKey(_keyConverter, ref reader, ref state, options);

                        reader.ReadWithVerify();

                        // Get the value from the converter and add it.
                        state.Current.KdlPropertyInfo = elementTypeInfo.PropertyInfoForTypeInfo;
                        _valueConverter.TryRead(ref reader, ElementType, options, ref state, out TValue? element, out _);
                        Add(key, element!, options, ref state);
                    }
                }
            }
            else
            {
                // Slower path that supports continuation and reading metadata.
                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    if (reader.TokenType != KdlTokenType.StartChildrenBlock)
                    {
                        ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                    }

                    state.Current.ObjectState = StackFrameObjectState.StartToken;
                }

                // Handle the metadata properties.
                if (state.Current.CanContainMetadata && state.Current.ObjectState < StackFrameObjectState.ReadMetadata)
                {
                    if (!KdlSerializer.TryReadMetadata(this, kdlTypeInfo, ref reader, ref state))
                    {
                        value = default;
                        return false;
                    }

                    if (state.Current.MetadataPropertyNames == MetadataPropertyName.Ref)
                    {
                        value = KdlSerializer.ResolveReferenceId<TDictionary>(ref state);
                        return true;
                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadMetadata;
                }

                // Dispatch to any polymorphic converters: should always be entered regardless of ObjectState progress
                if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Type) != 0 &&
                    state.Current.PolymorphicSerializationState != PolymorphicSerializationState.PolymorphicReEntryStarted &&
                    ResolvePolymorphicConverter(kdlTypeInfo, ref state) is KdlConverter polymorphicConverter)
                {
                    Debug.Assert(!IsValueType);
                    bool success = polymorphicConverter.OnTryReadAsObject(ref reader, polymorphicConverter.Type!, options, ref state, out object? objectResult);
                    value = (TDictionary)objectResult!;
                    state.ExitPolymorphicConverter(success);
                    return success;
                }

                // Create the dictionary.
                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    if (state.Current.CanContainMetadata)
                    {
                        KdlSerializer.ValidateMetadataForObjectConverter(ref state);
                    }

                    CreateCollection(ref reader, ref state);

                    if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Id) != 0)
                    {
                        Debug.Assert(state.ReferenceId != null);
                        Debug.Assert(options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve);
                        Debug.Assert(state.Current.ReturnValue is TDictionary);
                        state.ReferenceResolver.AddReference(state.ReferenceId, state.Current.ReturnValue);
                        state.ReferenceId = null;
                    }

                    kdlTypeInfo.OnDeserializing?.Invoke(state.Current.ReturnValue!);

                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                }

                // Process all elements.
                _keyConverter ??= GetConverter<TKey>(keyTypeInfo);
                _valueConverter ??= GetConverter<TValue>(elementTypeInfo);
                while (true)
                {
                    if (state.Current.PropertyState == StackFramePropertyState.None)
                    {
                        // Read the key name.
                        if (!reader.Read())
                        {
                            value = default;
                            return false;
                        }

                        state.Current.PropertyState = StackFramePropertyState.ReadName;
                    }

                    // Determine the property.
                    TKey key;
                    if (state.Current.PropertyState < StackFramePropertyState.Name)
                    {
                        if (reader.TokenType == KdlTokenType.EndChildrenBlock)
                        {
                            break;
                        }

                        // Read method would have thrown if otherwise.
                        Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);

                        state.Current.PropertyState = StackFramePropertyState.Name;

                        if (state.Current.CanContainMetadata)
                        {
                            ReadOnlySpan<byte> propertyName = reader.GetUnescapedSpan();
                            if (KdlSerializer.IsMetadataPropertyName(propertyName, state.Current.BaseKdlTypeInfo.PolymorphicTypeResolver))
                            {
                                if (options.AllowOutOfOrderMetadataProperties)
                                {
                                    reader.SkipWithVerify();
                                    state.Current.EndElement();
                                    continue;
                                }
                                else
                                {
                                    ThrowHelper.ThrowUnexpectedMetadataException(propertyName, ref reader, ref state);
                                }
                            }
                        }

                        state.Current.KdlPropertyInfo = keyTypeInfo.PropertyInfoForTypeInfo;
                        key = ReadDictionaryKey(_keyConverter, ref reader, ref state, options);
                    }
                    else
                    {
                        // DictionaryKey is assigned before all return false cases, null value is unreachable
                        key = (TKey)state.Current.DictionaryKey!;
                    }

                    if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                    {
                        if (!reader.TryAdvanceWithOptionalReadAhead(_valueConverter.RequiresReadAhead))
                        {
                            state.Current.DictionaryKey = key;
                            value = default;
                            return false;
                        }

                        state.Current.PropertyState = StackFramePropertyState.ReadValue;
                    }

                    if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                    {
                        // Get the value from the converter and add it.
                        state.Current.KdlPropertyInfo = elementTypeInfo.PropertyInfoForTypeInfo;
                        bool success = _valueConverter.TryRead(ref reader, typeof(TValue), options, ref state, out TValue? element, out _);
                        if (!success)
                        {
                            state.Current.DictionaryKey = key;
                            value = default;
                            return false;
                        }

                        Add(key, element!, options, ref state);
                        state.Current.EndElement();
                    }
                }
            }

            ConvertCollection(ref state, options);
            object result = state.Current.ReturnValue!;
            kdlTypeInfo.OnDeserialized?.Invoke(result);
            value = (TDictionary)result;

            return true;

            static TKey ReadDictionaryKey(KdlConverter<TKey> keyConverter, ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
            {
                TKey key;
                string unescapedPropertyNameAsString = reader.GetString()!;
                state.Current.KdlPropertyNameAsString = unescapedPropertyNameAsString; // Copy key name for KDL Path support in case of error.

                // Special case string to avoid calling GetString twice and save one allocation.
                if (keyConverter.IsInternalConverter && keyConverter.Type == typeof(string))
                {
                    key = (TKey)(object)unescapedPropertyNameAsString;
                }
                else
                {
                    key = keyConverter.ReadAsPropertyNameCore(ref reader, keyConverter.Type, options);
                }

                return key;
            }
        }

        internal sealed override bool OnTryWrite(
            KdlWriter writer,
            TDictionary dictionary,
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            if (dictionary == null)
            {
                writer.WriteNullValue();
                return true;
            }

            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;

            if (!state.Current.ProcessedStartToken)
            {
                state.Current.ProcessedStartToken = true;

                kdlTypeInfo.OnSerializing?.Invoke(dictionary);

                writer.WriteStartObject();

                if (state.CurrentContainsMetadata && CanHaveMetadata)
                {
                    KdlSerializer.WriteMetadataForObject(this, ref state, writer);
                }

                state.Current.KdlPropertyInfo = kdlTypeInfo.ElementTypeInfo!.PropertyInfoForTypeInfo;
            }

            bool success = OnWriteResume(writer, dictionary, options, ref state);
            if (success)
            {
                if (!state.Current.ProcessedEndToken)
                {
                    state.Current.ProcessedEndToken = true;
                    writer.WriteEndObject();
                }

                kdlTypeInfo.OnSerialized?.Invoke(dictionary);
            }

            return success;
        }
    }
}
