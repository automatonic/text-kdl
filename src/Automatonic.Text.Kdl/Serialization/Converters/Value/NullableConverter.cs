using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class NullableConverter<T> : KdlConverter<T?>
        where T : struct // Do not rename FQN (legacy schema generation)
    {
        internal override Type? ElementType => typeof(T);
        internal override KdlConverter? NullableElementConverter => _elementConverter;
        public override bool HandleNull => true;
        internal override bool CanPopulate => _elementConverter.CanPopulate;
        internal override bool ConstructorIsParameterized =>
            _elementConverter.ConstructorIsParameterized;

        // It is possible to cache the underlying converter since this is an internal converter and
        // an instance is created only once for each KdlSerializerOptions instance.
        private readonly KdlConverter<T> _elementConverter; // Do not rename (legacy schema generation)

        public NullableConverter(KdlConverter<T> elementConverter)
        {
            _elementConverter = elementConverter;
            IsInternalConverter = elementConverter.IsInternalConverter;
            IsInternalConverterForNumberType = elementConverter.IsInternalConverterForNumberType;
            ConverterStrategy = elementConverter.ConverterStrategy;
        }

        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out T? value
        )
        {
            if (!state.IsContinuation && reader.TokenType == KdlTokenType.Null)
            {
                value = null;
                return true;
            }

            KdlTypeInfo previousTypeInfo = state.Current.KdlTypeInfo;
            state.Current.KdlTypeInfo = state.Current.KdlTypeInfo.ElementTypeInfo!;
            if (
                _elementConverter.OnTryRead(
                    ref reader,
                    typeof(T),
                    options,
                    ref state,
                    out T element
                )
            )
            {
                value = element;
                state.Current.KdlTypeInfo = previousTypeInfo;
                return true;
            }

            state.Current.KdlTypeInfo = previousTypeInfo;
            value = null;
            return false;
        }

        internal override bool OnTryWrite(
            KdlWriter writer,
            T? value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return true;
            }

            state.Current.KdlPropertyInfo = state
                .Current
                .KdlTypeInfo
                .ElementTypeInfo!
                .PropertyInfoForTypeInfo;
            return _elementConverter.TryWrite(writer, value.Value, options, ref state);
        }

        public override T? Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            if (reader.TokenType == KdlTokenType.Null)
            {
                return null;
            }

            T value = _elementConverter.Read(ref reader, typeof(T), options);
            return value;
        }

        public override void Write(KdlWriter writer, T? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _elementConverter.Write(writer, value.Value, options);
            }
        }

        internal override T? ReadNumberWithCustomHandling(
            ref KdlReader reader,
            KdlNumberHandling numberHandling,
            KdlSerializerOptions options
        )
        {
            if (reader.TokenType == KdlTokenType.Null)
            {
                return null;
            }

            T value = _elementConverter.ReadNumberWithCustomHandling(
                ref reader,
                numberHandling,
                options
            );
            return value;
        }

        internal override void WriteNumberWithCustomHandling(
            KdlWriter writer,
            T? value,
            KdlNumberHandling handling
        )
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _elementConverter.WriteNumberWithCustomHandling(writer, value.Value, handling);
            }
        }
    }
}
