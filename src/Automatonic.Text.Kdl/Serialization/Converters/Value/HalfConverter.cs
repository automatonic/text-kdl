using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class HalfConverter : KdlPrimitiveConverter<Half>
    {
        private const int MaxFormatLength = 20;

        public HalfConverter() => IsInternalConverterForNumberType = true;

        public override Half Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType != KdlTokenType.Number)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedNumber(reader.TokenType);
            }

            return ReadCore(ref reader);
        }

        public override void Write(KdlWriter writer, Half value, KdlSerializerOptions options)
        {
            WriteCore(writer, value);
        }

        private static Half ReadCore(ref KdlReader reader)
        {
            Half result;

            byte[]? rentedByteBuffer = null;
            int bufferLength = reader.ValueLength;

            Span<byte> byteBuffer = bufferLength <= KdlConstants.StackallocByteThreshold
                ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                : (rentedByteBuffer = ArrayPool<byte>.Shared.Rent(bufferLength));

            int written = reader.CopyValue(byteBuffer);
            byteBuffer = byteBuffer[..written];

            bool success = TryParse(byteBuffer, out result);
            if (rentedByteBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(rentedByteBuffer);
            }

            if (!success)
            {
                ThrowHelper.ThrowFormatException(NumericType.Half);
            }

            Debug.Assert(!Half.IsNaN(result) && !Half.IsInfinity(result));
            return result;
        }

        private static void WriteCore(KdlWriter writer, Half value)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            Format(buffer, value, out int written);
            writer.WriteRawValue(buffer[..written]);
        }

        internal override Half ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, Half value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            Format(buffer, value, out int written);
            writer.WritePropertyName(buffer[..written]);
        }

        internal override Half ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String)
            {
                if ((KdlNumberHandling.AllowReadingFromString & handling) != 0)
                {
                    if (TryGetFloatingPointConstant(ref reader, out Half value))
                    {
                        return value;
                    }

                    return ReadCore(ref reader);
                }
                else if ((KdlNumberHandling.AllowNamedFloatingPointLiterals & handling) != 0)
                {
                    if (!TryGetFloatingPointConstant(ref reader, out Half value))
                    {
                        ThrowHelper.ThrowFormatException(NumericType.Half);
                    }

                    return value;
                }
            }

            return Read(ref reader, Type, options);
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, Half value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                const byte Quote = KdlConstants.Quote;
                Span<byte> buffer = stackalloc byte[MaxFormatLength + 2];
                buffer[0] = Quote;
                Format(buffer[1..], value, out int written);

                int length = written + 2;
                buffer[length - 1] = Quote;
                writer.WriteRawValue(buffer[..length]);
            }
            else if ((KdlNumberHandling.AllowNamedFloatingPointLiterals & handling) != 0)
            {
                WriteFloatingPointConstant(writer, value);
            }
            else
            {
                WriteCore(writer, value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Number, numberHandling, isIeeeFloatingPoint: true);

        private static bool TryGetFloatingPointConstant(ref KdlReader reader, out Half value)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            int written = reader.CopyValue(buffer);

            return KdlReaderHelper.TryGetFloatingPointConstant(buffer[..written], out value);
        }

        private static void WriteFloatingPointConstant(KdlWriter writer, Half value)
        {
            if (Half.IsNaN(value))
            {
                writer.WriteNumberValueAsStringUnescaped(KdlConstants.NaNValue);
            }
            else if (Half.IsPositiveInfinity(value))
            {
                writer.WriteNumberValueAsStringUnescaped(KdlConstants.PositiveInfinityValue);
            }
            else if (Half.IsNegativeInfinity(value))
            {
                writer.WriteNumberValueAsStringUnescaped(KdlConstants.NegativeInfinityValue);
            }
            else
            {
                WriteCore(writer, value);
            }
        }

        private static bool TryParse(ReadOnlySpan<byte> buffer, out Half result)
        {
            bool success = Half.TryParse(buffer, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

            // Half.TryParse is more lax with floating-point literals than other S.T.Kdl floating-point types
            // e.g: it parses "naN" successfully. Only succeed with the exact match.
            return success &&
                (!Half.IsNaN(result) || buffer.SequenceEqual(KdlConstants.NaNValue)) &&
                (!Half.IsPositiveInfinity(result) || buffer.SequenceEqual(KdlConstants.PositiveInfinityValue)) &&
                (!Half.IsNegativeInfinity(result) || buffer.SequenceEqual(KdlConstants.NegativeInfinityValue));
        }

        private static void Format(
            Span<byte> destination,
            Half value, out int written)
        {
            bool formattedSuccessfully = value.TryFormat(destination, out written, provider: CultureInfo.InvariantCulture);
            Debug.Assert(formattedSuccessfully);
        }
    }
}
