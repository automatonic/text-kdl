using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Automatonic.Text.Kdl.RandomAccess
{
    public sealed partial class KdlReadOnlyDocument
    {
        // Cached unrented documents for literal values.
        private static KdlReadOnlyDocument? s_nullLiteral;
        private static KdlReadOnlyDocument? s_trueLiteral;
        private static KdlReadOnlyDocument? s_falseLiteral;

        private const int UnseekableStreamInitialRentSize = 4096;

        /// <summary>
        ///   Parse memory as UTF-8 encoded text representing a single KDL value into a KdlDocument.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The <see cref="ReadOnlyMemory{T}"/> value will be used for the entire lifetime of the
        ///     KdlDocument object, and the caller must ensure that the data therein does not change during
        ///     the object lifetime.
        ///   </para>
        ///
        ///   <para>
        ///     Because the input is considered to be text, a UTF-8 Byte-Order-Mark (BOM) must not be present.
        ///   </para>
        /// </remarks>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <returns>
        ///   A KdlDocument representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static KdlReadOnlyDocument Parse(
            ReadOnlyMemory<byte> utf8Kdl,
            KdlReadOnlyDocumentOptions options = default
        )
        {
            return Parse(utf8Kdl, options.GetReaderOptions());
        }

        /// <summary>
        ///   Parse a sequence as UTF-8 encoded text representing a single KDL value into a KdlDocument.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The <see cref="ReadOnlySequence{T}"/> may be used for the entire lifetime of the
        ///     KdlDocument object, and the caller must ensure that the data therein does not change during
        ///     the object lifetime.
        ///   </para>
        ///
        ///   <para>
        ///     Because the input is considered to be text, a UTF-8 Byte-Order-Mark (BOM) must not be present.
        ///   </para>
        /// </remarks>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <returns>
        ///   A KdlDocument representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static KdlReadOnlyDocument Parse(
            ReadOnlySequence<byte> utf8Kdl,
            KdlReadOnlyDocumentOptions options = default
        )
        {
            KdlReaderOptions readerOptions = options.GetReaderOptions();

            if (utf8Kdl.IsSingleSegment)
            {
                return Parse(utf8Kdl.First, readerOptions);
            }

            int length = checked((int)utf8Kdl.Length);
            byte[] utf8Bytes = ArrayPool<byte>.Shared.Rent(length);

            try
            {
                utf8Kdl.CopyTo(utf8Bytes.AsSpan());
                return Parse(utf8Bytes.AsMemory(0, length), readerOptions, utf8Bytes);
            }
            catch
            {
                // Holds document content, clear it before returning it.
                utf8Bytes.AsSpan(0, length).Clear();
                ArrayPool<byte>.Shared.Return(utf8Bytes);
                throw;
            }
        }

        /// <summary>
        ///   Parse a <see cref="Stream"/> as UTF-8 encoded data representing a single KDL value into a
        ///   KdlDocument.  The Stream will be read to completion.
        /// </summary>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <returns>
        ///   A KdlDocument representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static KdlReadOnlyDocument Parse(
            Stream utf8Kdl,
            KdlReadOnlyDocumentOptions options = default
        )
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            ArraySegment<byte> drained = ReadToEnd(utf8Kdl);
            Debug.Assert(drained.Array != null);
            try
            {
                return Parse(drained.AsMemory(), options.GetReaderOptions(), drained.Array);
            }
            catch
            {
                // Holds document content, clear it before returning it.
                drained.AsSpan().Clear();
                ArrayPool<byte>.Shared.Return(drained.Array);
                throw;
            }
        }

        internal static KdlReadOnlyDocument ParseRented(
            PooledByteBufferWriter utf8Kdl,
            KdlReadOnlyDocumentOptions options = default
        )
        {
            return Parse(
                utf8Kdl.WrittenMemory,
                options.GetReaderOptions(),
                extraRentedArrayPoolBytes: null,
                extraPooledByteBufferWriter: utf8Kdl
            );
        }

        internal static KdlReadOnlyDocument ParseValue(
            Stream utf8Kdl,
            KdlReadOnlyDocumentOptions options
        )
        {
            Debug.Assert(utf8Kdl != null);

            ArraySegment<byte> drained = ReadToEnd(utf8Kdl);
            Debug.Assert(drained.Array != null);

            byte[] owned = new byte[drained.Count];
            Buffer.BlockCopy(drained.Array, 0, owned, 0, drained.Count);

            // Holds document content, clear it before returning it.
            drained.AsSpan().Clear();
            ArrayPool<byte>.Shared.Return(drained.Array);

            return ParseUnrented(owned.AsMemory(), options.GetReaderOptions());
        }

        internal static KdlReadOnlyDocument ParseValue(
            ReadOnlySpan<byte> utf8Kdl,
            KdlReadOnlyDocumentOptions options
        )
        {
            byte[] owned = new byte[utf8Kdl.Length];
            utf8Kdl.CopyTo(owned);

            return ParseUnrented(owned.AsMemory(), options.GetReaderOptions());
        }

        internal static KdlReadOnlyDocument ParseValue(
            string kdl,
            KdlReadOnlyDocumentOptions options
        )
        {
            Debug.Assert(kdl != null);
            return ParseValue(kdl.AsMemory(), options);
        }

        /// <summary>
        ///   Parse a <see cref="Stream"/> as UTF-8 encoded data representing a single KDL value into a
        ///   KdlDocument.  The Stream will be read to completion.
        /// </summary>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        ///   A Task to produce a KdlDocument representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static Task<KdlReadOnlyDocument> ParseAsync(
            Stream utf8Kdl,
            KdlReadOnlyDocumentOptions options = default,
            CancellationToken cancellationToken = default
        )
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            return ParseAsyncCore(utf8Kdl, options, cancellationToken);
        }

        private static async Task<KdlReadOnlyDocument> ParseAsyncCore(
            Stream utf8Kdl,
            KdlReadOnlyDocumentOptions options = default,
            CancellationToken cancellationToken = default
        )
        {
            ArraySegment<byte> drained = await ReadToEndAsync(utf8Kdl, cancellationToken)
                .ConfigureAwait(false);
            Debug.Assert(drained.Array != null);
            try
            {
                return Parse(drained.AsMemory(), options.GetReaderOptions(), drained.Array);
            }
            catch
            {
                // Holds document content, clear it before returning it.
                drained.AsSpan().Clear();
                ArrayPool<byte>.Shared.Return(drained.Array);
                throw;
            }
        }

        internal static async Task<KdlReadOnlyDocument> ParseAsyncCoreUnrented(
            Stream utf8Kdl,
            KdlReadOnlyDocumentOptions options = default,
            CancellationToken cancellationToken = default
        )
        {
            ArraySegment<byte> drained = await ReadToEndAsync(utf8Kdl, cancellationToken)
                .ConfigureAwait(false);
            Debug.Assert(drained.Array != null);

            byte[] owned = new byte[drained.Count];
            Buffer.BlockCopy(drained.Array, 0, owned, 0, drained.Count);

            // Holds document content, clear it before returning it.
            drained.AsSpan().Clear();
            ArrayPool<byte>.Shared.Return(drained.Array);

            return ParseUnrented(owned.AsMemory(), options.GetReaderOptions());
        }

        /// <summary>
        ///   Parses text representing a single KDL value into a KdlDocument.
        /// </summary>
        /// <remarks>
        ///   The <see cref="ReadOnlyMemory{T}"/> value may be used for the entire lifetime of the
        ///   KdlDocument object, and the caller must ensure that the data therein does not change during
        ///   the object lifetime.
        /// </remarks>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <returns>
        ///   A KdlDocument representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="kdl"/> does not represent a valid single KDL value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static KdlReadOnlyDocument Parse(
            ReadOnlyMemory<char> kdl,
            KdlReadOnlyDocumentOptions options = default
        )
        {
            ReadOnlySpan<char> kdlChars = kdl.Span;
            int expectedByteCount = KdlReaderHelper.GetUtf8ByteCount(kdlChars);
            byte[] utf8Bytes = ArrayPool<byte>.Shared.Rent(expectedByteCount);

            try
            {
                int actualByteCount = KdlReaderHelper.GetUtf8FromText(kdlChars, utf8Bytes);
                Debug.Assert(expectedByteCount == actualByteCount);

                return Parse(
                    utf8Bytes.AsMemory(0, actualByteCount),
                    options.GetReaderOptions(),
                    utf8Bytes
                );
            }
            catch
            {
                // Holds document content, clear it before returning it.
                utf8Bytes.AsSpan(0, expectedByteCount).Clear();
                ArrayPool<byte>.Shared.Return(utf8Bytes);
                throw;
            }
        }

        internal static KdlReadOnlyDocument ParseValue(
            ReadOnlyMemory<char> kdl,
            KdlReadOnlyDocumentOptions options
        )
        {
            ReadOnlySpan<char> kdlChars = kdl.Span;
            int expectedByteCount = KdlReaderHelper.GetUtf8ByteCount(kdlChars);
            byte[] owned;
            byte[] utf8Bytes = ArrayPool<byte>.Shared.Rent(expectedByteCount);

            try
            {
                int actualByteCount = KdlReaderHelper.GetUtf8FromText(kdlChars, utf8Bytes);
                Debug.Assert(expectedByteCount == actualByteCount);

                owned = new byte[actualByteCount];
                Buffer.BlockCopy(utf8Bytes, 0, owned, 0, actualByteCount);
            }
            finally
            {
                // Holds document content, clear it before returning it.
                utf8Bytes.AsSpan(0, expectedByteCount).Clear();
                ArrayPool<byte>.Shared.Return(utf8Bytes);
            }

            return ParseUnrented(owned.AsMemory(), options.GetReaderOptions());
        }

        /// <summary>
        ///   Parses text representing a single KDL value into a KdlDocument.
        /// </summary>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <returns>
        ///   A KdlDocument representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="kdl"/> does not represent a valid single KDL value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static KdlReadOnlyDocument Parse(
            string kdl,
            KdlReadOnlyDocumentOptions options = default
        )
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }

            return Parse(kdl.AsMemory(), options);
        }

        /// <summary>
        ///   Attempts to parse one KDL value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <param name="document">Receives the parsed document.</param>
        /// <returns>
        ///   <see langword="true"/> if a value was read and parsed into a KdlDocument,
        ///   <see langword="false"/> if the reader ran out of data while parsing.
        ///   All other situations result in an exception being thrown.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="KdlReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="KdlTokenType.PropertyName"/> or <see cref="KdlTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="KdlReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        ///
        ///   <para>
        ///     Upon completion of this method, <paramref name="reader"/> will be positioned at the
        ///     final token in the KDL value.  If an exception is thrown, or <see langword="false"/>
        ///     is returned, the reader is reset to the state it was in when the method was called.
        ///   </para>
        ///
        ///   <para>
        ///     This method makes a copy of the data the reader acted on, so there is no caller
        ///     requirement to maintain data integrity beyond the return of this method.
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The current <paramref name="reader"/> token does not start or represent a value.
        /// </exception>
        /// <exception cref="KdlException">
        ///   A value could not be read from the reader.
        /// </exception>
        public static bool TryParseValue(
            ref KdlReader reader,
            [NotNullWhen(true)] out KdlReadOnlyDocument? document
        )
        {
            return TryParseValue(ref reader, out document, shouldThrow: false, useArrayPools: true);
        }

        /// <summary>
        ///   Parses one KDL value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <returns>
        ///   A KdlDocument representing the value (and nested values) read from the reader.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="KdlReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="KdlTokenType.PropertyName"/> or <see cref="KdlTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="KdlReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        ///
        ///   <para>
        ///     Upon completion of this method, <paramref name="reader"/> will be positioned at the
        ///     final token in the KDL value. If an exception is thrown, the reader is reset to
        ///     the state it was in when the method was called.
        ///   </para>
        ///
        ///   <para>
        ///     This method makes a copy of the data the reader acted on, so there is no caller
        ///     requirement to maintain data integrity beyond the return of this method.
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The current <paramref name="reader"/> token does not start or represent a value.
        /// </exception>
        /// <exception cref="KdlException">
        ///   A value could not be read from the reader.
        /// </exception>
        public static KdlReadOnlyDocument ParseValue(ref KdlReader reader)
        {
            bool ret = TryParseValue(
                ref reader,
                out KdlReadOnlyDocument? document,
                shouldThrow: true,
                useArrayPools: true
            );

            Debug.Assert(ret, "TryParseValue returned false with shouldThrow: true.");
            Debug.Assert(document != null, "null document returned with shouldThrow: true.");
            return document;
        }

        internal static bool TryParseValue(
            ref KdlReader reader,
            [NotNullWhen(true)] out KdlReadOnlyDocument? document,
            bool shouldThrow,
            bool useArrayPools
        )
        {
            KdlReaderState state = reader.CurrentState;
            CheckSupportedOptions(state.Options, nameof(reader));

            // Value copy to overwrite the ref on an exception and undo the destructive reads.
            KdlReader restore = reader;

            ReadOnlySpan<byte> valueSpan = default;
            ReadOnlySequence<byte> valueSequence = default;

            try
            {
                switch (reader.TokenType)
                {
                    // A new reader was created and has never been read,
                    // so we need to move to the first token.
                    // (or a reader has terminated and we're about to throw)
                    case KdlTokenType.None:
                    // Using a reader loop the caller has identified a property they wish to
                    // hydrate into a KdlDocument. Move to the value first.
                    case KdlTokenType.PropertyName:
                    {
                        if (!reader.Read())
                        {
                            if (shouldThrow)
                            {
                                ThrowHelper.ThrowKdlReaderException(
                                    ref reader,
                                    ExceptionResource.ExpectedKdlTokens
                                );
                            }

                            reader = restore;
                            document = null;
                            return false;
                        }

                        break;
                    }
                }

                switch (reader.TokenType)
                {
                    // Any of the "value start" states are acceptable.
                    case KdlTokenType.StartChildrenBlock:
                    case KdlTokenType.StartArray:
                    {
                        long startingOffset = reader.TokenStartIndex;

                        if (!reader.TrySkip())
                        {
                            if (shouldThrow)
                            {
                                ThrowHelper.ThrowKdlReaderException(
                                    ref reader,
                                    ExceptionResource.ExpectedKdlTokens
                                );
                            }

                            reader = restore;
                            document = null;
                            return false;
                        }

                        long totalLength = reader.BytesConsumed - startingOffset;
                        ReadOnlySequence<byte> sequence = reader.OriginalSequence;

                        if (sequence.IsEmpty)
                        {
                            valueSpan = reader.OriginalSpan.Slice(
                                checked((int)startingOffset),
                                checked((int)totalLength)
                            );
                        }
                        else
                        {
                            valueSequence = sequence.Slice(startingOffset, totalLength);
                        }

                        Debug.Assert(
                            reader.TokenType
                                is KdlTokenType.EndChildrenBlock
                                    or KdlTokenType.EndArray
                        );

                        break;
                    }

                    case KdlTokenType.False:
                    case KdlTokenType.True:
                    case KdlTokenType.Null:
                        if (useArrayPools)
                        {
                            if (reader.HasValueSequence)
                            {
                                valueSequence = reader.ValueSequence;
                            }
                            else
                            {
                                valueSpan = reader.ValueSpan;
                            }

                            break;
                        }

                        document = CreateForLiteral(reader.TokenType);
                        return true;

                    case KdlTokenType.Number:
                    {
                        if (reader.HasValueSequence)
                        {
                            valueSequence = reader.ValueSequence;
                        }
                        else
                        {
                            valueSpan = reader.ValueSpan;
                        }

                        break;
                    }

                    // String's ValueSequence/ValueSpan omits the quotes, we need them back.
                    case KdlTokenType.String:
                    {
                        ReadOnlySequence<byte> sequence = reader.OriginalSequence;

                        if (sequence.IsEmpty)
                        {
                            // Since the quoted string fit in a ReadOnlySpan originally
                            // the contents length plus the two quotes can't overflow.
                            int payloadLength = reader.ValueSpan.Length + 2;
                            Debug.Assert(payloadLength > 1);

                            ReadOnlySpan<byte> readerSpan = reader.OriginalSpan;

                            Debug.Assert(
                                readerSpan[(int)reader.TokenStartIndex] == (byte)'"',
                                $"Calculated span starts with {readerSpan[(int)reader.TokenStartIndex]}"
                            );

                            Debug.Assert(
                                readerSpan[(int)reader.TokenStartIndex + payloadLength - 1]
                                    == (byte)'"',
                                $"Calculated span ends with {readerSpan[(int)reader.TokenStartIndex + payloadLength - 1]}"
                            );

                            valueSpan = readerSpan.Slice(
                                (int)reader.TokenStartIndex,
                                payloadLength
                            );
                        }
                        else
                        {
                            long payloadLength = 2;

                            if (reader.HasValueSequence)
                            {
                                payloadLength += reader.ValueSequence.Length;
                            }
                            else
                            {
                                payloadLength += reader.ValueSpan.Length;
                            }

                            valueSequence = sequence.Slice(reader.TokenStartIndex, payloadLength);
                            Debug.Assert(
                                valueSequence.First.Span[0] == (byte)'"',
                                $"Calculated sequence starts with {valueSequence.First.Span[0]}"
                            );

                            Debug.Assert(
                                valueSequence.ToArray()[payloadLength - 1] == (byte)'"',
                                $"Calculated sequence ends with {valueSequence.ToArray()[payloadLength - 1]}"
                            );
                        }

                        break;
                    }
                    default:
                    {
                        if (shouldThrow)
                        {
                            // Default case would only hit if TokenType equals KdlTokenType.EndObject or KdlTokenType.EndArray in which case it would never be sequence
                            Debug.Assert(!reader.HasValueSequence);
                            byte displayByte = reader.ValueSpan[0];

                            ThrowHelper.ThrowKdlReaderException(
                                ref reader,
                                ExceptionResource.ExpectedStartOfValueNotFound,
                                displayByte
                            );
                        }

                        reader = restore;
                        document = null;
                        return false;
                    }
                }
            }
            catch
            {
                reader = restore;
                throw;
            }

            int length = valueSpan.IsEmpty ? checked((int)valueSequence.Length) : valueSpan.Length;
            if (useArrayPools)
            {
                byte[] rented = ArrayPool<byte>.Shared.Rent(length);
                Span<byte> rentedSpan = rented.AsSpan(0, length);

                try
                {
                    if (valueSpan.IsEmpty)
                    {
                        valueSequence.CopyTo(rentedSpan);
                    }
                    else
                    {
                        valueSpan.CopyTo(rentedSpan);
                    }

                    document = Parse(rented.AsMemory(0, length), state.Options, rented);
                }
                catch
                {
                    // This really shouldn't happen since the document was already checked
                    // for consistency by Skip.  But if data mutations happened just after
                    // the calls to Read then the copy may not be valid.
                    rentedSpan.Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                    throw;
                }
            }
            else
            {
                byte[] owned;

                if (valueSpan.IsEmpty)
                {
                    owned = valueSequence.ToArray();
                }
                else
                {
                    owned = valueSpan.ToArray();
                }

                document = ParseUnrented(owned, state.Options, reader.TokenType);
            }

            return true;
        }

        private static KdlReadOnlyDocument CreateForLiteral(KdlTokenType tokenType)
        {
            switch (tokenType)
            {
                case KdlTokenType.False:
                    s_falseLiteral ??= Create(KdlConstants.FalseValue.ToArray());
                    return s_falseLiteral;
                case KdlTokenType.True:
                    s_trueLiteral ??= Create(KdlConstants.TrueValue.ToArray());
                    return s_trueLiteral;
                default:
                    Debug.Assert(tokenType == KdlTokenType.Null);
                    s_nullLiteral ??= Create(KdlConstants.NullValue.ToArray());
                    return s_nullLiteral;
            }

            KdlReadOnlyDocument Create(byte[] utf8Kdl)
            {
                MetadataDb database = MetadataDb.CreateLocked(utf8Kdl.Length);
                database.Append(tokenType, startLocation: 0, utf8Kdl.Length);
                return new KdlReadOnlyDocument(utf8Kdl, database, isDisposable: false);
            }
        }

        private static KdlReadOnlyDocument Parse(
            ReadOnlyMemory<byte> utf8Kdl,
            KdlReaderOptions readerOptions,
            byte[]? extraRentedArrayPoolBytes = null,
            PooledByteBufferWriter? extraPooledByteBufferWriter = null
        )
        {
            ReadOnlySpan<byte> utf8KdlSpan = utf8Kdl.Span;
            var database = MetadataDb.CreateRented(utf8Kdl.Length, convertToAlloc: false);
            var stack = new StackRowStack(
                KdlReadOnlyDocumentOptions.DefaultMaxDepth * StackRow.Size
            );

            try
            {
                Parse(utf8KdlSpan, readerOptions, ref database, ref stack);
            }
            catch
            {
                database.Dispose();
                throw;
            }
            finally
            {
                stack.Dispose();
            }

            return new KdlReadOnlyDocument(
                utf8Kdl,
                database,
                extraRentedArrayPoolBytes,
                extraPooledByteBufferWriter
            );
        }

        private static KdlReadOnlyDocument ParseUnrented(
            ReadOnlyMemory<byte> utf8Kdl,
            KdlReaderOptions readerOptions,
            KdlTokenType tokenType = KdlTokenType.None
        )
        {
            // These tokens should already have been processed.
            Debug.Assert(
                tokenType
                    is not KdlTokenType.Null
                        and not KdlTokenType.False
                        and not KdlTokenType.True
            );

            ReadOnlySpan<byte> utf8KdlSpan = utf8Kdl.Span;
            MetadataDb database;

            if (tokenType is KdlTokenType.String or KdlTokenType.Number)
            {
                // For primitive types, we can avoid renting MetadataDb and creating StackRowStack.
                database = MetadataDb.CreateLocked(utf8Kdl.Length);
                StackRowStack stack = default;
                Parse(utf8KdlSpan, readerOptions, ref database, ref stack);
            }
            else
            {
                database = MetadataDb.CreateRented(utf8Kdl.Length, convertToAlloc: true);
                var stack = new StackRowStack(
                    KdlReadOnlyDocumentOptions.DefaultMaxDepth * StackRow.Size
                );
                try
                {
                    Parse(utf8KdlSpan, readerOptions, ref database, ref stack);
                }
                finally
                {
                    stack.Dispose();
                }
            }

            return new KdlReadOnlyDocument(utf8Kdl, database, isDisposable: false);
        }

        private static ArraySegment<byte> ReadToEnd(Stream stream)
        {
            int written = 0;
            byte[]? rented = null;

            ReadOnlySpan<byte> utf8Bom = KdlConstants.Utf8Bom;

            try
            {
                if (stream.CanSeek)
                {
                    // Ask for 1 more than the length to avoid resizing later,
                    // which is unnecessary in the common case where the stream length doesn't change.
                    long expectedLength =
                        Math.Max(utf8Bom.Length, stream.Length - stream.Position) + 1;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)expectedLength));
                }
                else
                {
                    rented = ArrayPool<byte>.Shared.Rent(UnseekableStreamInitialRentSize);
                }

                int lastRead;

                // Read up to 3 bytes to see if it's the UTF-8 BOM
                do
                {
                    // No need for checking for growth, the minimal rent sizes both guarantee it'll fit.
                    Debug.Assert(rented.Length >= utf8Bom.Length);

                    lastRead = stream.Read(rented, written, utf8Bom.Length - written);

                    written += lastRead;
                } while (lastRead > 0 && written < utf8Bom.Length);

                // If we have 3 bytes, and they're the BOM, reset the write position to 0.
                if (
                    written == utf8Bom.Length
                    && utf8Bom.SequenceEqual(rented.AsSpan(0, utf8Bom.Length))
                )
                {
                    written = 0;
                }

                do
                {
                    if (rented.Length == written)
                    {
                        byte[] toReturn = rented;
                        rented = ArrayPool<byte>.Shared.Rent(checked(toReturn.Length * 2));
                        Buffer.BlockCopy(toReturn, 0, rented, 0, toReturn.Length);
                        // Holds document content, clear it.
                        ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                    }

                    lastRead = stream.Read(rented, written, rented.Length - written);
                    written += lastRead;
                } while (lastRead > 0);

                return new ArraySegment<byte>(rented, 0, written);
            }
            catch
            {
                if (rented != null)
                {
                    // Holds document content, clear it before returning it.
                    rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }

        private static async
#if NET
        ValueTask<ArraySegment<byte>>
#else
        Task<ArraySegment<byte>>
#endif
        ReadToEndAsync(Stream stream, CancellationToken cancellationToken)
        {
            int written = 0;
            byte[]? rented = null;

            try
            {
                // Save the length to a local to be reused across awaits.
                int utf8BomLength = KdlConstants.Utf8Bom.Length;

                if (stream.CanSeek)
                {
                    // Ask for 1 more than the length to avoid resizing later,
                    // which is unnecessary in the common case where the stream length doesn't change.
                    long expectedLength =
                        Math.Max(utf8BomLength, stream.Length - stream.Position) + 1;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)expectedLength));
                }
                else
                {
                    rented = ArrayPool<byte>.Shared.Rent(UnseekableStreamInitialRentSize);
                }

                int lastRead;

                // Read up to 3 bytes to see if it's the UTF-8 BOM
                do
                {
                    // No need for checking for growth, the minimal rent sizes both guarantee it'll fit.
                    Debug.Assert(rented.Length >= KdlConstants.Utf8Bom.Length);

                    lastRead = await stream
                        .ReadAsync(
#if NET
                            rented.AsMemory(written, utf8BomLength - written),
#else
                            rented, written, utf8BomLength - written,
#endif
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    written += lastRead;
                } while (lastRead > 0 && written < utf8BomLength);

                // If we have 3 bytes, and they're the BOM, reset the write position to 0.
                if (
                    written == utf8BomLength
                    && KdlConstants.Utf8Bom.SequenceEqual(rented.AsSpan(0, utf8BomLength))
                )
                {
                    written = 0;
                }

                do
                {
                    if (rented.Length == written)
                    {
                        byte[] toReturn = rented;
                        rented = ArrayPool<byte>.Shared.Rent(toReturn.Length * 2);
                        Buffer.BlockCopy(toReturn, 0, rented, 0, toReturn.Length);
                        // Holds document content, clear it.
                        ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                    }

                    lastRead = await stream
                        .ReadAsync(
#if NET
                            rented.AsMemory(written),
#else
                            rented, written, rented.Length - written,
#endif
                            cancellationToken)
                        .ConfigureAwait(false);

                    written += lastRead;
                } while (lastRead > 0);

                return new ArraySegment<byte>(rented, 0, written);
            }
            catch
            {
                if (rented != null)
                {
                    // Holds document content, clear it before returning it.
                    rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }
    }
}
