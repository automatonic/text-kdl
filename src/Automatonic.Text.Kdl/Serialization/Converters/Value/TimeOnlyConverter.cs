using System.Buffers.Text;
using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class TimeOnlyConverter : KdlPrimitiveConverter<TimeOnly>
    {
        private const int MinimumTimeOnlyFormatLength = 3; // h:m
        private const int MaximumTimeOnlyFormatLength = 16; // hh:mm:ss.fffffff
        private const int MaximumEscapedTimeOnlyFormatLength =
            KdlConstants.MaxExpansionFactorWhileEscaping * MaximumTimeOnlyFormatLength;

        public override TimeOnly Read(
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

        internal override TimeOnly ReadAsPropertyNameCore(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        private static TimeOnly ReadCore(ref KdlReader reader)
        {
            Debug.Assert(reader.TokenType is KdlTokenType.String or KdlTokenType.PropertyName);

            if (
                !KdlHelpers.IsInRangeInclusive(
                    reader.ValueLength,
                    MinimumTimeOnlyFormatLength,
                    MaximumEscapedTimeOnlyFormatLength
                )
            )
            {
                ThrowHelper.ThrowFormatException(DataType.TimeOnly);
            }

            scoped ReadOnlySpan<byte> source;
            if (!reader.HasValueSequence && !reader.ValueIsEscaped)
            {
                source = reader.ValueSpan;
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[MaximumEscapedTimeOnlyFormatLength];
                int bytesWritten = reader.CopyString(stackSpan);
                source = stackSpan[..bytesWritten];
            }

            byte firstChar = source[0];
            int firstSeparator = source.IndexOfAny((byte)'.', (byte)':');
            if (
                !KdlHelpers.IsDigit(firstChar)
                || firstSeparator < 0
                || source[firstSeparator] == (byte)'.'
            )
            {
                // Note: Utf8Parser.TryParse permits leading whitespace, negative values
                // and numbers of days so we need to exclude these cases here.
                ThrowHelper.ThrowFormatException(DataType.TimeOnly);
            }

            bool result = Utf8Parser.TryParse(
                source,
                out TimeSpan timespan,
                out int bytesConsumed,
                'c'
            );

            // Note: Utf8Parser.TryParse will return true for invalid input so
            // long as it starts with an integer. Example: "2021-06-18" or
            // "1$$$$$$$$$$". We need to check bytesConsumed to know if the
            // entire source was actually valid.

            if (!result || source.Length != bytesConsumed)
            {
                ThrowHelper.ThrowFormatException(DataType.TimeOnly);
            }

            Debug.Assert(
                TimeOnly.MinValue.ToTimeSpan() <= timespan
                    && timespan <= TimeOnly.MaxValue.ToTimeSpan()
            );
            return TimeOnly.FromTimeSpan(timespan);
        }

        public override void Write(KdlWriter writer, TimeOnly value, KdlSerializerOptions options)
        {
            Span<byte> output = stackalloc byte[MaximumTimeOnlyFormatLength];

            bool result = Utf8Formatter.TryFormat(
                value.ToTimeSpan(),
                output,
                out int bytesWritten,
                'c'
            );
            Debug.Assert(result);

            writer.WriteStringValue(output[..bytesWritten]);
        }

        internal override void WriteAsPropertyNameCore(
            KdlWriter writer,
            TimeOnly value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty
        )
        {
            Span<byte> output = stackalloc byte[MaximumTimeOnlyFormatLength];

            bool result = Utf8Formatter.TryFormat(
                value.ToTimeSpan(),
                output,
                out int bytesWritten,
                'c'
            );
            Debug.Assert(result);

            writer.WritePropertyName(output[..bytesWritten]);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new() { Type = KdlSchemaType.String, Format = "time" };
    }
}
