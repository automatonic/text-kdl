namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Defines an opaque type that holds and saves all the relevant state information which must be provided
    /// to the <see cref="KdlReader"/> to continue reading after processing incomplete data.
    /// This type is required to support reentrancy when reading incomplete data, and to continue
    /// reading once more data is available. Unlike the <see cref="KdlReader"/>, which is a ref struct,
    /// this type can survive across async/await boundaries and hence this type is required to provide
    /// support for reading in more data asynchronously before continuing with a new instance of the <see cref="KdlReader"/>.
    /// </summary>
    public readonly struct KdlReaderState
    {
        internal readonly long _lineNumber;
        internal readonly long _bytePositionInLine;
        internal readonly bool _inObject;
        internal readonly bool _isNotPrimitive;
        internal readonly bool _valueIsEscaped;
        internal readonly bool _trailingCommaBeforeComment;
        internal readonly KdlTokenType _tokenType;
        internal readonly KdlTokenType _previousTokenType;
        internal readonly KdlReaderOptions _readerOptions;
        internal readonly NodeStack _nodeStack;

        /// <summary>
        /// Constructs a new <see cref="KdlReaderState"/> instance.
        /// </summary>
        /// <param name="options">Defines the customized behavior of the <see cref="KdlReader"/>
        /// that is different from the KDL RFC (for example how to handle comments or maximum depth allowed when reading).
        /// By default, the <see cref="KdlReader"/> follows the KDL RFC strictly (i.e. comments within the KDL are invalid) and reads up to a maximum depth of 64.</param>
        /// <remarks>
        /// An instance of this state must be passed to the <see cref="KdlReader"/> ctor with the KDL data.
        /// Unlike the <see cref="KdlReader"/>, which is a ref struct, the state can survive
        /// across async/await boundaries and hence this type is required to provide support for reading
        /// in more data asynchronously before continuing with a new instance of the <see cref="KdlReader"/>.
        /// </remarks>
        public KdlReaderState(KdlReaderOptions options = default)
        {
            _lineNumber = default;
            _bytePositionInLine = default;
            _inObject = default;
            _isNotPrimitive = default;
            _valueIsEscaped = default;
            _trailingCommaBeforeComment = default;
            _tokenType = default;
            _previousTokenType = default;
            _readerOptions = options;

            // Only allocate if the user reads a KDL payload beyond the depth that the _allocationFreeContainer can handle.
            // This way we avoid allocations in the common, default cases, and allocate lazily.
            _nodeStack = default;
        }

        internal KdlReaderState(
            long lineNumber,
            long bytePositionInLine,
            bool inObject,
            bool isNotPrimitive,
            bool valueIsEscaped,
            bool trailingCommaBeforeComment,
            KdlTokenType tokenType,
            KdlTokenType previousTokenType,
            KdlReaderOptions readerOptions,
            NodeStack nodeStack
        )
        {
            _lineNumber = lineNumber;
            _bytePositionInLine = bytePositionInLine;
            _inObject = inObject;
            _isNotPrimitive = isNotPrimitive;
            _valueIsEscaped = valueIsEscaped;
            _trailingCommaBeforeComment = trailingCommaBeforeComment;
            _tokenType = tokenType;
            _previousTokenType = previousTokenType;
            _readerOptions = readerOptions;
            _nodeStack = nodeStack;
        }

        /// <summary>
        /// Gets the custom behavior when reading KDL using
        /// the <see cref="KdlReader"/> that may deviate from strict adherence
        /// to the KDL specification, which is the default behavior.
        /// </summary>
        public KdlReaderOptions Options => _readerOptions;
    }
}
