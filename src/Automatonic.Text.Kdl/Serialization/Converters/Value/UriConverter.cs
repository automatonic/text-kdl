using System.Diagnostics;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class UriConverter : KdlPrimitiveConverter<Uri?>
    {
        public override Uri? Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            return reader.TokenType is KdlTokenType.Null ? null : ReadCore(ref reader);
        }

        public override void Write(KdlWriter writer, Uri? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.OriginalString);
        }

        internal override Uri ReadAsPropertyNameCore(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(reader.TokenType is KdlTokenType.PropertyName);
            return ReadCore(ref reader);
        }

        private static Uri ReadCore(ref KdlReader reader)
        {
            string? uriString = reader.GetString();

            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out Uri? value))
            {
                ThrowHelper.ThrowKdlException();
            }

            return value;
        }

        internal override void WriteAsPropertyNameCore(
            KdlWriter writer,
            Uri value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty
        )
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

            writer.WritePropertyName(value.OriginalString);
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new() { Type = KdlSchemaType.String, Format = "uri" };
    }
}
