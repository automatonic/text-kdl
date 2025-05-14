using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Base class for all collections. Collections are assumed to implement <see cref="IEnumerable{T}"/>
    /// or a variant thereof e.g. <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    internal abstract class KdlCollectionConverter<TCollection, TElement> : KdlResumableConverter<TCollection>
    {
        internal override bool SupportsCreateObjectDelegate => true;
        private protected sealed override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.Enumerable;
        internal override Type ElementType => typeof(TElement);

        protected abstract void Add(in TElement value, ref ReadStack state);

        /// <summary>
        /// When overridden, create the collection. It may be a temporary collection or the final collection.
        /// </summary>
        protected virtual void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
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
            Debug.Assert(state.Current.ReturnValue is TCollection);
        }

        /// <summary>
        /// When overridden, converts the temporary collection held in state.Current.ReturnValue to the final collection.
        /// The <see cref="KdlConverter.IsConvertibleCollection"/> property must also be set to <see langword="true"/>.
        /// </summary>
        protected virtual void ConvertCollection(ref ReadStack state, KdlSerializerOptions options) { }

        protected static KdlConverter<TElement> GetElementConverter(KdlTypeInfo elementTypeInfo)
        {
            return ((KdlTypeInfo<TElement>)elementTypeInfo).EffectiveConverter;
        }

        protected static KdlConverter<TElement> GetElementConverter(ref WriteStack state)
        {
            Debug.Assert(state.Current.KdlPropertyInfo != null);
            return (KdlConverter<TElement>)state.Current.KdlPropertyInfo.EffectiveConverter;
        }

        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            [MaybeNullWhen(false)] out TCollection value)
        {
            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;
            KdlTypeInfo elementTypeInfo = kdlTypeInfo.ElementTypeInfo!;

            if (!state.SupportContinuation && !state.Current.CanContainMetadata)
            {
                // Fast path that avoids maintaining state variables and dealing with preserved references.

                if (reader.TokenType != KdlTokenType.StartArray)
                {
                    ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                }

                CreateCollection(ref reader, ref state, options);

                kdlTypeInfo.OnDeserializing?.Invoke(state.Current.ReturnValue!);

                state.Current.KdlPropertyInfo = elementTypeInfo.PropertyInfoForTypeInfo;
                KdlConverter<TElement> elementConverter = GetElementConverter(elementTypeInfo);
                if (elementConverter.CanUseDirectReadOrWrite && state.Current.NumberHandling == null)
                {
                    // Fast path that avoids validation and extra indirection.
                    while (true)
                    {
                        reader.ReadWithVerify();
                        if (reader.TokenType == KdlTokenType.EndArray)
                        {
                            break;
                        }

                        // Obtain the CLR value from the KDL and apply to the object.
                        TElement? element = elementConverter.Read(ref reader, elementConverter.Type, options);
                        Add(element!, ref state);
                    }
                }
                else
                {
                    // Process all elements.
                    while (true)
                    {
                        reader.ReadWithVerify();
                        if (reader.TokenType == KdlTokenType.EndArray)
                        {
                            break;
                        }

                        // Get the value from the converter and add it.
                        elementConverter.TryRead(ref reader, typeof(TElement), options, ref state, out TElement? element, out _);
                        Add(element!, ref state);
                    }
                }
            }
            else
            {
                // Slower path that supports continuation and reading metadata.
                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    if (reader.TokenType == KdlTokenType.StartArray)
                    {
                        state.Current.ObjectState = StackFrameObjectState.ReadMetadata;
                    }
                    else if (state.Current.CanContainMetadata)
                    {
                        if (reader.TokenType != KdlTokenType.StartObject)
                        {
                            ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                        }

                        state.Current.ObjectState = StackFrameObjectState.StartToken;
                    }
                    else
                    {
                        ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                    }
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
                        value = KdlSerializer.ResolveReferenceId<TCollection>(ref state);
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
                    value = (TCollection)objectResult!;
                    state.ExitPolymorphicConverter(success);
                    return success;
                }

                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    if (state.Current.CanContainMetadata)
                    {
                        KdlSerializer.ValidateMetadataForArrayConverter(this, ref reader, ref state);
                    }

                    CreateCollection(ref reader, ref state, options);

                    if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Id) != 0)
                    {
                        Debug.Assert(state.ReferenceId != null);
                        Debug.Assert(options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve);
                        Debug.Assert(state.Current.ReturnValue is TCollection);
                        state.ReferenceResolver.AddReference(state.ReferenceId, state.Current.ReturnValue);
                        state.ReferenceId = null;
                    }

                    kdlTypeInfo.OnDeserializing?.Invoke(state.Current.ReturnValue!);

                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                }

                if (state.Current.ObjectState < StackFrameObjectState.ReadElements)
                {
                    KdlConverter<TElement> elementConverter = GetElementConverter(elementTypeInfo);
                    state.Current.KdlPropertyInfo = elementTypeInfo.PropertyInfoForTypeInfo;

                    // Process all elements.
                    while (true)
                    {
                        if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                        {
                            if (!reader.TryAdvanceWithOptionalReadAhead(elementConverter.RequiresReadAhead))
                            {
                                value = default;
                                return false;
                            }

                            state.Current.PropertyState = StackFramePropertyState.ReadValue;
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadValueIsEnd)
                        {
                            if (reader.TokenType == KdlTokenType.EndArray)
                            {
                                break;
                            }

                            state.Current.PropertyState = StackFramePropertyState.ReadValueIsEnd;
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                        {
                            // Get the value from the converter and add it.
                            if (!elementConverter.TryRead(ref reader, typeof(TElement), options, ref state, out TElement? element, out _))
                            {
                                value = default;
                                return false;
                            }

                            Add(element!, ref state);

                            // No need to set PropertyState to TryRead since we're done with this element now.
                            state.Current.EndElement();
                        }
                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadElements;
                }

                if (state.Current.ObjectState < StackFrameObjectState.EndToken)
                {
                    // Array payload is nested inside a $values metadata property.
                    if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Values) != 0)
                    {
                        if (!reader.Read())
                        {
                            value = default;
                            return false;
                        }
                    }

                    state.Current.ObjectState = StackFrameObjectState.EndToken;
                }

                if (state.Current.ObjectState < StackFrameObjectState.EndTokenValidation)
                {
                    // Array payload is nested inside a $values metadata property.
                    if ((state.Current.MetadataPropertyNames & MetadataPropertyName.Values) != 0)
                    {
                        if (reader.TokenType != KdlTokenType.EndObject)
                        {
                            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
                            if (options.AllowOutOfOrderMetadataProperties)
                            {
                                Debug.Assert(KdlSerializer.IsMetadataPropertyName(reader.GetUnescapedSpan(), (state.Current.BaseKdlTypeInfo ?? kdlTypeInfo).PolymorphicTypeResolver), "should only be hit if metadata property.");
                                bool result = reader.TrySkipPartial(reader.CurrentDepth - 1); // skip to the end of the object
                                Debug.Assert(result, "Metadata reader must have buffered all contents.");
                                Debug.Assert(reader.TokenType is KdlTokenType.EndObject);
                            }
                            else
                            {
                                ThrowHelper.ThrowKdlException_MetadataInvalidPropertyInArrayMetadata(ref state, typeToConvert, reader);
                            }
                        }
                    }
                }
            }

            ConvertCollection(ref state, options);
            object returnValue = state.Current.ReturnValue!;
            kdlTypeInfo.OnDeserialized?.Invoke(returnValue);
            value = (TCollection)returnValue;

            return true;
        }

        internal override bool OnTryWrite(
            KdlWriter writer,
            TCollection value,
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            bool success;

            if (value == null)
            {
                writer.WriteNullValue();
                success = true;
            }
            else
            {
                KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;

                if (!state.Current.ProcessedStartToken)
                {
                    state.Current.ProcessedStartToken = true;

                    kdlTypeInfo.OnSerializing?.Invoke(value);

                    if (state.CurrentContainsMetadata && CanHaveMetadata)
                    {
                        state.Current.MetadataPropertyName = KdlSerializer.WriteMetadataForCollection(this, ref state, writer);
                    }

                    // Writing the start of the array must happen after any metadata
                    writer.WriteStartArray();
                    state.Current.KdlPropertyInfo = kdlTypeInfo.ElementTypeInfo!.PropertyInfoForTypeInfo;
                }

                success = OnWriteResume(writer, value, options, ref state);
                if (success)
                {
                    if (!state.Current.ProcessedEndToken)
                    {
                        state.Current.ProcessedEndToken = true;
                        writer.WriteEndArray();

                        if (state.Current.MetadataPropertyName != 0)
                        {
                            // Write the EndObject for $values.
                            writer.WriteEndObject();
                        }
                    }

                    kdlTypeInfo.OnSerialized?.Invoke(value);
                }
            }

            return success;
        }

        protected abstract bool OnWriteResume(KdlWriter writer, TCollection value, KdlSerializerOptions options, ref WriteStack state);
    }
}
