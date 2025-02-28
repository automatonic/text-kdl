using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class UInt128Converter : KdlPrimitiveConverter<UInt128>
    {
        private const int MaxFormatLength = 39;

        public UInt128Converter() => IsInternalConverterForNumberType = true;

        public override UInt128 Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType != KdlTokenType.Number)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedNumber(reader.TokenType);
            }

            return ReadCore(ref reader);
        }

        public override void Write(KdlWriter writer, UInt128 value, KdlSerializerOptions options)
        {
            WriteCore(writer, value);
        }

        private static UInt128 ReadCore(ref KdlReader reader)
        {
            int bufferLength = reader.ValueLength;

            byte[]? rentedBuffer = null;
            Span<byte> buffer = bufferLength <= KdlConstants.StackallocByteThreshold
                ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                : (rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferLength));

            int written = reader.CopyValue(buffer);
            if (!UInt128.TryParse(buffer[..written], CultureInfo.InvariantCulture, out UInt128 result))
            {
                ThrowHelper.ThrowFormatException(NumericType.UInt128);
            }

            if (rentedBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }

            return result;
        }

        private static void WriteCore(KdlWriter writer, UInt128 value)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            Format(buffer, value, out int written);
            writer.WriteRawValue(buffer[..written]);
        }

        internal override UInt128 ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, UInt128 value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            Format(buffer, value, out int written);
            writer.WritePropertyName(buffer);
        }

        internal override UInt128 ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return ReadCore(ref reader);
            }

            return Read(ref reader, Type, options);
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, UInt128 value, KdlNumberHandling handling)
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
            else
            {
                WriteCore(writer, value);
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling) =>
            GetSchemaForNumericType(KdlSchemaType.Integer, numberHandling);

        private static void Format(
            Span<byte> destination,
            UInt128 value, out int written)
        {
            bool formattedSuccessfully = value.TryFormat(destination, out written, provider: CultureInfo.InvariantCulture);
            Debug.Assert(formattedSuccessfully);
        }
    }
}
