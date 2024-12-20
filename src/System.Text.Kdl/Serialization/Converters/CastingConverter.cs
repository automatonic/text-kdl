// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Reflection;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter wrapper which casts SourceType into TargetType
    /// </summary>
    internal sealed class CastingConverter<T> : KdlConverter<T>
    {
        private readonly KdlConverter _sourceConverter;
        internal override Type? KeyType => _sourceConverter.KeyType;
        internal override Type? ElementType => _sourceConverter.ElementType;
        internal override KdlConverter? NullableElementConverter => _sourceConverter.NullableElementConverter;

        public override bool HandleNull { get; }
        internal override bool SupportsCreateObjectDelegate => _sourceConverter.SupportsCreateObjectDelegate;

        internal CastingConverter(KdlConverter sourceConverter)
        {
            Debug.Assert(typeof(T).IsInSubtypeRelationshipWith(sourceConverter.Type!));
            Debug.Assert(sourceConverter.SourceConverterForCastingConverter is null, "casting converters should not be layered.");

            _sourceConverter = sourceConverter;
            IsInternalConverter = sourceConverter.IsInternalConverter;
            IsInternalConverterForNumberType = sourceConverter.IsInternalConverterForNumberType;
            ConverterStrategy = sourceConverter.ConverterStrategy;
            CanBePolymorphic = sourceConverter.CanBePolymorphic;

            // Ensure HandleNull values reflect the exact configuration of the source converter
            HandleNullOnRead = sourceConverter.HandleNullOnRead;
            HandleNullOnWrite = sourceConverter.HandleNullOnWrite;
            HandleNull = sourceConverter.HandleNullOnWrite;
        }

        internal override KdlConverter? SourceConverterForCastingConverter => _sourceConverter;

        public override T? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
            => KdlSerializer.UnboxOnRead<T>(_sourceConverter.ReadAsObject(ref reader, typeToConvert, options));

        public override void Write(KdlWriter writer, T value, KdlSerializerOptions options)
            => _sourceConverter.WriteAsObject(writer, value, options);

        internal override bool OnTryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out T? value)
        {
            bool result = _sourceConverter.OnTryReadAsObject(ref reader, typeToConvert, options, ref state, out object? sourceValue);
            value = KdlSerializer.UnboxOnRead<T>(sourceValue);
            return result;
        }

        internal override bool OnTryWrite(KdlWriter writer, T value, KdlSerializerOptions options, ref WriteStack state)
            => _sourceConverter.OnTryWriteAsObject(writer, value, options, ref state);

        public override T ReadAsPropertyName(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
            => KdlSerializer.UnboxOnRead<T>(_sourceConverter.ReadAsPropertyNameAsObject(ref reader, typeToConvert, options))!;

        internal override T ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
            => KdlSerializer.UnboxOnRead<T>(_sourceConverter.ReadAsPropertyNameCoreAsObject(ref reader, typeToConvert, options))!;

        public override void WriteAsPropertyName(KdlWriter writer, [DisallowNull] T value, KdlSerializerOptions options)
            => _sourceConverter.WriteAsPropertyNameAsObject(writer, value, options);

        internal override void WriteAsPropertyNameCore(KdlWriter writer, T value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
            => _sourceConverter.WriteAsPropertyNameCoreAsObject(writer, value, options, isWritingExtensionDataProperty);

        internal override T ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
            => KdlSerializer.UnboxOnRead<T>(_sourceConverter.ReadNumberWithCustomHandlingAsObject(ref reader, handling, options))!;

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, T? value, KdlNumberHandling handling)
            => _sourceConverter.WriteNumberWithCustomHandlingAsObject(writer, value, handling);

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling)
            => _sourceConverter.GetSchema(numberHandling);
    }
}
