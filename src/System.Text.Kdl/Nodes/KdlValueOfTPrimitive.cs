using System.Diagnostics;
using System.Text.Kdl.Serialization;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    /// A KdlValue encapsulating a primitive value using a built-in converter for the type.
    /// </summary>
    internal sealed class KdlValuePrimitive<TValue> : KdlValue<TValue>
    {
        private readonly KdlConverter<TValue> _converter;
        private readonly KdlValueKind _valueKind;

        public KdlValuePrimitive(TValue value, KdlConverter<TValue> converter, KdlNodeOptions? options) : base(value, options)
        {
            Debug.Assert(TypeIsSupportedPrimitive, $"The type {typeof(TValue)} is not a supported primitive.");
            Debug.Assert(converter is { IsInternalConverter: true, ConverterStrategy: ConverterStrategy.Value });

            _converter = converter;
            _valueKind = DetermineValueKind(value);
        }

        private protected override KdlValueKind GetValueKindCore() => _valueKind;
        internal override KdlNode DeepCloneCore() => new KdlValuePrimitive<TValue>(Value, _converter, Options);

        internal override bool DeepEqualsCore(KdlNode otherNode)
        {
            if (otherNode is KdlValue otherValue && otherValue.TryGetValue(out TValue? v))
            {
                // Because TValue is equatable and otherNode returns a matching
                // type we can short circuit the comparison in this case.
                return EqualityComparer<TValue>.Default.Equals(Value, v);
            }

            return base.DeepEqualsCore(otherNode);
        }

        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            KdlConverter<TValue> converter = _converter;
            options ??= s_defaultOptions;

            if (converter.IsInternalConverterForNumberType)
            {
                converter.WriteNumberWithCustomHandling(writer, Value, options.NumberHandling);
            }
            else
            {
                converter.Write(writer, Value, options);
            }
        }
    }
}
