// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class Int128Converter : KdlPrimitiveConverter<Int128>
    {
        private const int MaxFormatLength = 40;

        public Int128Converter()
        {
            IsInternalConverterForNumberType = true;
        }

        public override Int128 Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType != KdlTokenType.Number)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedNumber(reader.TokenType);
            }

            return ReadCore(ref reader);
        }

        public override void Write(KdlWriter writer, Int128 value, KdlSerializerOptions options)
        {
            WriteCore(writer, value);
        }

        private static Int128 ReadCore(ref KdlReader reader)
        {
            int bufferLength = reader.ValueLength;

            byte[]? rentedBuffer = null;
            Span<byte> buffer = bufferLength <= KdlConstants.StackallocByteThreshold
                ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                : (rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferLength));

            int written = reader.CopyValue(buffer);
            if (!Int128.TryParse(buffer.Slice(0, written), CultureInfo.InvariantCulture, out Int128 result))
            {
                ThrowHelper.ThrowFormatException(NumericType.Int128);
            }

            if (rentedBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }

            return result;
        }

        private static void WriteCore(KdlWriter writer, Int128 value)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            Format(buffer, value, out int written);
            writer.WriteRawValue(buffer.Slice(0, written));
        }

        internal override Int128 ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, Int128 value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            Span<byte> buffer = stackalloc byte[MaxFormatLength];
            Format(buffer, value, out int written);
            writer.WritePropertyName(buffer);
        }

        internal override Int128 ReadNumberWithCustomHandling(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            if (reader.TokenType == KdlTokenType.String &&
                (KdlNumberHandling.AllowReadingFromString & handling) != 0)
            {
                return ReadCore(ref reader);
            }

            return Read(ref reader, Type, options);
        }

        internal override void WriteNumberWithCustomHandling(KdlWriter writer, Int128 value, KdlNumberHandling handling)
        {
            if ((KdlNumberHandling.WriteAsString & handling) != 0)
            {
                const byte Quote = KdlConstants.Quote;
                Span<byte> buffer = stackalloc byte[MaxFormatLength + 2];
                buffer[0] = Quote;
                Format(buffer.Slice(1), value, out int written);

                int length = written + 2;
                buffer[length - 1] = Quote;
                writer.WriteRawValue(buffer.Slice(0, length));
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
            Int128 value, out int written)
        {
            bool formattedSuccessfully = value.TryFormat(destination, out written, provider: CultureInfo.InvariantCulture);
            Debug.Assert(formattedSuccessfully);
        }
    }
}
