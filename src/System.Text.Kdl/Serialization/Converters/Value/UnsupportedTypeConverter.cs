using System.Text.Kdl.Schema;
using System.Text.Kdl.Nodes;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class UnsupportedTypeConverter<T> : KdlConverter<T>
    {
        private readonly string? _errorMessage;

        public UnsupportedTypeConverter(string? errorMessage = null) => _errorMessage = errorMessage;

        public string ErrorMessage => _errorMessage ?? string.Format(SR.SerializeTypeInstanceNotSupported, typeof(T).FullName);

        public override T Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options) =>
            throw new NotSupportedException(ErrorMessage);

        public override void Write(KdlWriter writer, T value, KdlSerializerOptions options) =>
            throw new NotSupportedException(ErrorMessage);

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new KdlSchema { Comment = "Unsupported .NET type", Not = KdlSchema.CreateTrueSchema() };
    }
}
