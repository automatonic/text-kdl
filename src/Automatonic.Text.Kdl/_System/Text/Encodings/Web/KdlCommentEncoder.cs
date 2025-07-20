using System.Text.Unicode;

namespace System.Text.Encodings.Web
{
    /// <summary>
    /// Represents a type used to do JavaScript encoding/escaping.
    /// </summary>
    public abstract class KdlCommentEncoder : TextEncoder
    {
        /// <summary>
        /// Returns a default built-in instance of <see cref="KdlCommentEncoder"/>.
        /// </summary>
        public static KdlCommentEncoder Default => DefaultKdlCommentEncoder.BasicLatinSingleton;

        /// <summary>
        /// Creates a new instance of KdlCommentEncoder with provided settings.
        /// </summary>
        /// <param name="settings">Settings used to control how the created <see cref="KdlCommentEncoder"/> encodes, primarily which characters to encode.</param>
        /// <returns>A new instance of the <see cref="KdlCommentEncoder"/>.</returns>
        public static KdlCommentEncoder Create(TextEncoderSettings settings)
        {
            return new DefaultKdlCommentEncoder(settings);
        }

        /// <summary>
        /// Creates a new instance of KdlCommentEncoder specifying character to be encoded.
        /// </summary>
        /// <param name="allowedRanges">Set of characters that the encoder is allowed to not encode.</param>
        /// <returns>A new instance of the <see cref="JavaScriptEncoder"/>.</returns>
        /// <remarks>Some characters in <paramref name="allowedRanges"/> might still get encoded, i.e. this parameter is just telling the encoder what ranges it is allowed to not encode, not what characters it must not encode.</remarks>
        public static KdlCommentEncoder Create(params UnicodeRange[] allowedRanges)
        {
            return new DefaultKdlCommentEncoder(new TextEncoderSettings(allowedRanges));
        }
    }
}
