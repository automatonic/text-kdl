using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Inherited by built-in converters serializing types as KDL primitives that support property name serialization.
    /// </summary>
    internal abstract class KdlPrimitiveConverter<T> : KdlConverter<T>
    {
        public sealed override void WriteAsPropertyName(KdlWriter writer, [DisallowNull] T value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

            WriteAsPropertyNameCore(writer, value, options, isWritingExtensionDataProperty: false);
        }

        public sealed override T ReadAsPropertyName(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType != KdlTokenType.PropertyName)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedPropertyName(reader.TokenType);
            }

            return ReadAsPropertyNameCore(ref reader, typeToConvert, options);
        }

        private protected static KdlSchema GetSchemaForNumericType(KdlSchemaType schemaType, KdlNumberHandling numberHandling, bool isIeeeFloatingPoint = false)
        {
            Debug.Assert(schemaType is KdlSchemaType.Integer or KdlSchemaType.Number);
            Debug.Assert(!isIeeeFloatingPoint || schemaType is KdlSchemaType.Number);
#if NET
            Debug.Assert(isIeeeFloatingPoint == (typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(Half)));
#endif
            string? pattern = null;

            if ((numberHandling & (KdlNumberHandling.AllowReadingFromString | KdlNumberHandling.WriteAsString)) != 0)
            {
                pattern = schemaType is KdlSchemaType.Integer
                    ? @"^-?(?:0|[1-9]\d*)$"
                    : isIeeeFloatingPoint
                        ? @"^-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?$"
                        : @"^-?(?:0|[1-9]\d*)(?:\.\d+)?$";

                schemaType |= KdlSchemaType.String;
            }

            if (isIeeeFloatingPoint && (numberHandling & KdlNumberHandling.AllowNamedFloatingPointLiterals) != 0)
            {
                return new KdlSchema
                {
                    AnyOf =
                    [
                        new KdlSchema { Type = schemaType, Pattern = pattern },
                        //TECHDEBT
                        //new KdlSchema { Enum = [(KdlVertex)"NaN", (KdlVertex)"Infinity", (KdlVertex)"-Infinity"] },
                    ]
                };
            }

            return new KdlSchema { Type = schemaType, Pattern = pattern };
        }
    }
}
