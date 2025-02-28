using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class UnsupportedTypeConverter<T>(string? errorMessage = null) : KdlConverter<T>
    {
        private readonly string? _errorMessage = errorMessage;

        public string ErrorMessage => _errorMessage ?? string.Format(SR.SerializeTypeInstanceNotSupported, typeof(T).FullName);

        public override T Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options) =>
            throw new NotSupportedException(ErrorMessage);

        public override void Write(KdlWriter writer, T value, KdlSerializerOptions options) =>
            throw new NotSupportedException(ErrorMessage);

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new()
            { Comment = "Unsupported .NET type", Not = KdlSchema.CreateTrueSchema() };
    }
}
