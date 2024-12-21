using System.Buffers.Text;
using System.Diagnostics;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class TimeSpanConverter : KdlPrimitiveConverter<TimeSpan>
    {
        private const int MinimumTimeSpanFormatLength = 1; // d
        private const int MaximumTimeSpanFormatLength = 26; // -dddddddd.hh:mm:ss.fffffff
        private const int MaximumEscapedTimeSpanFormatLength = KdlConstants.MaxExpansionFactorWhileEscaping * MaximumTimeSpanFormatLength;

        public override TimeSpan Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType != KdlTokenType.String)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedString(reader.TokenType);
            }

            return ReadCore(ref reader);
        }

        internal override TimeSpan ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        private static TimeSpan ReadCore(ref KdlReader reader)
        {
            Debug.Assert(reader.TokenType is KdlTokenType.String or KdlTokenType.PropertyName);

            if (!KdlHelpers.IsInRangeInclusive(reader.ValueLength, MinimumTimeSpanFormatLength, MaximumEscapedTimeSpanFormatLength))
            {
                ThrowHelper.ThrowFormatException(DataType.TimeSpan);
            }

            scoped ReadOnlySpan<byte> source;
            if (!reader.HasValueSequence && !reader.ValueIsEscaped)
            {
                source = reader.ValueSpan;
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[MaximumEscapedTimeSpanFormatLength];
                int bytesWritten = reader.CopyString(stackSpan);
                source = stackSpan[..bytesWritten];
            }

            byte firstChar = source[0];
            if (!KdlHelpers.IsDigit(firstChar) && firstChar != '-')
            {
                // Note: Utf8Parser.TryParse allows for leading whitespace so we
                // need to exclude that case here.
                ThrowHelper.ThrowFormatException(DataType.TimeSpan);
            }

            bool result = Utf8Parser.TryParse(source, out TimeSpan tmpValue, out int bytesConsumed, 'c');

            // Note: Utf8Parser.TryParse will return true for invalid input so
            // long as it starts with an integer. Example: "2021-06-18" or
            // "1$$$$$$$$$$". We need to check bytesConsumed to know if the
            // entire source was actually valid.

            if (!result || source.Length != bytesConsumed)
            {
                ThrowHelper.ThrowFormatException(DataType.TimeSpan);
            }

            return tmpValue;
        }

        public override void Write(KdlWriter writer, TimeSpan value, KdlSerializerOptions options)
        {
            Span<byte> output = stackalloc byte[MaximumTimeSpanFormatLength];

            bool result = Utf8Formatter.TryFormat(value, output, out int bytesWritten, 'c');
            Debug.Assert(result);

            writer.WriteStringValue(output[..bytesWritten]);
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, TimeSpan value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            Span<byte> output = stackalloc byte[MaximumTimeSpanFormatLength];

            bool result = Utf8Formatter.TryFormat(value, output, out int bytesWritten, 'c');
            Debug.Assert(result);

            writer.WritePropertyName(output[..bytesWritten]);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => new()
        {
            Type = KdlSchemaType.String,
            Comment = "Represents a System.TimeSpan value.",
            Pattern = @"^-?(\d+\.)?\d{2}:\d{2}:\d{2}(\.\d{1,7})?$"
        };
    }
}
