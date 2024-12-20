// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Kdl.Schema;
using System.Text.Kdl.Serialization.Converters;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Converts an object or value to or from KDL.
    /// </summary>
    public abstract partial class KdlConverter
    {
        internal KdlConverter()
        {
            IsInternalConverter = GetType().Assembly == typeof(KdlConverter).Assembly;
            ConverterStrategy = GetDefaultConverterStrategy();
        }

        /// <summary>
        /// Gets the type being converted by the current converter instance.
        /// </summary>
        /// <remarks>
        /// For instances of type <see cref="KdlConverter{T}"/> returns typeof(T),
        /// and for instances of type <see cref="KdlConverterFactory"/> returns <see langword="null" />.
        /// </remarks>
        public abstract Type? Type { get; }

        /// <summary>
        /// Determines whether the type can be converted.
        /// </summary>
        /// <param name="typeToConvert">The type is checked as to whether it can be converted.</param>
        /// <returns>True if the type can be converted, false otherwise.</returns>
        public abstract bool CanConvert(Type typeToConvert);

        internal ConverterStrategy ConverterStrategy
        {
            get => _converterStrategy;
            init
            {
                CanUseDirectReadOrWrite = value == ConverterStrategy.Value && IsInternalConverter;
                RequiresReadAhead = value == ConverterStrategy.Value;
                _converterStrategy = value;
            }
        }

        private ConverterStrategy _converterStrategy;

        /// <summary>
        /// Invoked by the base contructor to populate the initial value of the <see cref="ConverterStrategy"/> property.
        /// Used for declaring the default strategy for specific converter hierarchies without explicitly setting in a constructor.
        /// </summary>
        private protected abstract ConverterStrategy GetDefaultConverterStrategy();

        /// <summary>
        /// Indicates that the converter can consume the <see cref="KdlTypeInfo.CreateObject"/> delegate.
        /// Needed because certain collection converters cannot support arbitrary delegates.
        /// TODO remove once https://github.com/dotnet/runtime/pull/73395/ and
        /// https://github.com/dotnet/runtime/issues/71944 have been addressed.
        /// </summary>
        internal virtual bool SupportsCreateObjectDelegate => false;

        /// <summary>
        /// Indicates that the converter is compatible with <see cref="KdlObjectCreationHandling.Populate"/>.
        /// </summary>
        internal virtual bool CanPopulate => false;

        /// <summary>
        /// Can direct Read or Write methods be called (for performance).
        /// </summary>
        internal bool CanUseDirectReadOrWrite { get; set; }

        /// <summary>
        /// The converter supports writing and reading metadata.
        /// </summary>
        internal virtual bool CanHaveMetadata => false;

        /// <summary>
        /// The converter supports polymorphic writes; only reserved for System.Object types.
        /// </summary>
        internal bool CanBePolymorphic { get; init; }

        /// <summary>
        /// The serializer must read ahead all contents of the next KDL value
        /// before calling into the converter for deserialization.
        /// </summary>
        internal bool RequiresReadAhead { get; private protected set; }

        /// <summary>
        /// Whether the converter is a special root-level value streaming converter.
        /// </summary>
        internal bool IsRootLevelMultiContentStreamingConverter { get; init; }

        /// <summary>
        /// Used to support KdlObject as an extension property in a loosely-typed, trimmable manner.
        /// </summary>
        internal virtual void ReadElementAndSetProperty(
            object obj,
            string propertyName,
            ref KdlReader reader,
            KdlSerializerOptions options,
            scoped ref ReadStack state)
        {
            Debug.Fail("Should not be reachable.");

            throw new InvalidOperationException();
        }

        internal virtual KdlTypeInfo CreateKdlTypeInfo(KdlSerializerOptions options)
        {
            Debug.Fail("Should not be reachable.");

            throw new InvalidOperationException();
        }

        internal KdlConverter<TTarget> CreateCastingConverter<TTarget>()
        {
            Debug.Assert(this is not KdlConverterFactory);

            if (this is KdlConverter<TTarget> conv)
            {
                return conv;
            }
            else
            {
                KdlSerializerOptions.CheckConverterNullabilityIsSameAsPropertyType(this, typeof(TTarget));

                // Avoid layering casting converters by consulting any source converters directly.
                return
                    SourceConverterForCastingConverter?.CreateCastingConverter<TTarget>()
                    ?? new CastingConverter<TTarget>(this);
            }
        }

        /// <summary>
        /// Tracks whether the KdlConverter&lt;T&gt;.HandleNull property has been overridden by a derived converter.
        /// </summary>
        internal bool UsesDefaultHandleNull { get; private protected set; }

        /// <summary>
        /// Does the converter want to be called when reading null tokens.
        /// When KdlConverter&lt;T&gt;.HandleNull isn't overridden this can still be true for non-nullable structs.
        /// </summary>
        internal bool HandleNullOnRead { get; private protected init; }

        /// <summary>
        /// Does the converter want to be called for null values.
        /// Should always match the precise value of the KdlConverter&lt;T&gt;.HandleNull virtual property.
        /// </summary>
        internal bool HandleNullOnWrite { get; private protected init; }

        /// <summary>
        /// Set if this converter is itself a casting converter.
        /// </summary>
        internal virtual KdlConverter? SourceConverterForCastingConverter => null;

        internal virtual Type? ElementType => null;

        internal virtual Type? KeyType => null;

        internal virtual KdlConverter? NullableElementConverter => null;

        /// <summary>
        /// Cached value of TypeToConvert.IsValueType, which is an expensive call.
        /// </summary>
        internal bool IsValueType { get; init; }

        /// <summary>
        /// Whether the converter is built-in.
        /// </summary>
        internal bool IsInternalConverter { get; init; }

        /// <summary>
        /// Whether the converter is built-in and handles a number type.
        /// </summary>
        internal bool IsInternalConverterForNumberType { get; init; }

        /// <summary>
        /// Whether the converter handles collection deserialization by converting from
        /// an intermediate buffer such as immutable collections, arrays or memory types.
        /// Used in conjunction with <see cref="KdlCollectionConverter{TCollection, TElement}.ConvertCollection(ref ReadStack, KdlSerializerOptions)"/>.
        /// </summary>
        internal virtual bool IsConvertibleCollection => false;

        internal static bool ShouldFlush(ref WriteStack state, KdlWriter writer)
        {
            Debug.Assert(state.FlushThreshold == 0 || (state.PipeWriter is { CanGetUnflushedBytes: true }),
                "ShouldFlush should only be called by resumable serializers, all of which use the PipeWriter abstraction with CanGetUnflushedBytes == true.");
            // If surpassed flush threshold then return true which will flush stream.
            if (state.PipeWriter is { } pipeWriter)
            {
                return state.FlushThreshold > 0 && pipeWriter.UnflushedBytes > state.FlushThreshold - writer.BytesPending;
            }

            return false;
        }

        internal abstract object? ReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options);
        internal abstract bool OnTryReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out object? value);
        internal abstract bool TryReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out object? value);
        internal abstract object? ReadAsPropertyNameAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options);
        internal abstract object? ReadAsPropertyNameCoreAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options);
        internal abstract object? ReadNumberWithCustomHandlingAsObject(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options);

        internal abstract void WriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options);
        internal abstract bool OnTryWriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options, ref WriteStack state);
        internal abstract bool TryWriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options, ref WriteStack state);
        internal abstract void WriteAsPropertyNameAsObject(KdlWriter writer, object? value, KdlSerializerOptions options);
        internal abstract void WriteAsPropertyNameCoreAsObject(KdlWriter writer, object? value, KdlSerializerOptions options, bool isWritingExtensionDataProperty);
        internal abstract void WriteNumberWithCustomHandlingAsObject(KdlWriter writer, object? value, KdlNumberHandling handling);

        /// <summary>
        /// Gets a schema from the type being converted
        /// </summary>
        internal virtual KdlSchema? GetSchema(KdlNumberHandling numberHandling) => null;

        // Whether a type (ConverterStrategy.Object) is deserialized using a parameterized constructor.
        internal virtual bool ConstructorIsParameterized { get; }

        internal ConstructorInfo? ConstructorInfo { get; set; }

        /// <summary>
        /// Used for hooking custom configuration to a newly created associated KdlTypeInfo instance.
        /// </summary>
        internal virtual void ConfigureKdlTypeInfo(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options) { }

        /// <summary>
        /// Additional reflection-specific configuration required by certain collection converters.
        /// </summary>
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal virtual void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options) { }
    }
}
