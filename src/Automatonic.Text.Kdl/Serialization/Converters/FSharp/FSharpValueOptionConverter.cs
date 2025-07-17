using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    // Converter for F# struct optional values: https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-fsharpvalueoption-1.html
    // Serializes `ValueSome(value)` using the format of `value` and `ValueNone` values as `null`.
    internal sealed class FSharpValueOptionConverter<TValueOption, TElement>
        : KdlConverter<TValueOption>
        where TValueOption : struct, IEquatable<TValueOption>
    {
        internal override Type? ElementType => typeof(TElement);
        internal override KdlConverter? NullableElementConverter => _elementConverter;

        // 'ValueNone' is encoded using 'default' at runtime and serialized as 'null' in KDL.
        public override bool HandleNull => true;

        private readonly KdlConverter<TElement> _elementConverter;
        private readonly FSharpCoreReflectionProxy.StructGetter<
            TValueOption,
            TElement
        > _optionValueGetter;
        private readonly Func<TElement?, TValueOption> _optionConstructor;

        [RequiresUnreferencedCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
        [RequiresDynamicCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
        public FSharpValueOptionConverter(KdlConverter<TElement> elementConverter)
        {
            _elementConverter = elementConverter;
            _optionValueGetter =
                FSharpCoreReflectionProxy.Instance.CreateFSharpValueOptionValueGetter<
                    TValueOption,
                    TElement
                >();
            _optionConstructor =
                FSharpCoreReflectionProxy.Instance.CreateFSharpValueOptionSomeConstructor<
                    TValueOption,
                    TElement
                >();
            ConverterStrategy = elementConverter.ConverterStrategy;
        }

        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out TValueOption value
        )
        {
            // `null` values deserialize as `ValueNone`
            if (!state.IsContinuation && reader.TokenType == KdlTokenType.Null)
            {
                value = default;
                return true;
            }

            state.Current.KdlPropertyInfo = state
                .Current
                .KdlTypeInfo
                .ElementTypeInfo!
                .PropertyInfoForTypeInfo;
            if (
                _elementConverter.TryRead(
                    ref reader,
                    typeof(TElement),
                    options,
                    ref state,
                    out TElement? element,
                    out _
                )
            )
            {
                value = _optionConstructor(element);
                return true;
            }

            value = default;
            return false;
        }

        internal override bool OnTryWrite(
            KdlWriter writer,
            TValueOption value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            if (value.Equals(default))
            {
                // Write `ValueNone` values as null
                writer.WriteNullValue();
                return true;
            }

            TElement element = _optionValueGetter(ref value);

            state.Current.KdlPropertyInfo = state
                .Current
                .KdlTypeInfo
                .ElementTypeInfo!
                .PropertyInfoForTypeInfo;
            return _elementConverter.TryWrite(writer, element, options, ref state);
        }

        // Since this is a hybrid converter (ConverterStrategy depends on the element converter),
        // we need to override the value converter Write and Read methods too.

        public override void Write(
            KdlWriter writer,
            TValueOption value,
            KdlSerializerOptions options
        )
        {
            if (value.Equals(default))
            {
                // Write `ValueNone` values as null
                writer.WriteNullValue();
            }
            else
            {
                TElement element = _optionValueGetter(ref value);
                _elementConverter.Write(writer, element, options);
            }
        }

        public override TValueOption Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            if (reader.TokenType == KdlTokenType.Null)
            {
                return default;
            }

            TElement? element = _elementConverter.Read(ref reader, typeToConvert, options);
            return _optionConstructor(element);
        }
    }
}
