using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    // Converter for F# optional values: https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-option-1.html
    // Serializes `Some(value)` using the format of `value` and `None` values as `null`.
    internal sealed class FSharpOptionConverter<TOption, TElement> : KdlConverter<TOption>
        where TOption : class
    {
        internal override Type? ElementType => typeof(TElement);
        internal override KdlConverter? NullableElementConverter => _elementConverter;
        // 'None' is encoded using 'null' at runtime and serialized as 'null' in KDL.
        public override bool HandleNull => true;

        private readonly KdlConverter<TElement> _elementConverter;
        private readonly Func<TOption, TElement> _optionValueGetter;
        private readonly Func<TElement?, TOption> _optionConstructor;

        [RequiresUnreferencedCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
        [RequiresDynamicCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
        public FSharpOptionConverter(KdlConverter<TElement> elementConverter)
        {
            _elementConverter = elementConverter;
            _optionValueGetter = FSharpCoreReflectionProxy.Instance.CreateFSharpOptionValueGetter<TOption, TElement>();
            _optionConstructor = FSharpCoreReflectionProxy.Instance.CreateFSharpOptionSomeConstructor<TOption, TElement>();
            ConverterStrategy = elementConverter.ConverterStrategy;
        }

        internal override bool OnTryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out TOption? value)
        {
            // `null` values deserialize as `None`
            if (!state.IsContinuation && reader.TokenType == KdlTokenType.Null)
            {
                value = null;
                return true;
            }

            state.Current.KdlPropertyInfo = state.Current.KdlTypeInfo.ElementTypeInfo!.PropertyInfoForTypeInfo;
            if (_elementConverter.TryRead(ref reader, typeof(TElement), options, ref state, out TElement? element, out _))
            {
                value = _optionConstructor(element);
                return true;
            }

            value = null;
            return false;
        }

        internal override bool OnTryWrite(KdlWriter writer, TOption value, KdlSerializerOptions options, ref WriteStack state)
        {
            if (value is null)
            {
                // Write `None` values as null
                writer.WriteNullValue();
                return true;
            }

            TElement element = _optionValueGetter(value);
            state.Current.KdlPropertyInfo = state.Current.KdlTypeInfo.ElementTypeInfo!.PropertyInfoForTypeInfo;
            return _elementConverter.TryWrite(writer, element, options, ref state);
        }

        // Since this is a hybrid converter (ConverterStrategy depends on the element converter),
        // we need to override the value converter Write and Read methods too.

        public override void Write(KdlWriter writer, TOption value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                TElement element = _optionValueGetter(value);
                _elementConverter.Write(writer, element, options);
            }
        }

        public override TOption? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.Null)
            {
                return null;
            }

            TElement? element = _elementConverter.Read(ref reader, typeToConvert, options);
            return _optionConstructor(element);
        }
    }
}
