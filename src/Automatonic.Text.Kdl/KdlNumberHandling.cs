namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Determines how <see cref="KdlSerializer"/> handles numbers when serializing and deserializing.
    /// <remarks>
    /// The behavior of <see cref="WriteAsString"/> and <see cref="AllowNamedFloatingPointLiterals"/> is not defined by the JSON specification. Altering the default number handling can potentially produce JSON that cannot be parsed by other JSON implementations.
    /// </remarks>
    /// </summary>
    [Flags]
    public enum KdlNumberHandling
    {
        /// <summary>
        /// Numbers will only be read from <see cref="KdlTokenType.Number"/> tokens and will only be written as JSON numbers (without quotes).
        /// </summary>
        Strict = 0x0,

        /// <summary>
        /// Numbers can be read from <see cref="KdlTokenType.String"/> tokens.
        /// Does not prevent numbers from being read from <see cref="KdlTokenType.Number"/> token.
        /// Strings that have escaped characters will be unescaped before reading.
        /// Leading or trailing trivia within the string token, including whitespace, is not allowed.
        /// </summary>
        AllowReadingFromString = 0x1,

        /// <summary>
        /// Numbers will be written as JSON strings (with quotes), not as JSON numbers.
        /// <remarks>
        /// This behavior is not defined by the JSON specification. Altering the default number handling can potentially produce JSON that cannot be parsed by other JSON implementations.
        /// </remarks>
        /// </summary>
        WriteAsString = 0x2,

        /// <summary>
        /// The "NaN", "Infinity", and "-Infinity" <see cref="KdlTokenType.String"/> tokens can be read as
        /// floating-point constants, and the <see cref="float"/> and <see cref="double"/> values for these
        /// constants (such as <see cref="float.PositiveInfinity"/> and <see cref="double.NaN"/>)
        /// will be written as their corresponding JSON string representations.
        /// Strings that have escaped characters will be unescaped before reading.
        /// Leading or trailing trivia within the string token, including whitespace, is not allowed.
        /// <remarks>
        /// This behavior is not defined by the JSON specification. Altering the default number handling can potentially produce JSON that cannot be parsed by other JSON implementations.
        /// </remarks>
        /// </summary>
        AllowNamedFloatingPointLiterals = 0x4
    }
}
