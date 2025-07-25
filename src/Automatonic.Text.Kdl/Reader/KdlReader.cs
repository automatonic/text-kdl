﻿using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Provides a high-performance API for forward-only, read-only access to the (always UTF-8 encoded) KDL text.
    /// It processes the text sequentially with no caching and adheres strictly to the KDL Spec
    /// (https://github.com/kdl-org/kdl/blob/main/SPEC.md). When it encounters invalid KDL, it throws
    /// a KdlException with basic error information like line number and byte position on the line.
    /// Since this type is a ref struct, it does not directly support async. However, it does provide
    /// support for reentrancy to read incomplete data, and continue reading once more data is presented.
    /// To be able to set max depth while reading OR allow skipping comments, create an instance of
    /// <see cref="KdlReaderState"/> and pass that in to the reader.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public ref partial struct KdlReader
    {
        private ReadOnlySpan<byte> _buffer;

        private readonly bool _isFinalBlock;
        private readonly bool _isInputSequence;

        private long _lineNumber;
        private long _bytePositionInLine;

        // bytes consumed in the current segment (not token)
        private int _consumed;
        private bool _inNode;
        private bool _isNotPrimitive;
        private KdlTokenType _tokenType;
        private KdlTokenType _previousTokenType;
        private KdlReaderOptions _readerOptions;
        private NodeStack _nodeStack;

        private long _totalConsumed;
        private bool _isLastSegment;
        private readonly bool _isMultiSegment;
        private bool _trailingCommaBeforeComment;

        private SequencePosition _nextPosition;
        private SequencePosition _currentPosition;
        private readonly ReadOnlySequence<byte> _sequence;

        private readonly bool IsLastSpan => _isFinalBlock && (!_isMultiSegment || _isLastSegment);

        internal readonly ReadOnlySequence<byte> OriginalSequence => _sequence;

        internal readonly ReadOnlySpan<byte> OriginalSpan => _sequence.IsEmpty ? _buffer : default;

        internal readonly int ValueLength =>
            HasValueSequence ? checked((int)ValueSequence.Length) : ValueSpan.Length;

        /// <summary>
        /// Gets the value of the last processed token as a ReadOnlySpan&lt;byte&gt; slice
        /// of the input payload. If the KDL is provided within a ReadOnlySequence&lt;byte&gt;
        /// and the slice that represents the token value fits in a single segment, then
        /// <see cref="ValueSpan"/> will contain the sliced value since it can be represented as a span.
        /// Otherwise, the <see cref="ValueSequence"/> will contain the token value.
        /// </summary>
        /// <remarks>
        /// If <see cref="HasValueSequence"/> is true, <see cref="ValueSpan"/> contains useless data, likely for
        /// a previous single-segment token. Therefore, only access <see cref="ValueSpan"/> if <see cref="HasValueSequence"/> is false.
        /// Otherwise, the token value must be accessed from <see cref="ValueSequence"/>.
        /// </remarks>
        public ReadOnlySpan<byte> ValueSpan { get; private set; }

        /// <summary>
        /// Returns the total amount of bytes consumed by the <see cref="KdlReader"/> so far
        /// for the current instance of the <see cref="KdlReader"/> with the given UTF-8 encoded input text.
        /// </summary>
        public readonly long BytesConsumed
        {
            get
            {
#if DEBUG
                if (!_isInputSequence)
                {
                    Debug.Assert(_totalConsumed == 0);
                }
#endif
                return _totalConsumed + _consumed;
            }
        }

        /// <summary>
        /// Returns the index that the last processed KDL token starts at
        /// within the given UTF-8 encoded input text, skipping any white space.
        /// </summary>
        /// <remarks>
        /// For KDL strings (including property names), this points to before the start quote.
        /// For comments, this points to before the first comment delimiter (i.e. '/').
        /// </remarks>
        public long TokenStartIndex { get; private set; }

        /// <summary>
        /// Tracks the recursive depth of the nested objects / arrays within the KDL text
        /// processed so far. This provides the depth of the current token.
        /// </summary>
        public readonly int CurrentDepth
        {
            get
            {
                int readerDepth = _nodeStack.CurrentDepth;
                if (TokenType is KdlTokenType.StartChildrenBlock or KdlTokenType.StartChildrenBlock)
                {
                    Debug.Assert(readerDepth >= 1);
                    readerDepth--;
                }
                return readerDepth;
            }
        }

        internal readonly bool IsInArray => !_inNode;

        /// <summary>
        /// Gets the type of the last processed KDL token in the UTF-8 encoded KDL text.
        /// </summary>
        public readonly KdlTokenType TokenType => _tokenType;

        /// <summary>
        /// Lets the caller know which of the two 'Value' properties to read to get the
        /// token value. For input data within a ReadOnlySpan&lt;byte&gt; this will
        /// always return false. For input data within a ReadOnlySequence&lt;byte&gt;, this
        /// will only return true if the token value straddles more than a single segment and
        /// hence couldn't be represented as a span.
        /// </summary>
        public bool HasValueSequence { get; private set; }

        /// <summary>
        /// Lets the caller know whether the current <see cref="ValueSpan" /> or <see cref="ValueSequence"/> properties
        /// contain escape sequences per RFC 8259 section 7, and therefore require unescaping before being consumed.
        /// </summary>
        public bool ValueIsEscaped { get; private set; }

        /// <summary>
        /// Returns the mode of this instance of the <see cref="KdlReader"/>.
        /// True when the reader was constructed with the input span containing the entire data to process.
        /// False when the reader was constructed knowing that the input span may contain partial data with more data to follow.
        /// </summary>
        public readonly bool IsFinalBlock => _isFinalBlock;

        /// <summary>
        /// Gets the value of the last processed token as a ReadOnlySpan&lt;byte&gt; slice
        /// of the input payload. If the KDL is provided within a ReadOnlySequence&lt;byte&gt;
        /// and the slice that represents the token value fits in a single segment, then
        /// <see cref="ValueSpan"/> will contain the sliced value since it can be represented as a span.
        /// Otherwise, the <see cref="ValueSequence"/> will contain the token value.
        /// </summary>
        /// <remarks>
        /// If <see cref="HasValueSequence"/> is false, <see cref="ValueSequence"/> contains useless data, likely for
        /// a previous multi-segment token. Therefore, only access <see cref="ValueSequence"/> if <see cref="HasValueSequence"/> is true.
        /// Otherwise, the token value must be accessed from <see cref="ValueSpan"/>.
        /// </remarks>
        public ReadOnlySequence<byte> ValueSequence { get; private set; }

        /// <summary>
        /// Returns the current <see cref="SequencePosition"/> within the provided UTF-8 encoded
        /// input ReadOnlySequence&lt;byte&gt;. If the <see cref="KdlReader"/> was constructed
        /// with a ReadOnlySpan&lt;byte&gt; instead, this will always return a default <see cref="SequencePosition"/>.
        /// </summary>
        public readonly SequencePosition Position
        {
            get
            {
                if (_isInputSequence)
                {
                    Debug.Assert(_currentPosition.GetObject() != null);
                    return _sequence.GetPosition(_consumed, _currentPosition);
                }
                return default;
            }
        }

        /// <summary>
        /// Returns the current snapshot of the <see cref="KdlReader"/> state which must
        /// be captured by the caller and passed back in to the <see cref="KdlReader"/> ctor with more data.
        /// Unlike the <see cref="KdlReader"/>, which is a ref struct, the state can survive
        /// across async/await boundaries and hence this type is required to provide support for reading
        /// in more data asynchronously before continuing with a new instance of the <see cref="KdlReader"/>.
        /// </summary>
        public readonly KdlReaderState CurrentState =>
            new(
                lineNumber: _lineNumber,
                bytePositionInLine: _bytePositionInLine,
                inObject: _inNode,
                isNotPrimitive: _isNotPrimitive,
                valueIsEscaped: ValueIsEscaped,
                trailingCommaBeforeComment: _trailingCommaBeforeComment,
                tokenType: _tokenType,
                previousTokenType: _previousTokenType,
                readerOptions: _readerOptions,
                nodeStack: _nodeStack
            );

        /// <summary>
        /// Constructs a new <see cref="KdlReader"/> instance.
        /// </summary>
        /// <param name="kdlData">The ReadOnlySpan&lt;byte&gt; containing the UTF-8 encoded KDL text to process.</param>
        /// <param name="isFinalBlock">True when the input span contains the entire data to process.
        /// Set to false only if it is known that the input span contains partial data with more data to follow.</param>
        /// <param name="state">If this is the first call to the ctor, pass in a default state. Otherwise,
        /// capture the state from the previous instance of the <see cref="KdlReader"/> and pass that back.</param>
        /// <remarks>
        /// Since this type is a ref struct, it is a stack-only type and all the limitations of ref structs apply to it.
        /// This is the reason why the ctor accepts a <see cref="KdlReaderState"/>.
        /// </remarks>
        public KdlReader(ReadOnlySpan<byte> kdlData, bool isFinalBlock, KdlReaderState state)
        {
            _buffer = kdlData;

            _isFinalBlock = isFinalBlock;
            _isInputSequence = false;

            _lineNumber = state._lineNumber;
            _bytePositionInLine = state._bytePositionInLine;
            _inNode = state._inObject;
            _isNotPrimitive = state._isNotPrimitive;
            ValueIsEscaped = state._valueIsEscaped;
            _trailingCommaBeforeComment = state._trailingCommaBeforeComment;
            _tokenType = state._tokenType;
            _previousTokenType = state._previousTokenType;
            _readerOptions = state._readerOptions;
            if (_readerOptions.MaxDepth == 0)
            {
                _readerOptions.MaxDepth = KdlReaderOptions.DefaultMaxDepth; // If max depth is not set, revert to the default depth.
            }
            _nodeStack = state._nodeStack;

            _consumed = 0;
            TokenStartIndex = 0;
            _totalConsumed = 0;
            _isLastSegment = _isFinalBlock;
            _isMultiSegment = false;

            ValueSpan = [];

            _currentPosition = default;
            _nextPosition = default;
            _sequence = default;
            HasValueSequence = false;
            ValueSequence = ReadOnlySequence<byte>.Empty;
        }

        /// <summary>
        /// Constructs a new <see cref="KdlReader"/> instance.
        /// </summary>
        /// <param name="kdlData">The ReadOnlySpan&lt;byte&gt; containing the UTF-8 encoded KDL text to process.</param>
        /// <param name="options">Defines the customized behavior of the <see cref="KdlReader"/>
        /// that is different from the KDL RFC (for example how to handle comments or maximum depth allowed when reading).
        /// By default, the <see cref="KdlReader"/> follows the KDL RFC strictly (i.e. comments within the KDL are invalid) and reads up to a maximum depth of 64.</param>
        /// <remarks>
        ///   <para>
        ///     Since this type is a ref struct, it is a stack-only type and all the limitations of ref structs apply to it.
        ///   </para>
        ///   <para>
        ///     This assumes that the entire KDL payload is passed in (equivalent to <see cref="IsFinalBlock"/> = true)
        ///   </para>
        /// </remarks>
        public KdlReader(ReadOnlySpan<byte> kdlData, KdlReaderOptions options = default)
            : this(kdlData, isFinalBlock: true, new KdlReaderState(options)) { }

        /// <summary>
        /// Read the next KDL token from input source.
        /// </summary>
        /// <returns>True if the token was read successfully, else false.</returns>
        /// <exception cref="KdlException">
        /// Thrown when an invalid KDL token is encountered according to the KDL RFC
        /// or if the current depth exceeds the recursive limit set by the max depth.
        /// </exception>
        public bool Read()
        {
            bool retVal = _isMultiSegment ? ReadMultiSegment() : ReadSingleSegment();
            return retVal;
        }

        /// <summary>
        /// Skips the children of the current KDL token.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the reader was given partial data with more data to follow (i.e. <see cref="IsFinalBlock"/> is false).
        /// </exception>
        /// <exception cref="KdlException">
        /// Thrown when an invalid KDL token is encountered while skipping, according to the KDL RFC,
        /// or if the current depth exceeds the recursive limit set by the max depth.
        /// </exception>
        /// <remarks>
        /// When <see cref="TokenType"/> is <see cref="KdlTokenType.PropertyName" />, the reader first moves to the property value.
        /// When <see cref="TokenType"/> (originally, or after advancing) is <see cref="KdlTokenType.StartChildrenBlock" /> or
        /// <see cref="KdlTokenType.StartChildrenBlock" />, the reader advances to the matching
        /// <see cref="KdlTokenType.EndChildrenBlock" /> or <see cref="KdlTokenType.EndChildrenBlock" />.
        ///
        /// For all other token types, the reader does not move. After the next call to <see cref="Read"/>, the reader will be at
        /// the next value (when in an array), the next property name (when in an object), or the end array/object token.
        /// </remarks>
        public void Skip()
        {
            if (!_isFinalBlock)
            {
                ThrowHelper.ThrowInvalidOperationException_CannotSkipOnPartial();
            }

            SkipHelper();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipHelper()
        {
            Debug.Assert(_isFinalBlock);

            if (TokenType is KdlTokenType.PropertyName)
            {
                bool result = Read();
                // Since _isFinalBlock == true here, and the KDL token is not a primitive value or comment.
                // Read() is guaranteed to return true OR throw for invalid/incomplete data.
                Debug.Assert(result);
            }

            if (TokenType is KdlTokenType.StartChildrenBlock or KdlTokenType.StartChildrenBlock)
            {
                int depth = CurrentDepth;
                do
                {
                    bool result = Read();
                    // Since _isFinalBlock == true here, and the KDL token is not a primitive value or comment.
                    // Read() is guaranteed to return true OR throw for invalid/incomplete data.
                    Debug.Assert(result);
                } while (depth < CurrentDepth);
            }
        }

        /// <summary>
        /// Tries to skip the children of the current KDL token.
        /// </summary>
        /// <returns>True if there was enough data for the children to be skipped successfully, else false.</returns>
        /// <exception cref="KdlException">
        /// Thrown when an invalid KDL token is encountered while skipping, according to the KDL RFC,
        /// or if the current depth exceeds the recursive limit set by the max depth.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     If the reader did not have enough data to completely skip the children of the current token,
        ///     it will be reset to the state it was in before the method was called.
        ///   </para>
        ///   <para>
        ///     When <see cref="TokenType"/> is <see cref="KdlTokenType.PropertyName" />, the reader first moves to the property value.
        ///     When <see cref="TokenType"/> (originally, or after advancing) is <see cref="KdlTokenType.StartChildrenBlock" /> or
        ///     <see cref="KdlTokenType.StartChildrenBlock" />, the reader advances to the matching
        ///     <see cref="KdlTokenType.EndChildrenBlock" /> or <see cref="KdlTokenType.EndChildrenBlock" />.
        ///
        ///     For all other token types, the reader does not move. After the next call to <see cref="Read"/>, the reader will be at
        ///     the next value (when in an array), the next property name (when in an object), or the end array/object token.
        ///   </para>
        /// </remarks>
        public bool TrySkip()
        {
            if (_isFinalBlock)
            {
                SkipHelper();
                return true;
            }

            KdlReader restore = this;
            bool success = TrySkipPartial(targetDepth: CurrentDepth);
            if (!success)
            {
                // Roll back the reader if it contains partial data.
                this = restore;
            }

            return success;
        }

        /// <summary>
        /// Tries to skip the children of the current KDL token, advancing the reader even if there is not enough data.
        /// The skip operation can be resumed later, provided that the same <paramref name="targetDepth" /> is passed.
        /// </summary>
        /// <param name="targetDepth">The target depth we want to eventually skip to.</param>
        /// <returns>True if the entire KDL value has been skipped.</returns>
        internal bool TrySkipPartial(int targetDepth)
        {
            Debug.Assert(0 <= targetDepth && targetDepth <= CurrentDepth);

            if (targetDepth == CurrentDepth)
            {
                // This is the first call to TrySkipHelper.
                if (TokenType is KdlTokenType.PropertyName)
                {
                    // Skip any property name tokens preceding the value.
                    if (!Read())
                    {
                        return false;
                    }
                }

                if (
                    TokenType
                    is not (KdlTokenType.StartChildrenBlock or KdlTokenType.StartChildrenBlock)
                )
                {
                    // The next value is not an object or array, so there is nothing to skip.
                    return true;
                }
            }

            // Start or resume iterating through the KDL object or array.
            do
            {
                if (!Read())
                {
                    return false;
                }
            } while (targetDepth < CurrentDepth);

            Debug.Assert(targetDepth == CurrentDepth);
            return true;
        }

        /// <summary>
        /// Compares the UTF-8 encoded text to the unescaped KDL token value in the source and returns true if they match.
        /// </summary>
        /// <param name="utf8Text">The UTF-8 encoded text to compare against.</param>
        /// <returns>True if the KDL token value in the source matches the UTF-8 encoded look up text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to find a text match on a KDL token that is not a string
        /// (i.e. other than <see cref="KdlTokenType.String"/> or <see cref="KdlTokenType.PropertyName"/>).
        /// <seealso cref="TokenType" />
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     If the look up text is invalid UTF-8 text, the method will return false since you cannot have
        ///     invalid UTF-8 within the KDL payload.
        ///   </para>
        ///   <para>
        ///     The comparison of the KDL token value in the source and the look up text is done by first unescaping the KDL value in source,
        ///     if required. The look up text is matched as is, without any modifications to it.
        ///   </para>
        /// </remarks>
        public readonly bool ValueTextEquals(ReadOnlySpan<byte> utf8Text)
        {
            if (!IsTokenTypeString(TokenType))
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedStringComparison(TokenType);
            }

            return TextEqualsHelper(utf8Text);
        }

        /// <summary>
        /// Compares the string text to the unescaped KDL token value in the source and returns true if they match.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>True if the KDL token value in the source matches the look up text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to find a text match on a KDL token that is not a string
        /// (i.e. other than <see cref="KdlTokenType.String"/> or <see cref="KdlTokenType.PropertyName"/>).
        /// <seealso cref="TokenType" />
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     If the look up text is invalid UTF-8 text, the method will return false since you cannot have
        ///     invalid UTF-8 within the KDL payload.
        ///   </para>
        ///   <para>
        ///     The comparison of the KDL token value in the source and the look up text is done by first unescaping the KDL value in source,
        ///     if required. The look up text is matched as is, without any modifications to it.
        ///   </para>
        /// </remarks>
        public readonly bool ValueTextEquals(string? text)
        {
            return ValueTextEquals(text.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool TextEqualsHelper(ReadOnlySpan<byte> otherUtf8Text)
        {
            if (HasValueSequence)
            {
                return CompareToSequence(otherUtf8Text);
            }

            if (ValueIsEscaped)
            {
                return UnescapeAndCompare(otherUtf8Text);
            }

            return otherUtf8Text.SequenceEqual(ValueSpan);
        }

        /// <summary>
        /// Compares the text to the unescaped KDL token value in the source and returns true if they match.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>True if the KDL token value in the source matches the look up text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to find a text match on a KDL token that is not a string
        /// (i.e. other than <see cref="KdlTokenType.String"/> or <see cref="KdlTokenType.PropertyName"/>).
        /// <seealso cref="TokenType" />
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     If the look up text is invalid or incomplete UTF-16 text (i.e. unpaired surrogates), the method will return false
        ///     since you cannot have invalid UTF-16 within the KDL payload.
        ///   </para>
        ///   <para>
        ///     The comparison of the KDL token value in the source and the look up text is done by first unescaping the KDL value in source,
        ///     if required. The look up text is matched as is, without any modifications to it.
        ///   </para>
        /// </remarks>
        public readonly bool ValueTextEquals(ReadOnlySpan<char> text)
        {
            if (!IsTokenTypeString(TokenType))
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedStringComparison(TokenType);
            }

            if (MatchNotPossible(text.Length))
            {
                return false;
            }

            byte[]? otherUtf8TextArray = null;

            scoped Span<byte> otherUtf8Text;

            int length = checked(text.Length * KdlConstants.MaxExpansionFactorWhileTranscoding);

            if (length > KdlConstants.StackallocByteThreshold)
            {
                otherUtf8TextArray = ArrayPool<byte>.Shared.Rent(length);
                otherUtf8Text = otherUtf8TextArray;
            }
            else
            {
                otherUtf8Text = stackalloc byte[KdlConstants.StackallocByteThreshold];
            }

            OperationStatus status = KdlWriterHelper.ToUtf8(text, otherUtf8Text, out int written);
            Debug.Assert(status != OperationStatus.DestinationTooSmall);
            bool result;
            if (status == OperationStatus.InvalidData)
            {
                result = false;
            }
            else
            {
                Debug.Assert(status == OperationStatus.Done);
                result = TextEqualsHelper(otherUtf8Text[..written]);
            }

            if (otherUtf8TextArray != null)
            {
                otherUtf8Text[..written].Clear();
                ArrayPool<byte>.Shared.Return(otherUtf8TextArray);
            }

            return result;
        }

        private readonly bool CompareToSequence(ReadOnlySpan<byte> other)
        {
            Debug.Assert(HasValueSequence);

            if (ValueIsEscaped)
            {
                return UnescapeSequenceAndCompare(other);
            }

            ReadOnlySequence<byte> localSequence = ValueSequence;

            Debug.Assert(!localSequence.IsSingleSegment);

            if (localSequence.Length != other.Length)
            {
                return false;
            }

            int matchedSoFar = 0;

            foreach (ReadOnlyMemory<byte> memory in localSequence)
            {
                ReadOnlySpan<byte> span = memory.Span;

                if (other[matchedSoFar..].StartsWith(span))
                {
                    matchedSoFar += span.Length;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private readonly bool UnescapeAndCompare(ReadOnlySpan<byte> other)
        {
            Debug.Assert(!HasValueSequence);
            ReadOnlySpan<byte> localSpan = ValueSpan;

            if (
                localSpan.Length < other.Length
                || localSpan.Length / KdlConstants.MaxExpansionFactorWhileEscaping > other.Length
            )
            {
                return false;
            }

            int idx = localSpan.IndexOf(KdlConstants.BackSlash);
            Debug.Assert(idx != -1);

            if (!other.StartsWith(localSpan[..idx]))
            {
                return false;
            }

            return KdlReaderHelper.UnescapeAndCompare(localSpan[idx..], other[idx..]);
        }

        private readonly bool UnescapeSequenceAndCompare(ReadOnlySpan<byte> other)
        {
            Debug.Assert(HasValueSequence);
            Debug.Assert(!ValueSequence.IsSingleSegment);

            ReadOnlySequence<byte> localSequence = ValueSequence;
            long sequenceLength = localSequence.Length;

            // The KDL token value will at most shrink by 6 when unescaping.
            // If it is still larger than the lookup string, there is no value in unescaping and doing the comparison.
            if (
                sequenceLength < other.Length
                || sequenceLength / KdlConstants.MaxExpansionFactorWhileEscaping > other.Length
            )
            {
                return false;
            }

            int matchedSoFar = 0;

            bool result = false;

            foreach (ReadOnlyMemory<byte> memory in localSequence)
            {
                ReadOnlySpan<byte> span = memory.Span;

                int idx = span.IndexOf(KdlConstants.BackSlash);

                if (idx != -1)
                {
                    if (!other[matchedSoFar..].StartsWith(span[..idx]))
                    {
                        break;
                    }
                    matchedSoFar += idx;

                    other = other[matchedSoFar..];
                    localSequence = localSequence.Slice(matchedSoFar);

                    if (localSequence.IsSingleSegment)
                    {
                        result = KdlReaderHelper.UnescapeAndCompare(
                            localSequence.First.Span,
                            other
                        );
                    }
                    else
                    {
                        result = KdlReaderHelper.UnescapeAndCompare(localSequence, other);
                    }
                    break;
                }

                if (!other[matchedSoFar..].StartsWith(span))
                {
                    break;
                }
                matchedSoFar += span.Length;
            }

            return result;
        }

        // Returns true if the TokenType is a primitive string "value", i.e. PropertyName or String
        // Otherwise, return false.
        private static bool IsTokenTypeString(KdlTokenType tokenType)
        {
            return tokenType is KdlTokenType.PropertyName or KdlTokenType.String;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool MatchNotPossible(int charTextLength)
        {
            if (HasValueSequence)
            {
                return MatchNotPossibleSequence(charTextLength);
            }

            int sourceLength = ValueSpan.Length;

            // Transcoding from UTF-16 to UTF-8 will change the length by somwhere between 1x and 3x.
            // Unescaping the token value will at most shrink its length by 6x.
            // There is no point incurring the transcoding/unescaping/comparing cost if:
            // - The token value is smaller than charTextLength
            // - The token value needs to be transcoded AND unescaped and it is more than 6x larger than charTextLength
            //      - For an ASCII UTF-16 characters, transcoding = 1x, escaping = 6x => 6x factor
            //      - For non-ASCII UTF-16 characters within the BMP, transcoding = 2-3x, but they are represented as a single escaped hex value, \uXXXX => 6x factor
            //      - For non-ASCII UTF-16 characters outside of the BMP, transcoding = 4x, but the surrogate pair (2 characters) are represented by 16 bytes \uXXXX\uXXXX => 6x factor
            // - The token value needs to be transcoded, but NOT escaped and it is more than 3x larger than charTextLength
            //      - For an ASCII UTF-16 characters, transcoding = 1x,
            //      - For non-ASCII UTF-16 characters within the BMP, transcoding = 2-3x,
            //      - For non-ASCII UTF-16 characters outside of the BMP, transcoding = 2x, (surrogate pairs - 2 characters transcode to 4 UTF-8 bytes)

            if (
                sourceLength < charTextLength
                || sourceLength
                    / (
                        ValueIsEscaped
                            ? KdlConstants.MaxExpansionFactorWhileEscaping
                            : KdlConstants.MaxExpansionFactorWhileTranscoding
                    )
                    > charTextLength
            )
            {
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private readonly bool MatchNotPossibleSequence(int charTextLength)
        {
            long sourceLength = ValueSequence.Length;

            if (
                sourceLength < charTextLength
                || sourceLength
                    / (
                        ValueIsEscaped
                            ? KdlConstants.MaxExpansionFactorWhileEscaping
                            : KdlConstants.MaxExpansionFactorWhileTranscoding
                    )
                    > charTextLength
            )
            {
                return true;
            }
            return false;
        }

        private void StartChildrenBlock()
        {
            if (_nodeStack.CurrentDepth >= _readerOptions.MaxDepth)
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.NodeDepthTooLarge
                );
            }

            _nodeStack.Push();

            ValueSpan = _buffer.Slice(_consumed, 1);
            _consumed++;
            _bytePositionInLine++;
            _tokenType = KdlTokenType.StartChildrenBlock;
            _inNode = true;
        }

        private void EndChildrenBlock()
        {
            if (!_inNode || _nodeStack.CurrentDepth <= 0)
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.MismatchedChildrenBlock,
                    KdlConstants.CloseBrace
                );
            }

            if (_trailingCommaBeforeComment)
            {
                if (!_readerOptions.AllowTrailingCommas)
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd
                    );
                }
                _trailingCommaBeforeComment = false;
            }

            _tokenType = KdlTokenType.EndChildrenBlock;
            ValueSpan = _buffer.Slice(_consumed, 1);

            UpdateBitStackOnEndToken();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBitStackOnEndToken()
        {
            _consumed++;
            _bytePositionInLine++;
            _inNode = _nodeStack.Pop();
        }

        private bool ReadSingleSegment()
        {
            bool retVal = false;
            ValueSpan = default;
            ValueIsEscaped = false;

            if (!HasMoreData())
            {
                goto Done;
            }

            byte first = _buffer[_consumed];

            // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
            // SkipWhiteSpace only skips the whitespace characters as defined by KDL RFC 8259 section 2.
            // We do not validate if 'first' is an invalid KDL byte here (such as control characters).
            // Those cases are captured in ConsumeNextToken and ConsumeValue.
            if (first <= KdlConstants.Space)
            {
                SkipWhiteSpace();
                if (!HasMoreData())
                {
                    goto Done;
                }
                first = _buffer[_consumed];
            }

            TokenStartIndex = _consumed;

            if (_tokenType == KdlTokenType.None)
            {
                goto ReadFirstToken;
            }

            if (first == KdlConstants.Slash)
            {
                retVal = ConsumeNextTokenOrRollback(first);
                goto Done;
            }

            if (_tokenType == KdlTokenType.StartChildrenBlock)
            {
                if (first == KdlConstants.CloseBrace)
                {
                    EndChildrenBlock();
                }
                else
                {
                    if (first != KdlConstants.Quote)
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyNotFound,
                            first
                        );
                    }

                    int prevConsumed = _consumed;
                    long prevPosition = _bytePositionInLine;
                    long prevLineNumber = _lineNumber;
                    retVal = ConsumePropertyName();
                    if (!retVal)
                    {
                        // roll back potential changes
                        _consumed = prevConsumed;
                        _tokenType = KdlTokenType.StartChildrenBlock;
                        _bytePositionInLine = prevPosition;
                        _lineNumber = prevLineNumber;
                    }
                    goto Done;
                }
            }
            // else if (_tokenType == KdlTokenType.StartChildrenBlock)
            // {
            //     if (first == KdlConstants.CloseBracket)
            //     {
            //         EndArray();
            //     }
            //     else
            //     {
            //         retVal = ConsumeValue(first);
            //         goto Done;
            //     }
            // }
            else if (_tokenType == KdlTokenType.PropertyName)
            {
                retVal = ConsumeValue(first);
                goto Done;
            }
            else
            {
                retVal = ConsumeNextTokenOrRollback(first);
                goto Done;
            }

            retVal = true;

            Done:
            return retVal;

            ReadFirstToken:
            retVal = ReadFirstToken(first);
            goto Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasMoreData()
        {
            if (_consumed >= (uint)_buffer.Length)
            {
                if (_isNotPrimitive && IsLastSpan)
                {
                    if (_nodeStack.CurrentDepth != 0)
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ZeroDepthAtEnd
                        );
                    }

                    if (
                        _readerOptions.CommentHandling == KdlCommentHandling.Allow
                        && _tokenType == KdlTokenType.Comment
                    )
                    {
                        return false;
                    }

                    if (
                        _tokenType
                        is not KdlTokenType.EndChildrenBlock
                            and not KdlTokenType.EndChildrenBlock
                    )
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.InvalidEndOfKdlNonPrimitive
                        );
                    }
                }
                return false;
            }
            return true;
        }

        // Unlike the parameter-less overload of HasMoreData, if there is no more data when this method is called, we know the KDL input is invalid.
        // This is because, this method is only called after a ',' (i.e. we expect a value/property name) or after
        // a property name, which means it must be followed by a value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasMoreData(ExceptionResource resource)
        {
            if (_consumed >= (uint)_buffer.Length)
            {
                if (IsLastSpan)
                {
                    ThrowHelper.ThrowKdlReaderException(ref this, resource);
                }
                return false;
            }
            return true;
        }

        private bool ReadFirstToken(byte first)
        {
            if (first == KdlConstants.OpenBrace)
            {
                _nodeStack.Push();
                _tokenType = KdlTokenType.StartChildrenBlock;
                ValueSpan = _buffer.Slice(_consumed, 1);
                _consumed++;
                _bytePositionInLine++;
                _inNode = true;
                _isNotPrimitive = true;
            }
            else
            {
                // Create local copy to avoid bounds checks.
                ReadOnlySpan<byte> localBuffer = _buffer;

                if (KdlHelpers.IsDigit(first) || first == '-')
                {
                    if (!TryGetNumber(localBuffer[_consumed..], out int numberOfBytes))
                    {
                        return false;
                    }
                    _tokenType = KdlTokenType.Number;
                    _consumed += numberOfBytes;
                    _bytePositionInLine += numberOfBytes;
                }
                else if (!ConsumeValue(first))
                {
                    return false;
                }

                _isNotPrimitive = _tokenType is KdlTokenType.StartChildrenBlock;
                // Intentionally fall out of the if-block to return true
            }
            return true;
        }

        private void SkipWhiteSpace()
        {
            // Create local copy to avoid bounds checks.
            ReadOnlySpan<byte> localBuffer = _buffer;
            for (; _consumed < localBuffer.Length; _consumed++)
            {
                byte val = localBuffer[_consumed];

                // KDL RFC 8259 section 2 says only these 4 characters count, not all of the Unicode definitions of whitespace.
                if (
                    val
                    is not KdlConstants.Space
                        and not KdlConstants.CarriageReturn
                        and not KdlConstants.LineFeed
                        and not KdlConstants.Tab
                )
                {
                    break;
                }

                if (val == KdlConstants.LineFeed)
                {
                    _lineNumber++;
                    _bytePositionInLine = 0;
                }
                else
                {
                    _bytePositionInLine++;
                }
            }
        }

        /// <summary>
        /// This method contains the logic for processing the next value token and determining
        /// what type of data it is.
        /// </summary>
        private bool ConsumeValue(byte marker)
        {
            while (true)
            {
                Debug.Assert(
                    (
                        _trailingCommaBeforeComment
                        && _readerOptions.CommentHandling == KdlCommentHandling.Allow
                    ) || !_trailingCommaBeforeComment
                );
                Debug.Assert(
                    (_trailingCommaBeforeComment && marker != KdlConstants.Slash)
                        || !_trailingCommaBeforeComment
                );
                _trailingCommaBeforeComment = false;

                if (marker == KdlConstants.Quote)
                {
                    return ConsumeString();
                }
                else if (marker == KdlConstants.OpenBrace)
                {
                    StartChildrenBlock();
                }
                else if (KdlHelpers.IsDigit(marker) || marker == '-')
                {
                    return ConsumeNumber();
                }
                else if (marker == 'f')
                {
                    return ConsumeLiteral(KdlConstants.FalseValue, KdlTokenType.False);
                }
                else if (marker == 't')
                {
                    return ConsumeLiteral(KdlConstants.TrueValue, KdlTokenType.True);
                }
                else if (marker == 'n')
                {
                    return ConsumeLiteral(KdlConstants.NullValue, KdlTokenType.Null);
                }
                else
                {
                    switch (_readerOptions.CommentHandling)
                    {
                        case KdlCommentHandling.Disallow:
                            break;
                        case KdlCommentHandling.Allow:
                            if (marker == KdlConstants.Slash)
                            {
                                return ConsumeComment();
                            }
                            break;
                        default:
                            Debug.Assert(_readerOptions.CommentHandling == KdlCommentHandling.Skip);
                            if (marker == KdlConstants.Slash)
                            {
                                if (SkipComment())
                                {
                                    if (_consumed >= (uint)_buffer.Length)
                                    {
                                        if (
                                            _isNotPrimitive
                                            && IsLastSpan
                                            && _tokenType != KdlTokenType.EndChildrenBlock
                                            && _tokenType != KdlTokenType.EndChildrenBlock
                                        )
                                        {
                                            ThrowHelper.ThrowKdlReaderException(
                                                ref this,
                                                ExceptionResource.InvalidEndOfKdlNonPrimitive
                                            );
                                        }
                                        return false;
                                    }

                                    marker = _buffer[_consumed];

                                    // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
                                    if (marker <= KdlConstants.Space)
                                    {
                                        SkipWhiteSpace();
                                        if (!HasMoreData())
                                        {
                                            return false;
                                        }
                                        marker = _buffer[_consumed];
                                    }

                                    TokenStartIndex = _consumed;

                                    // Skip comments and consume the actual KDL value.
                                    continue;
                                }
                                return false;
                            }
                            break;
                    }
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedStartOfValueNotFound,
                        marker
                    );
                }
                break;
            }
            return true;
        }

        // Consumes 'null', or 'true', or 'false'
        private bool ConsumeLiteral(ReadOnlySpan<byte> literal, KdlTokenType tokenType)
        {
            ReadOnlySpan<byte> span = _buffer[_consumed..];
            Debug.Assert(span.Length > 0);
            Debug.Assert(span[0] is (byte)'n' or (byte)'t' or (byte)'f');

            if (!span.StartsWith(literal))
            {
                return CheckLiteral(span, literal);
            }

            ValueSpan = span[..literal.Length];
            _tokenType = tokenType;
            _consumed += literal.Length;
            _bytePositionInLine += literal.Length;
            return true;
        }

        private bool CheckLiteral(ReadOnlySpan<byte> span, ReadOnlySpan<byte> literal)
        {
            Debug.Assert(span.Length > 0 && span[0] == literal[0]);

            int indexOfFirstMismatch = 0;

            for (int i = 1; i < literal.Length; i++)
            {
                if (span.Length > i)
                {
                    if (span[i] != literal[i])
                    {
                        _bytePositionInLine += i;
                        ThrowInvalidLiteral(span);
                    }
                }
                else
                {
                    indexOfFirstMismatch = i;
                    break;
                }
            }

            Debug.Assert(indexOfFirstMismatch > 0 && indexOfFirstMismatch < literal.Length);

            if (IsLastSpan)
            {
                _bytePositionInLine += indexOfFirstMismatch;
                ThrowInvalidLiteral(span);
            }
            return false;
        }

        private void ThrowInvalidLiteral(ReadOnlySpan<byte> span)
        {
            byte firstByte = span[0];

            ExceptionResource resource;
            switch (firstByte)
            {
                case (byte)'t':
                    resource = ExceptionResource.ExpectedTrue;
                    break;
                case (byte)'f':
                    resource = ExceptionResource.ExpectedFalse;
                    break;
                default:
                    Debug.Assert(firstByte == 'n');
                    resource = ExceptionResource.ExpectedNull;
                    break;
            }
            ThrowHelper.ThrowKdlReaderException(ref this, resource, bytes: span);
        }

        private bool ConsumeNumber()
        {
            if (!TryGetNumber(_buffer[_consumed..], out int consumed))
            {
                return false;
            }

            _tokenType = KdlTokenType.Number;
            _consumed += consumed;
            _bytePositionInLine += consumed;

            if (_consumed >= (uint)_buffer.Length)
            {
                Debug.Assert(IsLastSpan);

                // If there is no more data, and the KDL is not a single value, throw.
                if (_isNotPrimitive)
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedEndOfDigitNotFound,
                        _buffer[_consumed - 1]
                    );
                }
            }

            // If there is more data and the KDL is not a single value, assert that there is an end of number delimiter.
            // Else, if either the KDL is a single value XOR if there is no more data, don't assert anything since there won't always be an end of number delimiter.
            Debug.Assert(
                (
                    (_consumed < _buffer.Length)
                    && !_isNotPrimitive
                    && KdlConstants.Delimiters.IndexOf(_buffer[_consumed]) >= 0
                ) || (_isNotPrimitive ^ (_consumed >= (uint)_buffer.Length))
            );

            return true;
        }

        private bool ConsumePropertyName()
        {
            _trailingCommaBeforeComment = false;

            if (!ConsumeString())
            {
                return false;
            }

            if (!HasMoreData(ExceptionResource.ExpectedValueAfterPropertyNameNotFound))
            {
                return false;
            }

            byte first = _buffer[_consumed];

            // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
            // We do not validate if 'first' is an invalid KDL byte here (such as control characters).
            // Those cases are captured below where we only accept ':'.
            if (first <= KdlConstants.Space)
            {
                SkipWhiteSpace();
                if (!HasMoreData(ExceptionResource.ExpectedValueAfterPropertyNameNotFound))
                {
                    return false;
                }
                first = _buffer[_consumed];
            }

            // The next character must be a key / value separator. Validate and skip.
            if (first != KdlConstants.KeyValueSeparator)
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.ExpectedSeparatorAfterPropertyNameNotFound,
                    first
                );
            }

            _consumed++;
            _bytePositionInLine++;
            _tokenType = KdlTokenType.PropertyName;
            return true;
        }

        private bool ConsumeString()
        {
            Debug.Assert(_buffer.Length >= _consumed + 1);
            Debug.Assert(_buffer[_consumed] == KdlConstants.Quote);

            // Create local copy to avoid bounds checks.
            ReadOnlySpan<byte> localBuffer = _buffer[(_consumed + 1)..];

            // Vectorized search for either quote, backslash, or any control character.
            // If the first found byte is a quote, we have reached an end of string, and
            // can avoid validation.
            // Otherwise, in the uncommon case, iterate one character at a time and validate.
            int idx = localBuffer.IndexOfQuoteOrAnyControlOrBackSlash();

            if (idx >= 0)
            {
                byte foundByte = localBuffer[idx];
                if (foundByte == KdlConstants.Quote)
                {
                    _bytePositionInLine += idx + 2; // Add 2 for the start and end quotes.
                    ValueSpan = localBuffer[..idx];
                    ValueIsEscaped = false;
                    _tokenType = KdlTokenType.String;
                    _consumed += idx + 2;
                    return true;
                }
                else
                {
                    return ConsumeStringAndValidate(localBuffer, idx);
                }
            }
            else
            {
                if (IsLastSpan)
                {
                    _bytePositionInLine += localBuffer.Length + 1; // Account for the start quote
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.EndOfStringNotFound
                    );
                }
                return false;
            }
        }

        // Found a backslash or control characters which are considered invalid within a string.
        // Search through the rest of the string one byte at a time.
        // https://kdl.dev/spec/#section-7
        private bool ConsumeStringAndValidate(ReadOnlySpan<byte> data, int idx)
        {
            Debug.Assert(idx >= 0 && idx < data.Length);
            Debug.Assert(data[idx] != KdlConstants.Quote);
            Debug.Assert(data[idx] is KdlConstants.BackSlash or < KdlConstants.Space);

            long prevLineBytePosition = _bytePositionInLine;
            long prevLineNumber = _lineNumber;

            _bytePositionInLine += idx + 1; // Add 1 for the first quote

            bool nextCharEscaped = false;
            for (; idx < data.Length; idx++)
            {
                byte currentByte = data[idx];
                if (currentByte == KdlConstants.Quote)
                {
                    if (!nextCharEscaped)
                    {
                        goto Done;
                    }
                    nextCharEscaped = false;
                }
                else if (currentByte == KdlConstants.BackSlash)
                {
                    nextCharEscaped = !nextCharEscaped;
                }
                else if (nextCharEscaped)
                {
                    int index = KdlConstants.EscapableChars.IndexOf(currentByte);
                    if (index == -1)
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.InvalidCharacterAfterEscapeWithinString,
                            currentByte
                        );
                    }

                    if (currentByte == 'u')
                    {
                        // Expecting 4 hex digits to follow the escaped 'u'
                        _bytePositionInLine++; // move past the 'u'
                        if (ValidateHexDigits(data, idx + 1))
                        {
                            idx += 4; // Skip the 4 hex digits, the for loop accounts for idx incrementing past the 'u'
                        }
                        else
                        {
                            // We found less than 4 hex digits. Check if there is more data to follow, otherwise throw.
                            idx = data.Length;
                            break;
                        }
                    }
                    nextCharEscaped = false;
                }
                else if (currentByte < KdlConstants.Space)
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.InvalidCharacterWithinString,
                        currentByte
                    );
                }

                _bytePositionInLine++;
            }

            if (idx >= data.Length)
            {
                if (IsLastSpan)
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.EndOfStringNotFound
                    );
                }
                _lineNumber = prevLineNumber;
                _bytePositionInLine = prevLineBytePosition;
                return false;
            }

            Done:
            _bytePositionInLine++; // Add 1 for the end quote
            ValueSpan = data[..idx];
            ValueIsEscaped = true;
            _tokenType = KdlTokenType.String;
            _consumed += idx + 2;
            return true;
        }

        private bool ValidateHexDigits(ReadOnlySpan<byte> data, int idx)
        {
            for (int j = idx; j < data.Length; j++)
            {
                byte nextByte = data[j];
                if (!KdlReaderHelper.IsHexDigit(nextByte))
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.InvalidHexCharacterWithinString,
                        nextByte
                    );
                }
                if (j - idx >= 3)
                {
                    return true;
                }
                _bytePositionInLine++;
            }

            return false;
        }

        // https://tools.ietf.org/html/rfc7159#section-6
        private bool TryGetNumber(ReadOnlySpan<byte> data, out int consumed)
        {
            // TODO: https://github.com/dotnet/runtime/issues/27837
            Debug.Assert(data.Length > 0);

            consumed = 0;
            int i = 0;

            ConsumeNumberResult signResult = ConsumeNegativeSign(ref data, ref i);
            if (signResult == ConsumeNumberResult.NeedMoreData)
            {
                return false;
            }

            Debug.Assert(signResult == ConsumeNumberResult.OperationIncomplete);

            byte nextByte = data[i];
            Debug.Assert(nextByte is >= (byte)'0' and <= (byte)'9');

            if (nextByte == '0')
            {
                ConsumeNumberResult result = ConsumeZero(ref data, ref i);
                if (result == ConsumeNumberResult.NeedMoreData)
                {
                    return false;
                }
                if (result == ConsumeNumberResult.Success)
                {
                    goto Done;
                }

                Debug.Assert(result == ConsumeNumberResult.OperationIncomplete);
                nextByte = data[i];
            }
            else
            {
                i++;
                ConsumeNumberResult result = ConsumeIntegerDigits(ref data, ref i);
                if (result == ConsumeNumberResult.NeedMoreData)
                {
                    return false;
                }
                if (result == ConsumeNumberResult.Success)
                {
                    goto Done;
                }

                Debug.Assert(result == ConsumeNumberResult.OperationIncomplete);
                nextByte = data[i];
                if (nextByte is not (byte)'.' and not (byte)'E' and not (byte)'e')
                {
                    _bytePositionInLine += i;
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedEndOfDigitNotFound,
                        nextByte
                    );
                }
            }

            Debug.Assert(nextByte is (byte)'.' or (byte)'E' or (byte)'e');

            if (nextByte == '.')
            {
                i++;
                ConsumeNumberResult result = ConsumeDecimalDigits(ref data, ref i);
                if (result == ConsumeNumberResult.NeedMoreData)
                {
                    return false;
                }
                if (result == ConsumeNumberResult.Success)
                {
                    goto Done;
                }

                Debug.Assert(result == ConsumeNumberResult.OperationIncomplete);
                nextByte = data[i];
                if (nextByte is not (byte)'E' and not (byte)'e')
                {
                    _bytePositionInLine += i;
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedNextDigitEValueNotFound,
                        nextByte
                    );
                }
            }

            Debug.Assert(nextByte is (byte)'E' or (byte)'e');
            i++;

            signResult = ConsumeSign(ref data, ref i);
            if (signResult == ConsumeNumberResult.NeedMoreData)
            {
                return false;
            }

            Debug.Assert(signResult == ConsumeNumberResult.OperationIncomplete);

            i++;
            ConsumeNumberResult resultExponent = ConsumeIntegerDigits(ref data, ref i);
            if (resultExponent == ConsumeNumberResult.NeedMoreData)
            {
                return false;
            }
            if (resultExponent == ConsumeNumberResult.Success)
            {
                goto Done;
            }

            Debug.Assert(resultExponent == ConsumeNumberResult.OperationIncomplete);

            _bytePositionInLine += i;
            ThrowHelper.ThrowKdlReaderException(
                ref this,
                ExceptionResource.ExpectedEndOfDigitNotFound,
                data[i]
            );

            Done:
            ValueSpan = data[..i];
            consumed = i;
            return true;
        }

        private ConsumeNumberResult ConsumeNegativeSign(
            ref ReadOnlySpan<byte> data,
            scoped ref int i
        )
        {
            byte nextByte = data[i];

            if (nextByte == '-')
            {
                i++;
                if (i >= data.Length)
                {
                    if (IsLastSpan)
                    {
                        _bytePositionInLine += i;
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.RequiredDigitNotFoundEndOfData
                        );
                    }
                    return ConsumeNumberResult.NeedMoreData;
                }

                nextByte = data[i];
                if (!KdlHelpers.IsDigit(nextByte))
                {
                    _bytePositionInLine += i;
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.RequiredDigitNotFoundAfterSign,
                        nextByte
                    );
                }
            }
            return ConsumeNumberResult.OperationIncomplete;
        }

        private ConsumeNumberResult ConsumeZero(ref ReadOnlySpan<byte> data, scoped ref int i)
        {
            Debug.Assert(data[i] == (byte)'0');
            i++;
            byte nextByte;
            if (i < data.Length)
            {
                nextByte = data[i];
                if (KdlConstants.Delimiters.IndexOf(nextByte) >= 0)
                {
                    return ConsumeNumberResult.Success;
                }
            }
            else
            {
                if (IsLastSpan)
                {
                    // A payload containing a single value: "0" is valid
                    // If we are dealing with multi-value KDL,
                    // ConsumeNumber will validate that we have a delimiter following the "0".
                    return ConsumeNumberResult.Success;
                }
                else
                {
                    return ConsumeNumberResult.NeedMoreData;
                }
            }
            nextByte = data[i];
            if (nextByte is not (byte)'.' and not (byte)'E' and not (byte)'e')
            {
                _bytePositionInLine += i;
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    KdlHelpers.IsInRangeInclusive(nextByte, '0', '9')
                        ? ExceptionResource.InvalidLeadingZeroInNumber
                        : ExceptionResource.ExpectedEndOfDigitNotFound,
                    nextByte
                );
            }

            return ConsumeNumberResult.OperationIncomplete;
        }

        private readonly ConsumeNumberResult ConsumeIntegerDigits(
            ref ReadOnlySpan<byte> data,
            scoped ref int i
        )
        {
            byte nextByte = default;
            for (; i < data.Length; i++)
            {
                nextByte = data[i];
                if (!KdlHelpers.IsDigit(nextByte))
                {
                    break;
                }
            }
            if (i >= data.Length)
            {
                if (IsLastSpan)
                {
                    // A payload containing a single value of integers (e.g. "12") is valid
                    // If we are dealing with multi-value KDL,
                    // ConsumeNumber will validate that we have a delimiter following the integer.
                    return ConsumeNumberResult.Success;
                }
                else
                {
                    return ConsumeNumberResult.NeedMoreData;
                }
            }
            if (KdlConstants.Delimiters.IndexOf(nextByte) >= 0)
            {
                return ConsumeNumberResult.Success;
            }

            return ConsumeNumberResult.OperationIncomplete;
        }

        private ConsumeNumberResult ConsumeDecimalDigits(
            ref ReadOnlySpan<byte> data,
            scoped ref int i
        )
        {
            if (i >= data.Length)
            {
                if (IsLastSpan)
                {
                    _bytePositionInLine += i;
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.RequiredDigitNotFoundEndOfData
                    );
                }
                return ConsumeNumberResult.NeedMoreData;
            }
            byte nextByte = data[i];
            if (!KdlHelpers.IsDigit(nextByte))
            {
                _bytePositionInLine += i;
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.RequiredDigitNotFoundAfterDecimal,
                    nextByte
                );
            }
            i++;

            return ConsumeIntegerDigits(ref data, ref i);
        }

        private ConsumeNumberResult ConsumeSign(ref ReadOnlySpan<byte> data, scoped ref int i)
        {
            if (i >= data.Length)
            {
                if (IsLastSpan)
                {
                    _bytePositionInLine += i;
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.RequiredDigitNotFoundEndOfData
                    );
                }
                return ConsumeNumberResult.NeedMoreData;
            }

            byte nextByte = data[i];
            if (nextByte is (byte)'+' or (byte)'-')
            {
                i++;
                if (i >= data.Length)
                {
                    if (IsLastSpan)
                    {
                        _bytePositionInLine += i;
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.RequiredDigitNotFoundEndOfData
                        );
                    }
                    return ConsumeNumberResult.NeedMoreData;
                }
                nextByte = data[i];
            }

            if (!KdlHelpers.IsDigit(nextByte))
            {
                _bytePositionInLine += i;
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.RequiredDigitNotFoundAfterSign,
                    nextByte
                );
            }

            return ConsumeNumberResult.OperationIncomplete;
        }

        private bool ConsumeNextTokenOrRollback(byte marker)
        {
            int prevConsumed = _consumed;
            long prevPosition = _bytePositionInLine;
            long prevLineNumber = _lineNumber;
            KdlTokenType prevTokenType = _tokenType;
            bool prevTrailingCommaBeforeComment = _trailingCommaBeforeComment;
            ConsumeTokenResult result = ConsumeNextToken(marker);
            if (result == ConsumeTokenResult.Success)
            {
                return true;
            }
            if (result == ConsumeTokenResult.NotEnoughDataRollBackState)
            {
                _consumed = prevConsumed;
                _tokenType = prevTokenType;
                _bytePositionInLine = prevPosition;
                _lineNumber = prevLineNumber;
                _trailingCommaBeforeComment = prevTrailingCommaBeforeComment;
            }
            return false;
        }

        /// <summary>
        /// This method consumes the next token regardless of whether we are inside an object or an array.
        /// For an object, it reads the next property name token. For an array, it just reads the next value.
        /// </summary>
        private ConsumeTokenResult ConsumeNextToken(byte marker)
        {
            if (_readerOptions.CommentHandling != KdlCommentHandling.Disallow)
            {
                if (_readerOptions.CommentHandling == KdlCommentHandling.Allow)
                {
                    if (marker == KdlConstants.Slash)
                    {
                        return ConsumeComment()
                            ? ConsumeTokenResult.Success
                            : ConsumeTokenResult.NotEnoughDataRollBackState;
                    }
                    if (_tokenType == KdlTokenType.Comment)
                    {
                        return ConsumeNextTokenFromLastNonCommentToken();
                    }
                }
                else
                {
                    Debug.Assert(_readerOptions.CommentHandling == KdlCommentHandling.Skip);
                    return ConsumeNextTokenUntilAfterAllCommentsAreSkipped(marker);
                }
            }

            if (_nodeStack.CurrentDepth == 0)
            {
                return ReadFirstToken(marker)
                    ? ConsumeTokenResult.Success
                    : ConsumeTokenResult.NotEnoughDataRollBackState;
            }

            if (marker == KdlConstants.ListSeparator)
            {
                _consumed++;
                _bytePositionInLine++;

                if (_consumed >= (uint)_buffer.Length)
                {
                    if (IsLastSpan)
                    {
                        _consumed--;
                        _bytePositionInLine--;
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyOrValueNotFound
                        );
                    }
                    return ConsumeTokenResult.NotEnoughDataRollBackState;
                }
                byte first = _buffer[_consumed];

                // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
                if (first <= KdlConstants.Space)
                {
                    SkipWhiteSpace();
                    // The next character must be a start of a property name or value.
                    if (!HasMoreData(ExceptionResource.ExpectedStartOfPropertyOrValueNotFound))
                    {
                        return ConsumeTokenResult.NotEnoughDataRollBackState;
                    }
                    first = _buffer[_consumed];
                }

                TokenStartIndex = _consumed;

                if (
                    _readerOptions.CommentHandling == KdlCommentHandling.Allow
                    && first == KdlConstants.Slash
                )
                {
                    _trailingCommaBeforeComment = true;
                    return ConsumeComment()
                        ? ConsumeTokenResult.Success
                        : ConsumeTokenResult.NotEnoughDataRollBackState;
                }

                if (_inNode)
                {
                    if (first != KdlConstants.Quote)
                    {
                        if (first == KdlConstants.CloseBrace)
                        {
                            if (_readerOptions.AllowTrailingCommas)
                            {
                                EndChildrenBlock();
                                return ConsumeTokenResult.Success;
                            }
                            ThrowHelper.ThrowKdlReaderException(
                                ref this,
                                ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd
                            );
                        }
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyNotFound,
                            first
                        );
                    }
                    return ConsumePropertyName()
                        ? ConsumeTokenResult.Success
                        : ConsumeTokenResult.NotEnoughDataRollBackState;
                }
                else
                {
                    if (first == KdlConstants.CloseBrace)
                    {
                        if (_readerOptions.AllowTrailingCommas)
                        {
                            EndChildrenBlock();
                            return ConsumeTokenResult.Success;
                        }
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.TrailingCommaNotAllowedBeforeArrayEnd
                        );
                    }
                    return ConsumeValue(first)
                        ? ConsumeTokenResult.Success
                        : ConsumeTokenResult.NotEnoughDataRollBackState;
                }
            }
            else if (marker == KdlConstants.CloseBrace)
            {
                EndChildrenBlock();
            }
            else
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.FoundInvalidCharacter,
                    marker
                );
            }
            return ConsumeTokenResult.Success;
        }

        private ConsumeTokenResult ConsumeNextTokenFromLastNonCommentToken()
        {
            Debug.Assert(_readerOptions.CommentHandling == KdlCommentHandling.Allow);
            Debug.Assert(_tokenType == KdlTokenType.Comment);

            if (KdlReaderHelper.IsTokenTypePrimitive(_previousTokenType))
            {
                _tokenType = _inNode
                    ? KdlTokenType.StartChildrenBlock
                    : KdlTokenType.StartChildrenBlock;
            }
            else
            {
                _tokenType = _previousTokenType;
            }

            Debug.Assert(_tokenType != KdlTokenType.Comment);

            if (!HasMoreData())
            {
                goto RollBack;
            }

            byte first = _buffer[_consumed];

            // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
            if (first <= KdlConstants.Space)
            {
                SkipWhiteSpace();
                if (!HasMoreData())
                {
                    goto RollBack;
                }
                first = _buffer[_consumed];
            }

            if (_nodeStack.CurrentDepth == 0 && _tokenType != KdlTokenType.None)
            {
                return ReadFirstToken(first)
                    ? ConsumeTokenResult.Success
                    : ConsumeTokenResult.NotEnoughDataRollBackState;
            }

            Debug.Assert(first != KdlConstants.Slash);

            TokenStartIndex = _consumed;

            if (first == KdlConstants.ListSeparator)
            {
                // A comma without some KDL value preceding it is invalid
                if (
                    _previousTokenType <= KdlTokenType.StartChildrenBlock
                    || _previousTokenType == KdlTokenType.StartChildrenBlock
                    || _trailingCommaBeforeComment
                )
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedStartOfPropertyOrValueAfterComment,
                        first
                    );
                }

                _consumed++;
                _bytePositionInLine++;

                if (_consumed >= (uint)_buffer.Length)
                {
                    if (IsLastSpan)
                    {
                        _consumed--;
                        _bytePositionInLine--;
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyOrValueNotFound
                        );
                    }
                    goto RollBack;
                }
                first = _buffer[_consumed];

                // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
                if (first <= KdlConstants.Space)
                {
                    SkipWhiteSpace();
                    // The next character must be a start of a property name or value.
                    if (!HasMoreData(ExceptionResource.ExpectedStartOfPropertyOrValueNotFound))
                    {
                        goto RollBack;
                    }
                    first = _buffer[_consumed];
                }

                TokenStartIndex = _consumed;

                if (first == KdlConstants.Slash)
                {
                    _trailingCommaBeforeComment = true;
                    if (ConsumeComment())
                    {
                        goto Done;
                    }
                    else
                    {
                        goto RollBack;
                    }
                }

                if (_inNode)
                {
                    if (first != KdlConstants.Quote)
                    {
                        if (first == KdlConstants.CloseBrace)
                        {
                            if (_readerOptions.AllowTrailingCommas)
                            {
                                EndChildrenBlock();
                                goto Done;
                            }
                            ThrowHelper.ThrowKdlReaderException(
                                ref this,
                                ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd
                            );
                        }

                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyNotFound,
                            first
                        );
                    }
                    if (ConsumePropertyName())
                    {
                        goto Done;
                    }
                    else
                    {
                        goto RollBack;
                    }
                }
                else
                {
                    if (first == KdlConstants.CloseBrace)
                    {
                        if (_readerOptions.AllowTrailingCommas)
                        {
                            EndChildrenBlock();
                            goto Done;
                        }
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.TrailingCommaNotAllowedBeforeArrayEnd
                        );
                    }

                    if (ConsumeValue(first))
                    {
                        goto Done;
                    }
                    else
                    {
                        goto RollBack;
                    }
                }
            }
            else if (first == KdlConstants.CloseBrace)
            {
                EndChildrenBlock();
            }
            else if (_tokenType == KdlTokenType.None)
            {
                if (ReadFirstToken(first))
                {
                    goto Done;
                }
                else
                {
                    goto RollBack;
                }
            }
            else if (_tokenType == KdlTokenType.StartChildrenBlock)
            {
                Debug.Assert(first != KdlConstants.CloseBrace);
                if (first != KdlConstants.Quote)
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedStartOfPropertyNotFound,
                        first
                    );
                }

                int prevConsumed = _consumed;
                long prevPosition = _bytePositionInLine;
                long prevLineNumber = _lineNumber;
                if (!ConsumePropertyName())
                {
                    // roll back potential changes
                    _consumed = prevConsumed;
                    _tokenType = KdlTokenType.StartChildrenBlock;
                    _bytePositionInLine = prevPosition;
                    _lineNumber = prevLineNumber;
                    goto RollBack;
                }
                goto Done;
            }
            else if (_tokenType == KdlTokenType.StartChildrenBlock)
            {
                Debug.Assert(first != KdlConstants.CloseBrace);
                if (!ConsumeValue(first))
                {
                    goto RollBack;
                }
                goto Done;
            }
            else if (_tokenType == KdlTokenType.PropertyName)
            {
                if (!ConsumeValue(first))
                {
                    goto RollBack;
                }
                goto Done;
            }
            else
            {
                Debug.Assert(
                    _tokenType is KdlTokenType.EndChildrenBlock or KdlTokenType.EndChildrenBlock
                );
                if (_inNode)
                {
                    Debug.Assert(first != KdlConstants.CloseBrace);
                    if (first != KdlConstants.Quote)
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyNotFound,
                            first
                        );
                    }

                    if (ConsumePropertyName())
                    {
                        goto Done;
                    }
                    else
                    {
                        goto RollBack;
                    }
                }
                else
                {
                    Debug.Assert(first != KdlConstants.CloseBrace);

                    if (ConsumeValue(first))
                    {
                        goto Done;
                    }
                    else
                    {
                        goto RollBack;
                    }
                }
            }

            Done:
            return ConsumeTokenResult.Success;

            RollBack:
            return ConsumeTokenResult.NotEnoughDataRollBackState;
        }

        private bool SkipAllComments(scoped ref byte marker)
        {
            while (marker == KdlConstants.Slash)
            {
                if (SkipComment())
                {
                    if (!HasMoreData())
                    {
                        goto IncompleteNoRollback;
                    }

                    marker = _buffer[_consumed];

                    // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
                    if (marker <= KdlConstants.Space)
                    {
                        SkipWhiteSpace();
                        if (!HasMoreData())
                        {
                            goto IncompleteNoRollback;
                        }
                        marker = _buffer[_consumed];
                    }
                }
                else
                {
                    goto IncompleteNoRollback;
                }
            }
            return true;

            IncompleteNoRollback:
            return false;
        }

        private bool SkipAllComments(scoped ref byte marker, ExceptionResource resource)
        {
            while (marker == KdlConstants.Slash)
            {
                if (SkipComment())
                {
                    // The next character must be a start of a property name or value.
                    if (!HasMoreData(resource))
                    {
                        goto IncompleteRollback;
                    }

                    marker = _buffer[_consumed];

                    // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
                    if (marker <= KdlConstants.Space)
                    {
                        SkipWhiteSpace();
                        // The next character must be a start of a property name or value.
                        if (!HasMoreData(resource))
                        {
                            goto IncompleteRollback;
                        }
                        marker = _buffer[_consumed];
                    }
                }
                else
                {
                    goto IncompleteRollback;
                }
            }
            return true;

            IncompleteRollback:
            return false;
        }

        private ConsumeTokenResult ConsumeNextTokenUntilAfterAllCommentsAreSkipped(byte marker)
        {
            if (!SkipAllComments(ref marker))
            {
                goto IncompleteNoRollback;
            }

            TokenStartIndex = _consumed;

            if (_tokenType == KdlTokenType.StartChildrenBlock)
            {
                if (marker == KdlConstants.CloseBrace)
                {
                    EndChildrenBlock();
                }
                else
                {
                    if (marker != KdlConstants.Quote)
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyNotFound,
                            marker
                        );
                    }

                    int prevConsumed = _consumed;
                    long prevPosition = _bytePositionInLine;
                    long prevLineNumber = _lineNumber;
                    if (!ConsumePropertyName())
                    {
                        // roll back potential changes
                        _consumed = prevConsumed;
                        _tokenType = KdlTokenType.StartChildrenBlock;
                        _bytePositionInLine = prevPosition;
                        _lineNumber = prevLineNumber;
                        goto IncompleteNoRollback;
                    }
                    goto Done;
                }
            }
            else if (_tokenType == KdlTokenType.StartChildrenBlock)
            {
                if (marker == KdlConstants.CloseBrace)
                {
                    EndChildrenBlock();
                }
                else
                {
                    if (!ConsumeValue(marker))
                    {
                        goto IncompleteNoRollback;
                    }
                    goto Done;
                }
            }
            else if (_tokenType == KdlTokenType.PropertyName)
            {
                if (!ConsumeValue(marker))
                {
                    goto IncompleteNoRollback;
                }
                goto Done;
            }
            else if (_nodeStack.CurrentDepth == 0)
            {
                return ReadFirstToken(marker)
                    ? ConsumeTokenResult.Success
                    : ConsumeTokenResult.NotEnoughDataRollBackState;
            }
            else if (marker == KdlConstants.ListSeparator)
            {
                _consumed++;
                _bytePositionInLine++;

                if (_consumed >= (uint)_buffer.Length)
                {
                    if (IsLastSpan)
                    {
                        _consumed--;
                        _bytePositionInLine--;
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyOrValueNotFound
                        );
                    }
                    return ConsumeTokenResult.NotEnoughDataRollBackState;
                }
                marker = _buffer[_consumed];

                // This check is done as an optimization to avoid calling SkipWhiteSpace when not necessary.
                if (marker <= KdlConstants.Space)
                {
                    SkipWhiteSpace();
                    // The next character must be a start of a property name or value.
                    if (!HasMoreData(ExceptionResource.ExpectedStartOfPropertyOrValueNotFound))
                    {
                        return ConsumeTokenResult.NotEnoughDataRollBackState;
                    }
                    marker = _buffer[_consumed];
                }

                if (
                    !SkipAllComments(
                        ref marker,
                        ExceptionResource.ExpectedStartOfPropertyOrValueNotFound
                    )
                )
                {
                    goto IncompleteRollback;
                }

                TokenStartIndex = _consumed;

                if (_inNode)
                {
                    if (marker != KdlConstants.Quote)
                    {
                        if (marker == KdlConstants.CloseBrace)
                        {
                            if (_readerOptions.AllowTrailingCommas)
                            {
                                EndChildrenBlock();
                                goto Done;
                            }
                            ThrowHelper.ThrowKdlReaderException(
                                ref this,
                                ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd
                            );
                        }

                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.ExpectedStartOfPropertyNotFound,
                            marker
                        );
                    }
                    return ConsumePropertyName()
                        ? ConsumeTokenResult.Success
                        : ConsumeTokenResult.NotEnoughDataRollBackState;
                }
                else
                {
                    if (marker == KdlConstants.CloseBrace)
                    {
                        if (_readerOptions.AllowTrailingCommas)
                        {
                            EndChildrenBlock();
                            goto Done;
                        }
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.TrailingCommaNotAllowedBeforeArrayEnd
                        );
                    }

                    return ConsumeValue(marker)
                        ? ConsumeTokenResult.Success
                        : ConsumeTokenResult.NotEnoughDataRollBackState;
                }
            }
            else if (marker == KdlConstants.CloseBrace)
            {
                EndChildrenBlock();
            }
            else
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.FoundInvalidCharacter,
                    marker
                );
            }

            Done:
            return ConsumeTokenResult.Success;
            IncompleteNoRollback:
            return ConsumeTokenResult.IncompleteNoRollBackNecessary;
            IncompleteRollback:
            return ConsumeTokenResult.NotEnoughDataRollBackState;
        }

        private bool SkipComment()
        {
            // Create local copy to avoid bounds checks.
            ReadOnlySpan<byte> localBuffer = _buffer[(_consumed + 1)..];

            if (localBuffer.Length > 0)
            {
                byte marker = localBuffer[0];
                if (marker == KdlConstants.Slash)
                {
                    return SkipSingleLineComment(localBuffer[1..], out _);
                }
                else if (marker == KdlConstants.Asterisk)
                {
                    return SkipMultiLineComment(localBuffer[1..], out _);
                }
                else
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.ExpectedStartOfValueNotFound,
                        KdlConstants.Slash
                    );
                }
            }

            if (IsLastSpan)
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.ExpectedStartOfValueNotFound,
                    KdlConstants.Slash
                );
            }
            return false;
        }

        private bool SkipSingleLineComment(ReadOnlySpan<byte> localBuffer, out int idx)
        {
            idx = FindLineSeparator(localBuffer);
            int toConsume;
            if (idx != -1)
            {
                toConsume = idx;
                if (localBuffer[idx] == KdlConstants.LineFeed)
                {
                    goto EndOfComment;
                }

                // If we are here, we have definintely found a \r. So now to check if \n follows.
                Debug.Assert(localBuffer[idx] == KdlConstants.CarriageReturn);

                if (idx < localBuffer.Length - 1)
                {
                    if (localBuffer[idx + 1] == KdlConstants.LineFeed)
                    {
                        toConsume++;
                    }

                    goto EndOfComment;
                }

                if (IsLastSpan)
                {
                    goto EndOfComment;
                }
                else
                {
                    // there might be LF in the next segment
                    return false;
                }
            }

            if (IsLastSpan)
            {
                idx = localBuffer.Length;
                toConsume = idx;
                // Assume everything on this line is a comment and there is no more data.
                _bytePositionInLine += 2 + localBuffer.Length;
                goto Done;
            }
            else
            {
                return false;
            }

            EndOfComment:
            toConsume++;
            _bytePositionInLine = 0;
            _lineNumber++;

            Done:
            _consumed += 2 + toConsume;
            return true;
        }

        private int FindLineSeparator(ReadOnlySpan<byte> localBuffer)
        {
            int totalIdx = 0;
            while (true)
            {
                int idx = localBuffer.IndexOfAny(
                    KdlConstants.LineFeed,
                    KdlConstants.CarriageReturn,
                    KdlConstants.StartingByteOfNonStandardSeparator
                );

                if (idx == -1)
                {
                    return -1;
                }

                totalIdx += idx;

                if (localBuffer[idx] != KdlConstants.StartingByteOfNonStandardSeparator)
                {
                    return totalIdx;
                }

                totalIdx++;
                localBuffer = localBuffer[(idx + 1)..];

                ThrowOnDangerousLineSeparator(localBuffer);
            }
        }

        // assumes first byte (KdlConstants.StartingByteOfNonStandardSeparator) is already read
        private void ThrowOnDangerousLineSeparator(ReadOnlySpan<byte> localBuffer)
        {
            // \u2028 and \u2029 are considered respectively line and paragraph separators
            // UTF-8 representation for them is E2, 80, A8/A9
            // we have already read E2, we need to check for remaining 2 bytes

            if (localBuffer.Length < 2)
            {
                return;
            }

            byte next = localBuffer[1];
            if (localBuffer[0] == 0x80 && (next == 0xA8 || next == 0xA9))
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.UnexpectedEndOfLineSeparator
                );
            }
        }

        private bool SkipMultiLineComment(ReadOnlySpan<byte> localBuffer, out int idx)
        {
            idx = 0;
            while (true)
            {
                int foundIdx = localBuffer[idx..].IndexOf(KdlConstants.Slash);
                if (foundIdx == -1)
                {
                    if (IsLastSpan)
                    {
                        ThrowHelper.ThrowKdlReaderException(
                            ref this,
                            ExceptionResource.EndOfCommentNotFound
                        );
                    }
                    return false;
                }
                if (foundIdx != 0 && localBuffer[foundIdx + idx - 1] == KdlConstants.Asterisk)
                {
                    // foundIdx points just after '*' in the end-of-comment delimiter. Hence increment idx by one
                    // position less to make it point right before beginning of end-of-comment delimiter i.e. */
                    idx += foundIdx - 1;
                    break;
                }
                idx += foundIdx + 1;
            }

            // Consume the /* and */ characters that are part of the multi-line comment.
            // idx points right before the final '*' (which is right before the last '/'). Hence increment _consumed
            // by 4 to exclude the start/end-of-comment delimiters.
            _consumed += 4 + idx;

            (int newLines, int newLineIndex) = KdlReaderHelper.CountNewLines(localBuffer[..idx]);
            _lineNumber += newLines;
            if (newLineIndex != -1)
            {
                // newLineIndex points at last newline character and byte positions in the new line start
                // after that. Hence add 1 to skip the newline character.
                _bytePositionInLine = idx - newLineIndex + 1;
            }
            else
            {
                _bytePositionInLine += 4 + idx;
            }
            return true;
        }

        private bool ConsumeComment()
        {
            // Create local copy to avoid bounds checks.
            ReadOnlySpan<byte> localBuffer = _buffer[(_consumed + 1)..];

            if (localBuffer.Length > 0)
            {
                byte marker = localBuffer[0];
                if (marker == KdlConstants.Slash)
                {
                    return ConsumeSingleLineComment(localBuffer[1..], _consumed);
                }
                else if (marker == KdlConstants.Asterisk)
                {
                    return ConsumeMultiLineComment(localBuffer[1..], _consumed);
                }
                else
                {
                    ThrowHelper.ThrowKdlReaderException(
                        ref this,
                        ExceptionResource.InvalidCharacterAtStartOfComment,
                        marker
                    );
                }
            }

            if (IsLastSpan)
            {
                ThrowHelper.ThrowKdlReaderException(
                    ref this,
                    ExceptionResource.UnexpectedEndOfDataWhileReadingComment
                );
            }
            return false;
        }

        private bool ConsumeSingleLineComment(ReadOnlySpan<byte> localBuffer, int previousConsumed)
        {
            if (!SkipSingleLineComment(localBuffer, out int idx))
            {
                return false;
            }

            // Exclude the // at start of the comment. idx points right before the line separator
            // at the end of the comment.
            ValueSpan = _buffer.Slice(previousConsumed + 2, idx);
            if (_tokenType != KdlTokenType.Comment)
            {
                _previousTokenType = _tokenType;
            }
            _tokenType = KdlTokenType.Comment;
            return true;
        }

        private bool ConsumeMultiLineComment(ReadOnlySpan<byte> localBuffer, int previousConsumed)
        {
            if (!SkipMultiLineComment(localBuffer, out int idx))
            {
                return false;
            }

            // Exclude the /* at start of the comment. idx already points right before the terminal '*/'
            // for the end of multiline comment.
            ValueSpan = _buffer.Slice(previousConsumed + 2, idx);
            if (_tokenType != KdlTokenType.Comment)
            {
                _previousTokenType = _tokenType;
            }
            _tokenType = KdlTokenType.Comment;
            return true;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string DebuggerDisplay =>
            $"TokenType = {DebugTokenType}, TokenStartIndex = {TokenStartIndex}, Consumed = {BytesConsumed}";

        // Using TokenType.ToString() (or {TokenType}) fails to render in the debug window. The
        // message "The runtime refused to evaluate the expression at this time." is shown. This
        // is a workaround until we root cause and fix the issue.
        private readonly string DebugTokenType =>
            TokenType switch
            {
                KdlTokenType.Comment => nameof(KdlTokenType.Comment),
                KdlTokenType.StartTypeAnnotation => nameof(KdlTokenType.StartTypeAnnotation),
                KdlTokenType.EndTypeAnnotation => nameof(KdlTokenType.EndTypeAnnotation),
                KdlTokenType.EndChildrenBlock => nameof(KdlTokenType.EndChildrenBlock),
                KdlTokenType.False => nameof(KdlTokenType.False),
                KdlTokenType.None => nameof(KdlTokenType.None),
                KdlTokenType.Null => nameof(KdlTokenType.Null),
                KdlTokenType.Number => nameof(KdlTokenType.Number),
                KdlTokenType.PropertyName => nameof(KdlTokenType.PropertyName),
                KdlTokenType.StartChildrenBlock => nameof(KdlTokenType.StartChildrenBlock),
                KdlTokenType.String => nameof(KdlTokenType.String),
                KdlTokenType.True => nameof(KdlTokenType.True),
                _ => ((byte)TokenType).ToString(),
            };

        private readonly ReadOnlySpan<byte> GetUnescapedSpan()
        {
            ReadOnlySpan<byte> span = HasValueSequence ? ValueSequence.ToArray() : ValueSpan;
            if (ValueIsEscaped)
            {
                span = KdlReaderHelper.GetUnescapedSpan(span);
            }

            return span;
        }
    }
}
