namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Defines a custom exception object that is thrown when invalid KDL text is encountered, when the defined maximum depth is passed,
    /// or the KDL text is not compatible with the type of a property on an object.
    /// </summary>
    [Serializable]
    public class KdlException : Exception
    {
        // Allow the message to mutate to avoid re-throwing and losing the StackTrace to an inner exception.
        internal string? _message;

        /// <summary>
        /// Creates a new exception object to relay error information to the user.
        /// </summary>
        /// <param name="message">The context specific error message.</param>
        /// <param name="lineNumber">The line number at which the invalid KDL was encountered (starting at 0) when deserializing.</param>
        /// <param name="bytePositionInLine">The byte count within the current line where the invalid KDL was encountered (starting at 0).</param>
        /// <param name="path">The path where the invalid KDL was encountered.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        /// <remarks>
        /// Note that the <paramref name="bytePositionInLine"/> counts the number of bytes (i.e. UTF-8 code units) and not characters or scalars.
        /// </remarks>
        public KdlException(string? message, string? path, long? lineNumber, long? bytePositionInLine, Exception? innerException) : base(message, innerException)
        {
            _message = message;
            LineNumber = lineNumber;
            BytePositionInLine = bytePositionInLine;
            Path = path;
        }

        /// <summary>
        /// Creates a new exception object to relay error information to the user.
        /// </summary>
        /// <param name="message">The context specific error message.</param>
        /// <param name="path">The path where the invalid KDL was encountered.</param>
        /// <param name="lineNumber">The line number at which the invalid KDL was encountered (starting at 0) when deserializing.</param>
        /// <param name="bytePositionInLine">The byte count within the current line where the invalid KDL was encountered (starting at 0).</param>
        /// <remarks>
        /// Note that the <paramref name="bytePositionInLine"/> counts the number of bytes (i.e. UTF-8 code units) and not characters or scalars.
        /// </remarks>
        public KdlException(string? message, string? path, long? lineNumber, long? bytePositionInLine) : base(message)
        {
            _message = message;
            LineNumber = lineNumber;
            BytePositionInLine = bytePositionInLine;
            Path = path;
        }

        /// <summary>
        /// Creates a new exception object to relay error information to the user.
        /// </summary>
        /// <param name="message">The context specific error message.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public KdlException(string? message, Exception? innerException) : base(message, innerException) => _message = message;

        /// <summary>
        /// Creates a new exception object to relay error information to the user.
        /// </summary>
        /// <param name="message">The context specific error message.</param>
        public KdlException(string? message) : base(message) => _message = message;

        /// <summary>
        /// Creates a new exception object to relay error information to the user.
        /// </summary>
        public KdlException() : base() { }

        /// <summary>
        /// Specifies that 'try' logic should append Path information to the exception message.
        /// </summary>
        internal bool AppendPathInformation { get; set; }

        /// <summary>
        /// The number of lines read so far before the exception (starting at 0).
        /// </summary>
        public long? LineNumber { get; internal set; }

        /// <summary>
        /// The number of bytes read within the current line before the exception (starting at 0).
        /// </summary>
        public long? BytePositionInLine { get; internal set; }

        /// <summary>
        /// The path within the KDL where the exception was encountered.
        /// </summary>
        public string? Path { get; internal set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => _message ?? base.Message;

        internal void SetMessage(string? message)
        {
            _message = message;
        }
    }
}