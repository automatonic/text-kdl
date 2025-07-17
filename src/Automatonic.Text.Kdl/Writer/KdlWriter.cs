using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Provides a high-performance API for forward-only, non-cached writing of UTF-8 encoded KDL text.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     It writes the text sequentially with no caching and adheres to the KDL RFC
    ///     by default (https://tools.ietf.org/html/rfc8259), with the exception of writing comments.
    ///   </para>
    ///   <para>
    ///     When the user attempts to write invalid KDL and validation is enabled, it throws
    ///     an <see cref="InvalidOperationException"/> with a context specific error message.
    ///   </para>
    ///   <para>
    ///     To be able to format the output with indentation and whitespace OR to skip validation, create an instance of
    ///     <see cref="KdlWriterOptions"/> and pass that in to the writer.
    ///   </para>
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed partial class KdlWriter : IDisposable, IAsyncDisposable
    {
        private const int DefaultGrowthSize = 4096;
        private const int InitialGrowthSize = 256;

        private IBufferWriter<byte>? _output;
        private Stream? _stream;
        private ArrayBufferWriter<byte>? _arrayBufferWriter;

        private Memory<byte> _memory;

        private bool _inObject;
        private bool _commentAfterNoneOrPropertyName;
        private KdlTokenType _tokenType;
        private BitStack _bitStack;

        // The highest order bit of _currentDepth is used to discern whether we are writing the first item in a list or not.
        // if (_currentDepth >> 31) == 1, add a list separator before writing the item
        // else, no list separator is needed since we are writing the first item.
        private int _currentDepth;

        private KdlWriterOptions _options; // Since KdlWriterOptions is a struct, use a field to avoid a copy for internal code.

        // Cache indentation settings from KdlWriterOptions to avoid recomputing them in the hot path.
        private byte _indentByte;
        private int _indentLength;

        // A length of 1 will emit LF for indented writes, a length of 2 will emit CRLF. Other values are invalid.
        private int _newLineLength;

        /// <summary>
        /// Returns the amount of bytes written by the <see cref="KdlWriter"/> so far
        /// that have not yet been flushed to the output and committed.
        /// </summary>
        public int BytesPending { get; private set; }

        /// <summary>
        /// Returns the amount of bytes committed to the output by the <see cref="KdlWriter"/> so far.
        /// </summary>
        /// <remarks>
        /// In the case of IBufferwriter, this is how much the IBufferWriter has advanced.
        /// In the case of Stream, this is how much data has been written to the stream.
        /// </remarks>
        public long BytesCommitted { get; private set; }

        /// <summary>
        /// Gets the custom behavior when writing KDL using
        /// the <see cref="KdlWriter"/> which indicates whether to format the output
        /// while writing and whether to skip structural KDL validation or not.
        /// </summary>
        public KdlWriterOptions Options => _options;

        private int Indentation => CurrentDepth * _indentLength;

        internal KdlTokenType TokenType => _tokenType;

        /// <summary>
        /// Tracks the recursive depth of the nested objects / arrays within the KDL text
        /// written so far. This provides the depth of the current token.
        /// </summary>
        public int CurrentDepth => _currentDepth & KdlConstants.RemoveFlagsBitMask;

        private KdlWriter() { }

        /// <summary>
        /// Constructs a new <see cref="KdlWriter"/> instance with a specified <paramref name="bufferWriter"/>.
        /// </summary>
        /// <param name="bufferWriter">An instance of <see cref="IBufferWriter{Byte}" /> used as a destination for writing KDL text into.</param>
        /// <param name="options">Defines the customized behavior of the <see cref="KdlWriter"/>
        /// By default, the <see cref="KdlWriter"/> writes KDL minimized (that is, with no extra whitespace)
        /// and validates that the KDL being written is structurally valid according to KDL RFC.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the instance of <see cref="IBufferWriter{Byte}" /> that is passed in is null.
        /// </exception>
        public KdlWriter(IBufferWriter<byte> bufferWriter, KdlWriterOptions options = default)
        {
            if (bufferWriter is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(bufferWriter));
            }

            _output = bufferWriter;
            SetOptions(options);
        }

        /// <summary>
        /// Constructs a new <see cref="KdlWriter"/> instance with a specified <paramref name="utf8Kdl"/>.
        /// </summary>
        /// <param name="utf8Kdl">An instance of <see cref="Stream" /> used as a destination for writing KDL text into.</param>
        /// <param name="options">Defines the customized behavior of the <see cref="KdlWriter"/>
        /// By default, the <see cref="KdlWriter"/> writes KDL minimized (that is, with no extra whitespace)
        /// and validates that the KDL being written is structurally valid according to KDL RFC.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the instance of <see cref="Stream" /> that is passed in is null.
        /// </exception>
        public KdlWriter(Stream utf8Kdl, KdlWriterOptions options = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            if (!utf8Kdl.CanWrite)
            {
                throw new ArgumentException(SR.StreamNotWritable);
            }

            _stream = utf8Kdl;
            SetOptions(options);

            _arrayBufferWriter = new ArrayBufferWriter<byte>();
        }

        private void SetOptions(KdlWriterOptions options)
        {
            _options = options;
            _indentByte = (byte)_options.IndentCharacter;
            _indentLength = options.IndentSize;

            Debug.Assert(options.NewLine is "\n" or "\r\n", "Invalid NewLine string.");
            _newLineLength = options.NewLine.Length;

            if (_options.MaxDepth == 0)
            {
                _options.MaxDepth = KdlWriterOptions.DefaultMaxDepth; // If max depth is not set, revert to the default depth.
            }
        }

        /// <summary>
        /// Resets the <see cref="KdlWriter"/> internal state so that it can be re-used.
        /// </summary>
        /// <remarks>
        /// The <see cref="KdlWriter"/> will continue to use the original writer options
        /// and the original output as the destination (either <see cref="IBufferWriter{Byte}" /> or <see cref="Stream" />).
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        ///   The instance of <see cref="KdlWriter"/> has been disposed.
        /// </exception>
        public void Reset()
        {
            CheckNotDisposed();

            _arrayBufferWriter?.Clear();
            ResetHelper();
        }

        /// <summary>
        /// Resets the <see cref="KdlWriter"/> internal state so that it can be re-used with the new instance of <see cref="Stream" />.
        /// </summary>
        /// <param name="utf8Kdl">An instance of <see cref="Stream" /> used as a destination for writing KDL text into.</param>
        /// <remarks>
        /// The <see cref="KdlWriter"/> will continue to use the original writer options
        /// but now write to the passed in <see cref="Stream" /> as the new destination.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the instance of <see cref="Stream" /> that is passed in is null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The instance of <see cref="KdlWriter"/> has been disposed.
        /// </exception>
        public void Reset(Stream utf8Kdl)
        {
            CheckNotDisposed();

            ArgumentNullException.ThrowIfNull(utf8Kdl, nameof(utf8Kdl));

            if (!utf8Kdl.CanWrite)
            {
                throw new ArgumentException(SR.StreamNotWritable);
            }

            _stream = utf8Kdl;
            if (_arrayBufferWriter == null)
            {
                _arrayBufferWriter = new ArrayBufferWriter<byte>();
            }
            else
            {
                _arrayBufferWriter.Clear();
            }
            _output = null;

            ResetHelper();
        }

        /// <summary>
        /// Resets the <see cref="KdlWriter"/> internal state so that it can be re-used with the new instance of <see cref="IBufferWriter{Byte}" />.
        /// </summary>
        /// <param name="bufferWriter">An instance of <see cref="IBufferWriter{Byte}" /> used as a destination for writing KDL text into.</param>
        /// <remarks>
        /// The <see cref="KdlWriter"/> will continue to use the original writer options
        /// but now write to the passed in <see cref="IBufferWriter{Byte}" /> as the new destination.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the instance of <see cref="IBufferWriter{Byte}" /> that is passed in is null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The instance of <see cref="KdlWriter"/> has been disposed.
        /// </exception>
        public void Reset(IBufferWriter<byte> bufferWriter)
        {
            CheckNotDisposed();

            _output = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));
            _stream = null;
            _arrayBufferWriter = null;

            ResetHelper();
        }

        internal void ResetAllStateForCacheReuse()
        {
            ResetHelper();

            _stream = null;
            _arrayBufferWriter = null;
            _output = null;
        }

        internal void Reset(IBufferWriter<byte> bufferWriter, KdlWriterOptions options)
        {
            Debug.Assert(_output is null && _stream is null && _arrayBufferWriter is null);

            _output = bufferWriter;
            SetOptions(options);
        }

        internal static KdlWriter CreateEmptyInstanceForCaching() => new();

        private void ResetHelper()
        {
            BytesPending = default;
            BytesCommitted = default;
            _memory = default;

            _inObject = default;
            _tokenType = default;
            _commentAfterNoneOrPropertyName = default;
            _currentDepth = default;

            _bitStack = default;
        }

        private void CheckNotDisposed()
        {
            if (_stream == null)
            {
                // The conditions are ordered with stream first as that would be the most common mode
                if (_output == null)
                {
                    ThrowHelper.ThrowObjectDisposedException_KdlWriter();
                }
            }
        }

        /// <summary>
        /// Commits the KDL text written so far which makes it visible to the output destination.
        /// </summary>
        /// <remarks>
        /// In the case of IBufferWriter, this advances the underlying <see cref="IBufferWriter{Byte}" /> based on what has been written so far.
        /// In the case of Stream, this writes the data to the stream and flushes it.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        ///   The instance of <see cref="KdlWriter"/> has been disposed.
        /// </exception>
        public void Flush()
        {
            CheckNotDisposed();

            _memory = default;

            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);
                if (BytesPending != 0)
                {
                    _arrayBufferWriter.Advance(BytesPending);
                    BytesPending = 0;

#if NET
                    _stream.Write(_arrayBufferWriter.WrittenSpan);
#else
                    Debug.Assert(
                        _arrayBufferWriter.WrittenMemory.Length == _arrayBufferWriter.WrittenCount
                    );
                    bool result = MemoryMarshal.TryGetArray(
                        _arrayBufferWriter.WrittenMemory,
                        out ArraySegment<byte> underlyingBuffer
                    );
                    Debug.Assert(result);
                    Debug.Assert(underlyingBuffer.Offset == 0);
                    Debug.Assert(_arrayBufferWriter.WrittenCount == underlyingBuffer.Count);
                    _stream.Write(
                        underlyingBuffer.Array,
                        underlyingBuffer.Offset,
                        underlyingBuffer.Count
                    );
#endif

                    BytesCommitted += _arrayBufferWriter.WrittenCount;
                    _arrayBufferWriter.Clear();
                }
                _stream.Flush();
            }
            else
            {
                Debug.Assert(_output != null);
                if (BytesPending != 0)
                {
                    _output.Advance(BytesPending);
                    BytesCommitted += BytesPending;
                    BytesPending = 0;
                }
            }
        }

        /// <summary>
        /// Commits any left over KDL text that has not yet been flushed and releases all resources used by the current instance.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     In the case of IBufferWriter, this advances the underlying <see cref="IBufferWriter{Byte}" /> based on what has been written so far.
        ///     In the case of Stream, this writes the data to the stream and flushes it.
        ///   </para>
        ///   <para>
        ///     The <see cref="KdlWriter"/> instance cannot be re-used after disposing.
        ///   </para>
        /// </remarks>
        public void Dispose()
        {
            if (_stream == null)
            {
                // The conditions are ordered with stream first as that would be the most common mode
                if (_output == null)
                {
                    return;
                }
            }

            Flush();
            ResetHelper();

            _stream = null;
            _arrayBufferWriter = null;
            _output = null;
        }

        /// <summary>
        /// Asynchronously commits any left over KDL text that has not yet been flushed and releases all resources used by the current instance.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     In the case of IBufferWriter, this advances the underlying <see cref="IBufferWriter{Byte}" /> based on what has been written so far.
        ///     In the case of Stream, this writes the data to the stream and flushes it.
        ///   </para>
        ///   <para>
        ///     The <see cref="KdlWriter"/> instance cannot be re-used after disposing.
        ///   </para>
        /// </remarks>
        public async ValueTask DisposeAsync()
        {
            if (_stream == null)
            {
                // The conditions are ordered with stream first as that would be the most common mode
                if (_output == null)
                {
                    return;
                }
            }

            await FlushAsync().ConfigureAwait(false);
            ResetHelper();

            _stream = null;
            _arrayBufferWriter = null;
            _output = null;
        }

        /// <summary>
        /// Asynchronously commits the KDL text written so far which makes it visible to the output destination.
        /// </summary>
        /// <remarks>
        /// In the case of IBufferWriter, this advances the underlying <see cref="IBufferWriter{Byte}" /> based on what has been written so far.
        /// In the case of Stream, this writes the data to the stream and flushes it asynchronously, while monitoring cancellation requests.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        ///   The instance of <see cref="KdlWriter"/> has been disposed.
        /// </exception>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            CheckNotDisposed();

            _memory = default;

            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);
                if (BytesPending != 0)
                {
                    _arrayBufferWriter.Advance(BytesPending);
                    BytesPending = 0;

#if NET
                    await _stream
                        .WriteAsync(_arrayBufferWriter.WrittenMemory, cancellationToken)
                        .ConfigureAwait(false);
#else
                    Debug.Assert(
                        _arrayBufferWriter.WrittenMemory.Length == _arrayBufferWriter.WrittenCount
                    );
                    bool result = MemoryMarshal.TryGetArray(
                        _arrayBufferWriter.WrittenMemory,
                        out ArraySegment<byte> underlyingBuffer
                    );
                    Debug.Assert(result);
                    Debug.Assert(underlyingBuffer.Offset == 0);
                    Debug.Assert(_arrayBufferWriter.WrittenCount == underlyingBuffer.Count);
                    await _stream
                        .WriteAsync(
                            underlyingBuffer.Array,
                            underlyingBuffer.Offset,
                            underlyingBuffer.Count,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
#endif

                    BytesCommitted += _arrayBufferWriter.WrittenCount;
                    _arrayBufferWriter.Clear();
                }
                await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Debug.Assert(_output != null);
                if (BytesPending != 0)
                {
                    _output.Advance(BytesPending);
                    BytesCommitted += BytesPending;
                    BytesPending = 0;
                }
            }
        }

        /// <summary>
        /// Writes the beginning of a KDL array.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray()
        {
            WriteStart(KdlConstants.OpenBracket);
            _tokenType = KdlTokenType.StartArray;
        }

        /// <summary>
        /// Writes the beginning of a KDL object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject()
        {
            WriteStart(KdlConstants.OpenBrace);
            _tokenType = KdlTokenType.StartChildrenBlock;
        }

        private void WriteStart(byte token)
        {
            if (CurrentDepth >= _options.MaxDepth)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    ExceptionResource.DepthTooLarge,
                    _currentDepth,
                    _options.MaxDepth,
                    token: default,
                    tokenType: default
                );
            }

            if (_options.IndentedOrNotSkipValidation)
            {
                WriteStartSlow(token);
            }
            else
            {
                WriteStartMinimized(token);
            }

            _currentDepth &= KdlConstants.RemoveFlagsBitMask;
            _currentDepth++;
        }

        private void WriteStartMinimized(byte token)
        {
            if (_memory.Length - BytesPending < 2) // 1 start token, and optionally, 1 list separator
            {
                Grow(2);
            }

            Span<byte> output = _memory.Span;
            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }
            output[BytesPending++] = token;
        }

        private void WriteStartSlow(byte token)
        {
            Debug.Assert(_options.Indented || !_options.SkipValidation);

            if (_options.Indented)
            {
                if (!_options.SkipValidation)
                {
                    ValidateStart();
                    UpdateBitStackOnStart(token);
                }
                WriteStartIndented(token);
            }
            else
            {
                Debug.Assert(!_options.SkipValidation);
                ValidateStart();
                UpdateBitStackOnStart(token);
                WriteStartMinimized(token);
            }
        }

        private void ValidateStart()
        {
            if (_inObject)
            {
                if (_tokenType != KdlTokenType.PropertyName)
                {
                    Debug.Assert(
                        _tokenType is not KdlTokenType.None and not KdlTokenType.StartArray
                    );
                    ThrowHelper.ThrowInvalidOperationException(
                        ExceptionResource.CannotStartObjectArrayWithoutProperty,
                        currentDepth: default,
                        maxDepth: _options.MaxDepth,
                        token: default,
                        _tokenType
                    );
                }
            }
            else
            {
                Debug.Assert(_tokenType != KdlTokenType.PropertyName);
                Debug.Assert(_tokenType != KdlTokenType.StartChildrenBlock);

                // It is more likely for CurrentDepth to not equal 0 when writing valid KDL, so check that first to rely on short-circuiting and return quickly.
                if (CurrentDepth == 0 && _tokenType != KdlTokenType.None)
                {
                    ThrowHelper.ThrowInvalidOperationException(
                        ExceptionResource.CannotStartObjectArrayAfterPrimitiveOrClose,
                        currentDepth: default,
                        maxDepth: _options.MaxDepth,
                        token: default,
                        _tokenType
                    );
                }
            }
        }

        private void WriteStartIndented(byte token)
        {
            int indent = Indentation;
            Debug.Assert(indent <= _indentLength * _options.MaxDepth);

            int minRequired = indent + 1; // 1 start token
            int maxRequired = minRequired + 3; // Optionally, 1 list separator and 1-2 bytes for new line

            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> output = _memory.Span;

            if (_currentDepth < 0)
            {
                output[BytesPending++] = KdlConstants.ListSeparator;
            }

            if (
                _tokenType is not KdlTokenType.PropertyName and not KdlTokenType.None
                || _commentAfterNoneOrPropertyName
            )
            {
                WriteNewLine(output);
                WriteIndentation(output[BytesPending..], indent);
                BytesPending += indent;
            }

            output[BytesPending++] = token;
        }

        /// <summary>
        /// Writes the beginning of a KDL array with a pre-encoded property name as the key.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(KdlEncodedText propertyName)
        {
            WriteStartHelper(propertyName.EncodedUtf8Bytes, KdlConstants.OpenBracket);
            _tokenType = KdlTokenType.StartArray;
        }

        /// <summary>
        /// Writes the beginning of a KDL object with a pre-encoded property name as the key.
        /// </summary>
        /// <param name="propertyName">The KDL-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(KdlEncodedText propertyName)
        {
            WriteStartHelper(propertyName.EncodedUtf8Bytes, KdlConstants.OpenBrace);
            _tokenType = KdlTokenType.StartChildrenBlock;
        }

        private void WriteStartHelper(ReadOnlySpan<byte> utf8PropertyName, byte token)
        {
            Debug.Assert(utf8PropertyName.Length <= KdlConstants.MaxUnescapedTokenSize);

            ValidateDepth();

            WriteStartByOptions(utf8PropertyName, token);

            _currentDepth &= KdlConstants.RemoveFlagsBitMask;
            _currentDepth++;
        }

        /// <summary>
        /// Writes the beginning of a KDL array with a property name as the key.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded property name of the KDL array to be written.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(ReadOnlySpan<byte> utf8PropertyName)
        {
            ValidatePropertyNameAndDepth(utf8PropertyName);

            WriteStartEscape(utf8PropertyName, KdlConstants.OpenBracket);

            _currentDepth &= KdlConstants.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = KdlTokenType.StartArray;
        }

        /// <summary>
        /// Writes the beginning of a KDL object with a property name as the key.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded property name of the KDL object to be written.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(ReadOnlySpan<byte> utf8PropertyName)
        {
            ValidatePropertyNameAndDepth(utf8PropertyName);

            WriteStartEscape(utf8PropertyName, KdlConstants.OpenBrace);

            _currentDepth &= KdlConstants.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = KdlTokenType.StartChildrenBlock;
        }

        private void WriteStartEscape(ReadOnlySpan<byte> utf8PropertyName, byte token)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(utf8PropertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStartEscapeProperty(utf8PropertyName, token, propertyIdx);
            }
            else
            {
                WriteStartByOptions(utf8PropertyName, token);
            }
        }

        private void WriteStartByOptions(ReadOnlySpan<byte> utf8PropertyName, byte token)
        {
            ValidateWritingProperty(token);

            if (_options.Indented)
            {
                WritePropertyNameIndented(utf8PropertyName, token);
            }
            else
            {
                WritePropertyNameMinimized(utf8PropertyName, token);
            }
        }

        private void WriteStartEscapeProperty(
            ReadOnlySpan<byte> utf8PropertyName,
            byte token,
            int firstEscapeIndexProp
        )
        {
            Debug.Assert(
                int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping
                    >= utf8PropertyName.Length
            );
            Debug.Assert(
                firstEscapeIndexProp >= 0 && firstEscapeIndexProp < utf8PropertyName.Length
            );

            byte[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(
                utf8PropertyName.Length,
                firstEscapeIndexProp
            );

            Span<byte> escapedPropertyName =
                length <= KdlConstants.StackallocByteThreshold
                    ? stackalloc byte[KdlConstants.StackallocByteThreshold]
                    : (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(
                utf8PropertyName,
                escapedPropertyName,
                firstEscapeIndexProp,
                _options.Encoder,
                out int written
            );

            WriteStartByOptions(escapedPropertyName[..written], token);

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        /// <summary>
        /// Writes the beginning of a KDL array with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteStartArray(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the beginning of a KDL object with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }
            WriteStartObject(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the beginning of a KDL array with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(ReadOnlySpan<char> propertyName)
        {
            ValidatePropertyNameAndDepth(propertyName);

            WriteStartEscape(propertyName, KdlConstants.OpenBracket);

            _currentDepth &= KdlConstants.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = KdlTokenType.StartArray;
        }

        /// <summary>
        /// Writes the beginning of a KDL object with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the KDL has exceeded the maximum depth of 1000
        /// OR if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(ReadOnlySpan<char> propertyName)
        {
            ValidatePropertyNameAndDepth(propertyName);

            WriteStartEscape(propertyName, KdlConstants.OpenBrace);

            _currentDepth &= KdlConstants.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = KdlTokenType.StartChildrenBlock;
        }

        private void WriteStartEscape(ReadOnlySpan<char> propertyName, byte token)
        {
            int propertyIdx = KdlWriterHelper.NeedsEscaping(propertyName, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStartEscapeProperty(propertyName, token, propertyIdx);
            }
            else
            {
                WriteStartByOptions(propertyName, token);
            }
        }

        private void WriteStartByOptions(ReadOnlySpan<char> propertyName, byte token)
        {
            ValidateWritingProperty(token);

            if (_options.Indented)
            {
                WritePropertyNameIndented(propertyName, token);
            }
            else
            {
                WritePropertyNameMinimized(propertyName, token);
            }
        }

        private void WriteStartEscapeProperty(
            ReadOnlySpan<char> propertyName,
            byte token,
            int firstEscapeIndexProp
        )
        {
            Debug.Assert(
                int.MaxValue / KdlConstants.MaxExpansionFactorWhileEscaping >= propertyName.Length
            );
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < propertyName.Length);

            char[]? propertyArray = null;

            int length = KdlWriterHelper.GetMaxEscapedLength(
                propertyName.Length,
                firstEscapeIndexProp
            );

            Span<char> escapedPropertyName =
                length <= KdlConstants.StackallocCharThreshold
                    ? stackalloc char[KdlConstants.StackallocCharThreshold]
                    : (propertyArray = ArrayPool<char>.Shared.Rent(length));

            KdlWriterHelper.EscapeString(
                propertyName,
                escapedPropertyName,
                firstEscapeIndexProp,
                _options.Encoder,
                out int written
            );

            WriteStartByOptions(escapedPropertyName[..written], token);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        /// <summary>
        /// Writes the end of a KDL array.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteEndArray()
        {
            WriteEnd(KdlConstants.CloseBracket);
            _tokenType = KdlTokenType.EndArray;
        }

        /// <summary>
        /// Writes the end of a KDL object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid KDL being written (while validation is enabled).
        /// </exception>
        public void WriteEndObject()
        {
            WriteEnd(KdlConstants.CloseBrace);
            _tokenType = KdlTokenType.EndChildrenBlock;
        }

        private void WriteEnd(byte token)
        {
            if (_options.IndentedOrNotSkipValidation)
            {
                WriteEndSlow(token);
            }
            else
            {
                WriteEndMinimized(token);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            // Necessary if WriteEndX is called without a corresponding WriteStartX first.
            if (CurrentDepth != 0)
            {
                _currentDepth--;
            }
        }

        private void WriteEndMinimized(byte token)
        {
            if (_memory.Length - BytesPending < 1) // 1 end token
            {
                Grow(1);
            }

            Span<byte> output = _memory.Span;
            output[BytesPending++] = token;
        }

        private void WriteEndSlow(byte token)
        {
            Debug.Assert(_options.Indented || !_options.SkipValidation);

            if (_options.Indented)
            {
                if (!_options.SkipValidation)
                {
                    ValidateEnd(token);
                }
                WriteEndIndented(token);
            }
            else
            {
                Debug.Assert(!_options.SkipValidation);
                ValidateEnd(token);
                WriteEndMinimized(token);
            }
        }

        private void ValidateEnd(byte token)
        {
            if (_bitStack.CurrentDepth <= 0 || _tokenType == KdlTokenType.PropertyName)
            {
                ThrowHelper.ThrowInvalidOperationException(
                    ExceptionResource.MismatchedObjectArray,
                    currentDepth: default,
                    maxDepth: _options.MaxDepth,
                    token,
                    _tokenType
                );
            }

            if (token == KdlConstants.CloseBracket)
            {
                if (_inObject)
                {
                    Debug.Assert(_tokenType != KdlTokenType.None);
                    ThrowHelper.ThrowInvalidOperationException(
                        ExceptionResource.MismatchedObjectArray,
                        currentDepth: default,
                        maxDepth: _options.MaxDepth,
                        token,
                        _tokenType
                    );
                }
            }
            else
            {
                Debug.Assert(token == KdlConstants.CloseBrace);

                if (!_inObject)
                {
                    ThrowHelper.ThrowInvalidOperationException(
                        ExceptionResource.MismatchedObjectArray,
                        currentDepth: default,
                        maxDepth: _options.MaxDepth,
                        token,
                        _tokenType
                    );
                }
            }

            _inObject = _bitStack.Pop();
        }

        private void WriteEndIndented(byte token)
        {
            // Do not format/indent empty KDL object/array.
            if (_tokenType is KdlTokenType.StartChildrenBlock or KdlTokenType.StartArray)
            {
                WriteEndMinimized(token);
            }
            else
            {
                int indent = Indentation;

                // Necessary if WriteEndX is called without a corresponding WriteStartX first.
                if (indent != 0)
                {
                    // The end token should be at an outer indent and since we haven't updated
                    // current depth yet, explicitly subtract here.
                    indent -= _indentLength;
                }

                Debug.Assert(indent <= _indentLength * _options.MaxDepth);
                Debug.Assert(_options.SkipValidation || _tokenType != KdlTokenType.None);

                int maxRequired = indent + 3; // 1 end token, 1-2 bytes for new line

                if (_memory.Length - BytesPending < maxRequired)
                {
                    Grow(maxRequired);
                }

                Span<byte> output = _memory.Span;

                WriteNewLine(output);

                WriteIndentation(output[BytesPending..], indent);
                BytesPending += indent;

                output[BytesPending++] = token;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteNewLine(Span<byte> output)
        {
            // Write '\r\n' OR '\n', depending on the configured new line string
            Debug.Assert(_newLineLength is 1 or 2, "Invalid new line length.");
            if (_newLineLength == 2)
            {
                output[BytesPending++] = KdlConstants.CarriageReturn;
            }
            output[BytesPending++] = KdlConstants.LineFeed;
        }

        private void WriteIndentation(Span<byte> buffer, int indent)
        {
            KdlWriterHelper.WriteIndentation(buffer, indent, _indentByte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBitStackOnStart(byte token)
        {
            if (token == KdlConstants.OpenBracket)
            {
                _bitStack.PushFalse();
                _inObject = false;
            }
            else
            {
                Debug.Assert(token == KdlConstants.OpenBrace);
                _bitStack.PushTrue();
                _inObject = true;
            }
        }

        private void Grow(int requiredSize)
        {
            Debug.Assert(requiredSize > 0);

            if (_memory.Length == 0)
            {
                FirstCallToGetMemory(requiredSize);
                return;
            }

            int sizeHint = Math.Max(DefaultGrowthSize, requiredSize);

            Debug.Assert(BytesPending != 0);

            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);

                int needed = BytesPending + sizeHint;
                KdlHelpers.ValidateInt32MaxArrayLength((uint)needed);

                _memory = _arrayBufferWriter.GetMemory(needed);

                Debug.Assert(_memory.Length >= sizeHint);
            }
            else
            {
                Debug.Assert(_output != null);

                _output.Advance(BytesPending);
                BytesCommitted += BytesPending;
                BytesPending = 0;

                _memory = _output.GetMemory(sizeHint);

                if (_memory.Length < sizeHint)
                {
                    ThrowHelper.ThrowInvalidOperationException_NeedLargerSpan();
                }
            }
        }

        private void FirstCallToGetMemory(int requiredSize)
        {
            Debug.Assert(_memory.Length == 0);
            Debug.Assert(BytesPending == 0);

            int sizeHint = Math.Max(InitialGrowthSize, requiredSize);

            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);
                _memory = _arrayBufferWriter.GetMemory(sizeHint);
                Debug.Assert(_memory.Length >= sizeHint);
            }
            else
            {
                Debug.Assert(_output != null);
                _memory = _output.GetMemory(sizeHint);

                if (_memory.Length < sizeHint)
                {
                    ThrowHelper.ThrowInvalidOperationException_NeedLargerSpan();
                }
            }
        }

        private void SetFlagToAddListSeparatorBeforeNextItem()
        {
            _currentDepth |= 1 << 31;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay =>
            $"BytesCommitted = {BytesCommitted} BytesPending = {BytesPending} CurrentDepth = {CurrentDepth}";
    }
}
