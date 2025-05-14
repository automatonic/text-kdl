using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization
{
    public partial class KdlConverter
    {
        /// <summary>
        /// Initializes the state for polymorphic cases and returns the appropriate derived converter.
        /// </summary>
        internal KdlConverter? ResolvePolymorphicConverter(KdlTypeInfo kdlTypeInfo, ref ReadStack state)
        {
            Debug.Assert(!IsValueType);
            Debug.Assert(CanHaveMetadata);
            Debug.Assert((state.Current.MetadataPropertyNames & MetadataPropertyName.Type) != 0);
            Debug.Assert(state.Current.PolymorphicSerializationState != PolymorphicSerializationState.PolymorphicReEntryStarted);
            Debug.Assert(kdlTypeInfo.PolymorphicTypeResolver?.UsesTypeDiscriminators == true);

            KdlConverter? polymorphicConverter = null;

            switch (state.Current.PolymorphicSerializationState)
            {
                case PolymorphicSerializationState.None:
                    Debug.Assert(!state.IsContinuation);
                    Debug.Assert(state.PolymorphicTypeDiscriminator != null);

                    PolymorphicTypeResolver resolver = kdlTypeInfo.PolymorphicTypeResolver;
                    if (resolver.TryGetDerivedKdlTypeInfo(state.PolymorphicTypeDiscriminator, out KdlTypeInfo? resolvedType))
                    {
                        Debug.Assert(Type!.IsAssignableFrom(resolvedType.Type));

                        polymorphicConverter = state.InitializePolymorphicReEntry(resolvedType);
                        if (!polymorphicConverter.CanHaveMetadata)
                        {
                            ThrowHelper.ThrowNotSupportedException_DerivedConverterDoesNotSupportMetadata(resolvedType.Type);
                        }
                    }
                    else
                    {
                        state.Current.PolymorphicSerializationState = PolymorphicSerializationState.PolymorphicReEntryNotFound;
                    }

                    state.PolymorphicTypeDiscriminator = null;
                    break;

                case PolymorphicSerializationState.PolymorphicReEntrySuspended:
                    polymorphicConverter = state.ResumePolymorphicReEntry();
                    Debug.Assert(Type!.IsAssignableFrom(polymorphicConverter.Type));
                    break;

                case PolymorphicSerializationState.PolymorphicReEntryNotFound:
                    Debug.Assert(state.Current.PolymorphicKdlTypeInfo is null);
                    break;

                default:
                    Debug.Fail("Unexpected PolymorphicSerializationState.");
                    break;
            }

            return polymorphicConverter;
        }

        /// <summary>
        /// Initializes the state for polymorphic cases and returns the appropriate derived converter.
        /// </summary>
        internal KdlConverter? ResolvePolymorphicConverter(object value, KdlTypeInfo kdlTypeInfo, KdlSerializerOptions options, ref WriteStack state)
        {
            Debug.Assert(!IsValueType);
            Debug.Assert(value != null && Type!.IsAssignableFrom(value.GetType()));
            Debug.Assert(CanBePolymorphic || kdlTypeInfo.PolymorphicTypeResolver != null);
            Debug.Assert(state.PolymorphicTypeDiscriminator is null);

            KdlConverter? polymorphicConverter = null;

            switch (state.Current.PolymorphicSerializationState)
            {
                case PolymorphicSerializationState.None:
                    Debug.Assert(!state.IsContinuation);

                    Type runtimeType = value.GetType();

                    if (CanBePolymorphic && runtimeType != Type)
                    {
                        Debug.Assert(Type == typeof(object));
                        kdlTypeInfo = state.Current.InitializePolymorphicReEntry(runtimeType, options);
                        polymorphicConverter = kdlTypeInfo.Converter;
                    }

                    if (kdlTypeInfo.PolymorphicTypeResolver is PolymorphicTypeResolver resolver)
                    {
                        Debug.Assert(kdlTypeInfo.Converter.CanHaveMetadata);

                        if (resolver.TryGetDerivedKdlTypeInfo(runtimeType, out KdlTypeInfo? derivedKdlTypeInfo, out object? typeDiscriminator))
                        {
                            polymorphicConverter = state.Current.InitializePolymorphicReEntry(derivedKdlTypeInfo);

                            if (typeDiscriminator is not null)
                            {
                                if (!polymorphicConverter.CanHaveMetadata)
                                {
                                    ThrowHelper.ThrowNotSupportedException_DerivedConverterDoesNotSupportMetadata(derivedKdlTypeInfo.Type);
                                }

                                state.PolymorphicTypeDiscriminator = typeDiscriminator;
                                state.PolymorphicTypeResolver = resolver;
                            }
                        }
                    }

                    if (polymorphicConverter is null)
                    {
                        state.Current.PolymorphicSerializationState = PolymorphicSerializationState.PolymorphicReEntryNotFound;
                    }

                    break;

                case PolymorphicSerializationState.PolymorphicReEntrySuspended:
                    Debug.Assert(state.IsContinuation);
                    polymorphicConverter = state.Current.ResumePolymorphicReEntry();
                    Debug.Assert(Type.IsAssignableFrom(polymorphicConverter.Type));
                    break;

                case PolymorphicSerializationState.PolymorphicReEntryNotFound:
                    Debug.Assert(state.IsContinuation);
                    break;

                default:
                    Debug.Fail("Unexpected PolymorphicSerializationState.");
                    break;
            }

            return polymorphicConverter;
        }

        internal bool TryHandleSerializedObjectReference(KdlWriter writer, object value, KdlSerializerOptions options, KdlConverter? polymorphicConverter, ref WriteStack state)
        {
            Debug.Assert(!IsValueType);
            Debug.Assert(!state.IsContinuation);
            Debug.Assert(value != null);

            switch (options.ReferenceHandlingStrategy)
            {
                case KdlKnownReferenceHandler.IgnoreCycles:
                    ReferenceResolver resolver = state.ReferenceResolver;
                    if (resolver.ContainsReferenceForCycleDetection(value))
                    {
                        writer.WriteNullValue();
                        return true;
                    }

                    resolver.PushReferenceForCycleDetection(value);
                    // WriteStack reuses root-level stack frames for its children as a performance optimization;
                    // we want to avoid writing any data for the root-level object to avoid corrupting the stack.
                    // This is fine since popping the root object at the end of serialization is not essential.
                    state.Current.IsPushedReferenceForCycleDetection = state.CurrentDepth > 0;
                    break;

                case KdlKnownReferenceHandler.Preserve:
                    bool canHaveIdMetadata = polymorphicConverter?.CanHaveMetadata ?? CanHaveMetadata;
                    if (canHaveIdMetadata && KdlSerializer.TryGetReferenceForValue(value, ref state, writer))
                    {
                        // We found a repeating reference and wrote the relevant metadata; serialization complete.
                        return true;
                    }
                    break;

                default:
                    Debug.Fail("Unexpected ReferenceHandlingStrategy.");
                    break;
            }

            return false;
        }
    }
}
