using System.Diagnostics;
using System.Globalization;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class DateOnlyConverter : KdlPrimitiveConverter<DateOnly>
    {
        public const int FormatLength = 10; // YYYY-MM-DD
        public const int MaxEscapedFormatLength =
            FormatLength * KdlConstants.MaxExpansionFactorWhileEscaping;

        public override DateOnly Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            if (reader.TokenType != KdlTokenType.String)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedString(reader.TokenType);
            }

            return ReadCore(ref reader);
        }

        internal override DateOnly ReadAsPropertyNameCore(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        private static DateOnly ReadCore(ref KdlReader reader)
        {
            if (
                !KdlHelpers.IsInRangeInclusive(
                    reader.ValueLength,
                    FormatLength,
                    MaxEscapedFormatLength
                )
            )
            {
                ThrowHelper.ThrowFormatException(DataType.DateOnly);
            }

            scoped ReadOnlySpan<byte> source;
            if (!reader.HasValueSequence && !reader.ValueIsEscaped)
            {
                source = reader.ValueSpan;
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[MaxEscapedFormatLength];
                int bytesWritten = reader.CopyString(stackSpan);
                source = stackSpan[..bytesWritten];
            }

            if (!KdlHelpers.TryParseAsIso(source, out DateOnly value))
            {
                ThrowHelper.ThrowFormatException(DataType.DateOnly);
            }

            return value;
        }

        public override void Write(KdlWriter writer, DateOnly value, KdlSerializerOptions options)
        {
            Span<byte> buffer = stackalloc byte[FormatLength];
            bool formattedSuccessfully = value.TryFormat(
                buffer,
                out int charsWritten,
                "O",
                CultureInfo.InvariantCulture
            );
            Debug.Assert(formattedSuccessfully && charsWritten == FormatLength);
            writer.WriteStringValue(buffer);
        }

        internal override void WriteAsPropertyNameCore(
            KdlWriter writer,
            DateOnly value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty
        )
        {
            Span<byte> buffer = stackalloc byte[FormatLength];
            bool formattedSuccessfully = value.TryFormat(
                buffer,
                out int charsWritten,
                "O",
                CultureInfo.InvariantCulture
            );
            Debug.Assert(formattedSuccessfully && charsWritten == FormatLength);
            writer.WritePropertyName(buffer);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new() { Type = KdlSchemaType.String, Format = "date" };
    }
}
