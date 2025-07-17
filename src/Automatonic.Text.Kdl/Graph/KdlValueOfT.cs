using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.RandomAccess;

namespace Automatonic.Text.Kdl.Graph
{
    [DebuggerDisplay("{ToKdlString(),nq}")]
    [DebuggerTypeProxy(typeof(KdlValue<>.DebugView))]
    internal abstract class KdlValue<TValue> : KdlValue
    {
        internal readonly TValue Value; // keep as a field for direct access to avoid copies

        protected KdlValue(TValue value, KdlElementOptions? options)
            : base(options)
        {
            Debug.Assert(value != null);
            Debug.Assert(
                value
                    is not KdlReadOnlyElement
                        or KdlReadOnlyElement { ValueKind: not KdlValueKind.Null }
            );
            Debug.Assert(value is not KdlElement);
            Value = value;
        }

        public override T GetValue<T>()
        {
            // If no conversion is needed, just return the raw value.
            if (Value is T returnValue)
            {
                return returnValue;
            }

            // Currently we do not support other conversions.
            // Generics (and also boxing) do not support standard cast operators say from 'long' to 'int',
            //  so attempting to cast here would throw InvalidCastException.
            ThrowHelper.ThrowInvalidOperationException_NodeUnableToConvert(
                typeof(TValue),
                typeof(T)
            );
            return default!;
        }

        public override bool TryGetValue<T>([NotNullWhen(true)] out T value)
        {
            // If no conversion is needed, just return the raw value.
            if (Value is T returnValue)
            {
                value = returnValue;
                return true;
            }

            // Currently we do not support other conversions.
            // Generics (and also boxing) do not support standard cast operators say from 'long' to 'int',
            //  so attempting to cast here would throw InvalidCastException.
            value = default!;
            return false;
        }

        /// <summary>
        /// Whether <typeparamref name="TValue"/> is a built-in type that admits primitive KdlValue representation.
        /// </summary>
        internal static bool TypeIsSupportedPrimitive => s_valueKind.HasValue;
        private static readonly KdlValueKind? s_valueKind = DetermineValueKindForType(
            typeof(TValue)
        );

        /// <summary>
        /// Determines the KdlValueKind for the value of a built-in type.
        /// </summary>
        private protected static KdlValueKind DetermineValueKind(TValue value)
        {
            Debug.Assert(
                s_valueKind is not null,
                "Should only be invoked for types that are supported primitives."
            );

            if (value is bool boolean)
            {
                // Boolean requires special handling since kind varies by value.
                return boolean ? KdlValueKind.True : KdlValueKind.False;
            }

            return s_valueKind.Value;
        }

        /// <summary>
        /// Precomputes the KdlValueKind for a given built-in type where possible.
        /// </summary>
        private static KdlValueKind? DetermineValueKindForType(Type type)
        {
            if (type.IsEnum)
            {
                return null; // Can vary depending on converter configuration and value.
            }

            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            {
                // Because KdlVertex excludes null values, we can identify with the value kind of the underlying type.
                return DetermineValueKindForType(underlyingType);
            }

            if (
                type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(DateOnly)
                || type == typeof(TimeOnly)
                || type == typeof(Guid)
                || type == typeof(Uri)
                || type == typeof(Version)
            )
            {
                return KdlValueKind.String;
            }

            if (type == typeof(Half) || type == typeof(UInt128) || type == typeof(Int128))
            {
                return KdlValueKind.Number;
            }
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => KdlValueKind.Undefined, // Can vary dependending on value.
                TypeCode.SByte => KdlValueKind.Number,
                TypeCode.Byte => KdlValueKind.Number,
                TypeCode.Int16 => KdlValueKind.Number,
                TypeCode.UInt16 => KdlValueKind.Number,
                TypeCode.Int32 => KdlValueKind.Number,
                TypeCode.UInt32 => KdlValueKind.Number,
                TypeCode.Int64 => KdlValueKind.Number,
                TypeCode.UInt64 => KdlValueKind.Number,
                TypeCode.Single => KdlValueKind.Number,
                TypeCode.Double => KdlValueKind.Number,
                TypeCode.Decimal => KdlValueKind.Number,
                TypeCode.String => KdlValueKind.String,
                TypeCode.Char => KdlValueKind.String,
                _ => null,
            };
        }

        [ExcludeFromCodeCoverage] // Justification = "Design-time"
        [DebuggerDisplay("{Kdl,nq}")]
        private sealed class DebugView(KdlValue<TValue> node)
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public KdlValue<TValue> _node = node;

            public string Kdl => _node.ToKdlString();
            public string Path => _node.GetPath();
            public TValue? Value => _node.Value;
        }
    }
}
