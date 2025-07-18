using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Provides a mechanism to invoke "fast-path" serialization logic via
    /// <see cref="KdlTypeInfo{T}.SerializeHandler"/>. This type holds an optional
    /// reference to an actual <see cref="KdlConverter{T}"/> for the type
    /// <typeparamref name="T"/>, to provide a fallback when the fast path cannot be used.
    /// </summary>
    /// <typeparam name="T">The type to converter</typeparam>
    internal sealed class KdlMetadataServicesConverter<T> : KdlResumableConverter<T>
    {
        // A backing converter for when fast-path logic cannot be used.
        internal KdlConverter<T> Converter { get; }

        internal override Type? KeyType => Converter.KeyType;
        internal override Type? ElementType => Converter.ElementType;
        internal override KdlConverter? NullableElementConverter =>
            Converter.NullableElementConverter;
        public override bool HandleNull { get; }

        internal override bool ConstructorIsParameterized => Converter.ConstructorIsParameterized;
        internal override bool SupportsCreateObjectDelegate =>
            Converter.SupportsCreateObjectDelegate;
        internal override bool CanHaveMetadata => Converter.CanHaveMetadata;

        internal override bool CanPopulate => Converter.CanPopulate;

        public KdlMetadataServicesConverter(KdlConverter<T> converter)
        {
            Converter = converter;
            ConverterStrategy = converter.ConverterStrategy;
            IsInternalConverter = converter.IsInternalConverter;
            IsInternalConverterForNumberType = converter.IsInternalConverterForNumberType;
            CanBePolymorphic = converter.CanBePolymorphic;

            // Ensure HandleNull values reflect the exact configuration of the source converter
            HandleNullOnRead = converter.HandleNullOnRead;
            HandleNullOnWrite = converter.HandleNullOnWrite;
            HandleNull = converter.HandleNullOnWrite;
        }

        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out T? value
        ) => Converter.OnTryRead(ref reader, typeToConvert, options, ref state, out value);

        internal override bool OnTryWrite(
            KdlWriter writer,
            T value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            KdlTypeInfo kdlTypeInfo = state.Current.KdlTypeInfo;
            Debug.Assert(
                kdlTypeInfo is KdlTypeInfo<T> typeInfo && typeInfo.SerializeHandler != null
            );

            if (
                !state.SupportContinuation
                && kdlTypeInfo.CanUseSerializeHandler
                && !KdlHelpers.RequiresSpecialNumberHandlingOnWrite(state.Current.NumberHandling)
                && !state.CurrentContainsMetadata
            ) // Do not use the fast path if state needs to write metadata.
            {
                ((KdlTypeInfo<T>)kdlTypeInfo).SerializeHandler!(writer, value);
                return true;
            }

            return Converter.OnTryWrite(writer, value, options, ref state);
        }

        internal override void ConfigureKdlTypeInfo(
            KdlTypeInfo kdlTypeInfo,
            KdlSerializerOptions options
        ) => Converter.ConfigureKdlTypeInfo(kdlTypeInfo, options);

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            Converter.GetSchema(numberHandling);
    }
}
