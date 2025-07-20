namespace System.Text.Encodings.Web
{
#pragma warning disable CS3001 // Argument type is not CLS-compliant
    public class DefaultKdlCommentEncoder(TextEncoderSettings textEncoderSettings)
        : KdlCommentEncoder
    {
        private readonly TextEncoderSettings textEncoderSettings = textEncoderSettings;

        public static KdlCommentEncoder BasicLatinSingleton { get; internal set; }

        public override int MaxOutputCharactersPerInputCharacter =>
            throw new NotImplementedException();

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength) =>
            throw new NotImplementedException();

        public override unsafe bool TryEncodeUnicodeScalar(
            int unicodeScalar,
            char* buffer,
            int bufferLength,
            out int numberOfCharactersWritten
        ) => throw new NotImplementedException();

        public override bool WillEncode(int unicodeScalar) => throw new NotImplementedException();
    }
#pragma warning restore CS3001 // Argument type is not CLS-compliant
}
