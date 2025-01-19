using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Converters;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Converts an object or value to or from KDL.
    /// </summary>
    /// <typeparam name="T">The <see cref="System.Type"/> to convert.</typeparam>
    public abstract partial class KdlConverter<T> : KdlConverter
    {
        /// <summary>
        /// When overridden, constructs a new <see cref="KdlConverter{T}"/> instance.
        /// </summary>
        protected internal KdlConverter()
        {
            IsValueType = typeof(T).IsValueType;

            if (HandleNull)
            {
                HandleNullOnRead = true;
                HandleNullOnWrite = true;
            }
            else if (UsesDefaultHandleNull)
            {
                // If the type doesn't support null, allow the converter a chance to modify.
                // These semantics are backwards compatible with 3.0.
                HandleNullOnRead = default(T) is not null;

                // The framework handles null automatically on writes.
                HandleNullOnWrite = false;
            }
        }

        /// <summary>
        /// Determines whether the type can be converted.
        /// </summary>
        /// <remarks>
        /// The default implementation is to return True when <paramref name="typeToConvert"/> equals typeof(T).
        /// </remarks>
        /// <param name="typeToConvert"></param>
        /// <returns>True if the type can be converted, False otherwise.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(T);
        }

        private protected override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.Value;

        internal sealed override KdlTypeInfo CreateKdlTypeInfo(KdlSerializerOptions options)
        {
            return new KdlTypeInfo<T>(this, options);
        }

        /// <summary>
        /// Indicates whether <see langword="null"/> should be passed to the converter on serialization,
        /// and whether <see cref="KdlTokenType.Null"/> should be passed on deserialization.
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="true"/> for converters based on value types, and <see langword="false"/> for converters based on reference types.
        /// </remarks>
        public virtual bool HandleNull
        {
            get
            {
                UsesDefaultHandleNull = true;
                return false;
            }
        }

        // This non-generic API is sealed as it just forwards to the generic version.
        internal sealed override void WriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options)
        {
            T valueOfT = KdlSerializer.UnboxOnWrite<T>(value)!;
            Write(writer, valueOfT, options);
        }

        // This non-generic API is sealed as it just forwards to the generic version.
        internal sealed override bool OnTryWriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options, ref WriteStack state)
        {
            T valueOfT = KdlSerializer.UnboxOnWrite<T>(value)!;
            return OnTryWrite(writer, valueOfT, options, ref state);
        }

        // This non-generic API is sealed as it just forwards to the generic version.
        internal sealed override void WriteAsPropertyNameAsObject(KdlWriter writer, object? value, KdlSerializerOptions options)
        {
            T valueOfT = KdlSerializer.UnboxOnWrite<T>(value)!;
            WriteAsPropertyName(writer, valueOfT, options);
        }

        internal sealed override void WriteAsPropertyNameCoreAsObject(KdlWriter writer, object? value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            T valueOfT = KdlSerializer.UnboxOnWrite<T>(value)!;
            WriteAsPropertyNameCore(writer, valueOfT, options, isWritingExtensionDataProperty);
        }

        internal sealed override void WriteNumberWithCustomHandlingAsObject(KdlWriter writer, object? value, KdlNumberHandling handling)
        {
            T valueOfT = KdlSerializer.UnboxOnWrite<T>(value)!;
            WriteNumberWithCustomHandling(writer, valueOfT, handling);
        }

        // This non-generic API is sealed as it just forwards to the generic version.
        internal sealed override bool TryWriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options, ref WriteStack state)
        {
            T valueOfT = KdlSerializer.UnboxOnWrite<T>(value)!;
            return TryWrite(writer, valueOfT, options, ref state);
        }

        // Provide a default implementation for value converters.
        internal virtual bool OnTryWrite(KdlWriter writer,
#nullable disable // T may or may not be nullable depending on the derived converter's HandleNull override.
            T value,
#nullable enable
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            Write(writer, value, options);
            return true;
        }

        // Provide a default implementation for value converters.
        internal virtual bool OnTryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out T? value)
        {
            value = Read(ref reader, typeToConvert, options);
            return true;
        }

        /// <summary>
        /// Read and convert the KDL to T.
        /// </summary>
        /// <remarks>
        /// A converter may throw any Exception, but should throw <cref>KdlException</cref> when the KDL is invalid.
        /// </remarks>
        /// <param name="reader">The <see cref="KdlReader"/> to read from.</param>
        /// <param name="typeToConvert">The <see cref="System.Type"/> being converted.</param>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> being used.</param>
        /// <returns>The value that was converted.</returns>
        /// <remarks>Note that the value of <seealso cref="HandleNull"/> determines if the converter handles null KDL tokens.</remarks>
        public abstract T? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options);

        internal bool TryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out T? value, out bool isPopulatedValue)
        {
            // For perf and converter simplicity, handle null here instead of forwarding to the converter.
            if (reader.TokenType == KdlTokenType.Null && !HandleNullOnRead && !state.IsContinuation)
            {
                if (default(T) is not null)
                {
                    ThrowHelper.ThrowKdlException_DeserializeUnableToConvertValue(Type);
                }

                value = default;
                isPopulatedValue = false;
                return true;
            }

            if (ConverterStrategy == ConverterStrategy.Value)
            {
                // A value converter should never be within a continuation.
                Debug.Assert(!state.IsContinuation);
#if !DEBUG
                // For performance, only perform validation on internal converters on debug builds.
                if (IsInternalConverter)
                {
                    if (state.Current.NumberHandling != null && IsInternalConverterForNumberType)
                    {
                        value = ReadNumberWithCustomHandling(ref reader, state.Current.NumberHandling.Value, options);
                    }
                    else
                    {
                        value = Read(ref reader, typeToConvert, options);
                    }
                }
                else
#endif
                {
                    KdlTokenType originalPropertyTokenType = reader.TokenType;
                    int originalPropertyDepth = reader.CurrentDepth;
                    long originalPropertyBytesConsumed = reader.BytesConsumed;

                    if (state.Current.NumberHandling != null && IsInternalConverterForNumberType)
                    {
                        value = ReadNumberWithCustomHandling(ref reader, state.Current.NumberHandling.Value, options);
                    }
                    else
                    {
                        value = Read(ref reader, typeToConvert, options);
                    }

                    VerifyRead(
                        originalPropertyTokenType,
                        originalPropertyDepth,
                        originalPropertyBytesConsumed,
                        isValueConverter: true,
                        ref reader);
                }

                isPopulatedValue = false;
                return true;
            }

            Debug.Assert(IsInternalConverter);
            bool isContinuation = state.IsContinuation;
            bool success;

            if (
#if NET
                !typeof(T).IsValueType &&
#endif
                CanBePolymorphic)
            {
                // Special case object converters since they don't
                // require the expensive ReadStack.Push()/Pop() operations.
                Debug.Assert(this is ObjectConverter);
                success = OnTryRead(ref reader, typeToConvert, options, ref state, out value);
                Debug.Assert(success);
                isPopulatedValue = false;
                return true;
            }

            KdlPropertyInfo? propertyInfo = state.Current.KdlPropertyInfo;
            object? parentObj = state.Current.ReturnValue;

#if DEBUG
            // DEBUG: ensure push/pop operations preserve stack integrity
            KdlTypeInfo originalKdlTypeInfo = state.Current.KdlTypeInfo;
#endif
            state.Push();
            Debug.Assert(Type == state.Current.KdlTypeInfo.Type);

            if (!isContinuation)
            {
#if DEBUG
                // For performance, only perform token type validation of converters on debug builds.
                Debug.Assert(state.Current.OriginalTokenType == KdlTokenType.None);
                state.Current.OriginalTokenType = reader.TokenType;
#endif
                Debug.Assert(state.Current.OriginalDepth == 0);
                state.Current.OriginalDepth = reader.CurrentDepth;
            }

            if (parentObj != null && propertyInfo != null && !propertyInfo.IsForTypeInfo)
            {
                state.Current.HasParentObject = true;
            }

            success = OnTryRead(ref reader, typeToConvert, options, ref state, out value);
#if DEBUG
            if (success)
            {
                if (state.IsContinuation)
                {
                    // The resumable converter did not forward to the next converter that previously returned false.
                    ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                }

                VerifyRead(
                    state.Current.OriginalTokenType,
                    state.Current.OriginalDepth,
                    bytesConsumed: 0,
                    isValueConverter: false,
                    ref reader);

                // No need to clear state.Current.* since a stack pop will occur.
            }
#endif

            isPopulatedValue = state.Current.IsPopulating;
            state.Pop(success);
#if DEBUG
            Debug.Assert(ReferenceEquals(originalKdlTypeInfo, state.Current.KdlTypeInfo));
#endif
            return success;
        }

        internal sealed override bool OnTryReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out object? value)
        {
            bool success = OnTryRead(ref reader, typeToConvert, options, ref state, out T? typedValue);
            value = typedValue;
            return success;
        }

        internal sealed override bool TryReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out object? value)
        {
            bool success = TryRead(ref reader, typeToConvert, options, ref state, out T? typedValue, out _);
            value = typedValue;
            return success;
        }

        internal sealed override object? ReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            T? typedValue = Read(ref reader, typeToConvert, options);
            return typedValue;
        }

        internal sealed override object? ReadAsPropertyNameAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            T typedValue = ReadAsPropertyName(ref reader, typeToConvert, options);
            return typedValue;
        }

        internal sealed override object? ReadAsPropertyNameCoreAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            T typedValue = ReadAsPropertyNameCore(ref reader, typeToConvert, options);
            return typedValue;
        }

        internal sealed override object? ReadNumberWithCustomHandlingAsObject(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            T typedValue = ReadNumberWithCustomHandling(ref reader, handling, options);
            return typedValue;
        }

        /// <summary>
        /// Performance optimization.
        /// The 'in' modifier in 'TryWrite(in T Value)' causes boxing for Nullable{T}, so this helper avoids that.
        /// TODO: Remove this work-around once https://github.com/dotnet/runtime/issues/50915 is addressed.
        /// </summary>
        private static bool IsNull(T? value) => value is null;

        internal bool TryWrite(KdlWriter writer, in T? value, KdlSerializerOptions options, ref WriteStack state)
        {
            if (writer.CurrentDepth >= options.EffectiveMaxDepth)
            {
                ThrowHelper.ThrowKdlException_SerializerCycleDetected(options.EffectiveMaxDepth);
            }

            if (default(T) is null && !HandleNullOnWrite && IsNull(value))
            {
                // We do not pass null values to converters unless HandleNullOnWrite is true. Null values for properties were
                // already handled in GetMemberAndWriteKdl() so we don't need to check for IgnoreNullValues here.
                writer.WriteNullValue();
                return true;
            }

            if (ConverterStrategy == ConverterStrategy.Value)
            {
                Debug.Assert(!state.IsContinuation);

                int originalPropertyDepth = writer.CurrentDepth;

                if (state.Current.NumberHandling != null && IsInternalConverterForNumberType)
                {
                    WriteNumberWithCustomHandling(writer, value, state.Current.NumberHandling.Value);
                }
                else
                {
                    Write(writer, value, options);
                }

                VerifyWrite(originalPropertyDepth, writer);
                return true;
            }

            Debug.Assert(IsInternalConverter);
            bool isContinuation = state.IsContinuation;
            bool success;

            if (
#if NET
                // Short-circuit the check against "is not null"; treated as a constant by recent versions of the JIT.
                !typeof(T).IsValueType &&
#else
                !IsValueType &&
#endif
                value is not null &&
                // Do not handle objects that have already been
                // handled by a polymorphic converter for a base type.
                state.Current.PolymorphicSerializationState != PolymorphicSerializationState.PolymorphicReEntryStarted)
            {
                KdlTypeInfo jsonTypeInfo = state.PeekNestedKdlTypeInfo();
                Debug.Assert(jsonTypeInfo.Converter.Type == Type);

                bool canBePolymorphic = CanBePolymorphic || jsonTypeInfo.PolymorphicTypeResolver is not null;
                KdlConverter? polymorphicConverter = canBePolymorphic ?
                    ResolvePolymorphicConverter(value, jsonTypeInfo, options, ref state) :
                    null;

                if (!isContinuation && options.ReferenceHandlingStrategy != KdlKnownReferenceHandler.Unspecified &&
                    TryHandleSerializedObjectReference(writer, value, options, polymorphicConverter, ref state))
                {
                    // The reference handler wrote reference metadata, serialization complete.
                    return true;
                }

                if (polymorphicConverter is not null)
                {
                    success = polymorphicConverter.TryWriteAsObject(writer, value, options, ref state);
                    state.Current.ExitPolymorphicConverter(success);

                    if (success)
                    {
                        if (state.Current.IsPushedReferenceForCycleDetection)
                        {
                            state.ReferenceResolver.PopReferenceForCycleDetection();
                            state.Current.IsPushedReferenceForCycleDetection = false;
                        }
                    }

                    return success;
                }
            }

#if DEBUG
            // DEBUG: ensure push/pop operations preserve stack integrity
            KdlTypeInfo originalKdlTypeInfo = state.Current.KdlTypeInfo;
#endif
            state.Push();
            Debug.Assert(Type == state.Current.KdlTypeInfo.Type);

#if DEBUG
            // For performance, only perform validation on internal converters on debug builds.
            if (!isContinuation)
            {
                Debug.Assert(state.Current.OriginalDepth == 0);
                state.Current.OriginalDepth = writer.CurrentDepth;
            }
#endif
            success = OnTryWrite(writer, value, options, ref state);
#if DEBUG
            if (success)
            {
                VerifyWrite(state.Current.OriginalDepth, writer);
            }
#endif
            state.Pop(success);

            if (success && state.Current.IsPushedReferenceForCycleDetection)
            {
                state.ReferenceResolver.PopReferenceForCycleDetection();
                state.Current.IsPushedReferenceForCycleDetection = false;
            }
#if DEBUG
            Debug.Assert(ReferenceEquals(originalKdlTypeInfo, state.Current.KdlTypeInfo));
#endif
            return success;
        }

        internal bool TryWriteDataExtensionProperty(KdlWriter writer, T value, KdlSerializerOptions options, ref WriteStack state)
        {
            Debug.Assert(value != null);

            if (!IsInternalConverter)
            {
                return TryWrite(writer, value, options, ref state);
            }

            KdlDictionaryConverter<T>? dictionaryConverter = this as KdlDictionaryConverter<T>
                ?? (this as KdlMetadataServicesConverter<T>)?.Converter as KdlDictionaryConverter<T>;

            if (dictionaryConverter == null)
            {
                // If not KdlDictionaryConverter<T> then we are KdlNode.
                // Avoid a type reference to KdlNode and its converter to support trimming.
                Debug.Assert(Type == typeof(Nodes.KdlNode));
                return TryWrite(writer, value, options, ref state);
            }

            if (writer.CurrentDepth >= options.EffectiveMaxDepth)
            {
                ThrowHelper.ThrowKdlException_SerializerCycleDetected(options.EffectiveMaxDepth);
            }

            bool isContinuation = state.IsContinuation;
            bool success;

            state.Push();

            if (!isContinuation)
            {
                Debug.Assert(state.Current.OriginalDepth == 0);
                state.Current.OriginalDepth = writer.CurrentDepth;
            }

            // Extension data properties change how dictionary key naming policies are applied.
            state.Current.IsWritingExtensionDataProperty = true;
            state.Current.KdlPropertyInfo = state.Current.KdlTypeInfo.ElementTypeInfo!.PropertyInfoForTypeInfo;

            success = dictionaryConverter.OnWriteResume(writer, value, options, ref state);
            if (success)
            {
                VerifyWrite(state.Current.OriginalDepth, writer);
            }

            state.Pop(success);

            return success;
        }

        /// <inheritdoc/>
        public sealed override Type Type { get; } = typeof(T);

        internal void VerifyRead(KdlTokenType tokenType, int depth, long bytesConsumed, bool isValueConverter, ref KdlReader reader)
        {
            Debug.Assert(isValueConverter == (ConverterStrategy == ConverterStrategy.Value));

            switch (tokenType)
            {
                case KdlTokenType.StartArray:
                    if (reader.TokenType != KdlTokenType.EndArray)
                    {
                        ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                    }
                    else if (depth != reader.CurrentDepth)
                    {
                        ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                    }

                    break;

                case KdlTokenType.StartObject:
                    if (reader.TokenType != KdlTokenType.EndObject)
                    {
                        ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                    }
                    else if (depth != reader.CurrentDepth)
                    {
                        ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                    }

                    break;

                case KdlTokenType.None:
                    Debug.Assert(IsRootLevelMultiContentStreamingConverter);
                    break;

                default:
                    if (isValueConverter)
                    {
                        // A value converter should not make any reads.
                        if (reader.BytesConsumed != bytesConsumed)
                        {
                            ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                        }
                    }
                    else
                    {
                        // A non-value converter (object or collection) should always have Start and End tokens
                        // unless it is polymorphic or supports null value reads.
                        if (!CanBePolymorphic && !(HandleNullOnRead && tokenType == KdlTokenType.Null))
                        {
                            ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
                        }
                    }

                    // Should not be possible to change token type.
                    Debug.Assert(reader.TokenType == tokenType);
                    break;
            }
        }

        internal void VerifyWrite(int originalDepth, KdlWriter writer)
        {
            if (originalDepth != writer.CurrentDepth)
            {
                ThrowHelper.ThrowKdlException_SerializationConverterWrite(this);
            }
        }

        /// <summary>
        /// Write the value as KDL.
        /// </summary>
        /// <remarks>
        /// A converter may throw any Exception, but should throw <cref>KdlException</cref> when the KDL
        /// cannot be created.
        /// </remarks>
        /// <param name="writer">The <see cref="KdlWriter"/> to write to.</param>
        /// <param name="value">The value to convert. Note that the value of <seealso cref="HandleNull"/> determines if the converter handles <see langword="null" /> values.</param>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> being used.</param>
        public abstract void Write(
            KdlWriter writer,
#nullable disable // T may or may not be nullable depending on the derived converter's HandleNull override.
            T value,
#nullable restore
            KdlSerializerOptions options);

        /// <summary>
        /// Reads a dictionary key from a KDL property name.
        /// </summary>
        /// <param name="reader">The <see cref="KdlReader"/> to read from.</param>
        /// <param name="typeToConvert">The <see cref="System.Type"/> being converted.</param>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> being used.</param>
        /// <returns>The value that was converted.</returns>
        /// <remarks>Method should be overridden in custom converters of types used in deserialized dictionary keys.</remarks>
        public virtual T ReadAsPropertyName(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            // .NET 5 backward compatibility: hardcode the default converter for primitive key serialization.
            KdlConverter<T>? fallbackConverter = GetFallbackConverterForPropertyNameSerialization(options);
            if (fallbackConverter is null)
            {
                ThrowHelper.ThrowNotSupportedException_DictionaryKeyTypeNotSupported(Type, this);
            }

            return fallbackConverter.ReadAsPropertyNameCore(ref reader, typeToConvert, options);
        }

        internal virtual T ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);

            long originalBytesConsumed = reader.BytesConsumed;
            T result = ReadAsPropertyName(ref reader, typeToConvert, options);
            if (reader.BytesConsumed != originalBytesConsumed)
            {
                ThrowHelper.ThrowKdlException_SerializationConverterRead(this);
            }

            return result;
        }

        /// <summary>
        /// Writes a dictionary key as a KDL property name.
        /// </summary>
        /// <param name="writer">The <see cref="KdlWriter"/> to write to.</param>
        /// <param name="value">The value to convert. Note that the value of <seealso cref="HandleNull"/> determines if the converter handles <see langword="null" /> values.</param>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> being used.</param>
        /// <remarks>Method should be overridden in custom converters of types used in serialized dictionary keys.</remarks>
        public virtual void WriteAsPropertyName(KdlWriter writer, [DisallowNull] T value, KdlSerializerOptions options)
        {
            // .NET 5 backward compatibility: hardcode the default converter for primitive key serialization.
            KdlConverter<T>? fallbackConverter = GetFallbackConverterForPropertyNameSerialization(options);
            if (fallbackConverter is null)
            {
                ThrowHelper.ThrowNotSupportedException_DictionaryKeyTypeNotSupported(Type, this);
            }

            fallbackConverter.WriteAsPropertyNameCore(writer, value, options, isWritingExtensionDataProperty: false);
        }

        internal virtual void WriteAsPropertyNameCore(KdlWriter writer, [DisallowNull] T value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

            if (isWritingExtensionDataProperty)
            {
                // Extension data is meant as mechanism to gather unused KDL properties;
                // do not apply any custom key conversions and hardcode the default behavior.
                Debug.Assert(!IsInternalConverter && Type == typeof(string));
                writer.WritePropertyName((string)(object)value!);
                return;
            }

            int originalDepth = writer.CurrentDepth;
            WriteAsPropertyName(writer, value, options);
            if (originalDepth != writer.CurrentDepth || writer.TokenType != KdlTokenType.PropertyName)
            {
                ThrowHelper.ThrowKdlException_SerializationConverterWrite(this);
            }
        }

        // .NET 5 backward compatibility: hardcode the default converter for primitive key serialization.
        private KdlConverter<T>? GetFallbackConverterForPropertyNameSerialization(KdlSerializerOptions options)
        {
            KdlConverter<T>? result = null;

            // For consistency do not return any default converters for options instances linked to a
            // KdlSerializerContext, even if the default converters might have been rooted.
            if (!IsInternalConverter && options.TypeInfoResolver is not KdlSerializerContext)
            {
                result = _fallbackConverterForPropertyNameSerialization;

                if (result is null && DefaultKdlTypeInfoResolver.TryGetDefaultSimpleConverter(Type, out KdlConverter? defaultConverter))
                {
                    Debug.Assert(defaultConverter != this);
                    _fallbackConverterForPropertyNameSerialization = result = (KdlConverter<T>)defaultConverter;
                }
            }

            return result;
        }

        private KdlConverter<T>? _fallbackConverterForPropertyNameSerialization;

        internal virtual T ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
            => throw new InvalidOperationException();

        internal virtual void WriteNumberWithCustomHandling(KdlWriter writer, T? value, KdlNumberHandling handling)
            => throw new InvalidOperationException();
    }
}
