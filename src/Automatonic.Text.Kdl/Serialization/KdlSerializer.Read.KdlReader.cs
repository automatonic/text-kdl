using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Reads one KDL value (including objects or arrays) from the provided reader into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="reader">The reader to read.</param>
        /// <param name="options">Options to control the serializer behavior during reading.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or a value could not be read from the reader.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
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
        ///
        ///   <para>
        ///     The <see cref="KdlReaderOptions"/> used to create the instance of the <see cref="KdlReader"/> take precedence over the <see cref="KdlSerializerOptions"/> when they conflict.
        ///     Hence, <see cref="KdlReaderOptions.AllowTrailingCommas"/>, <see cref="KdlReaderOptions.MaxDepth"/>, and <see cref="KdlReaderOptions.CommentHandling"/> are used while reading.
        ///   </para>
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>(ref KdlReader reader, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return Read<TValue>(ref reader, jsonTypeInfo);
        }

        /// <summary>
        /// Reads one KDL value (including objects or arrays) from the provided reader into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="reader">The reader to read.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the serializer behavior during reading.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <paramref name="returnType"/> is not compatible with the KDL,
        /// or a value could not be read from the reader.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
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
        ///   <para>
        ///     The <see cref="KdlReaderOptions"/> used to create the instance of the <see cref="KdlReader"/> take precedence over the <see cref="KdlSerializerOptions"/> when they conflict.
        ///     Hence, <see cref="KdlReaderOptions.AllowTrailingCommas"/>, <see cref="KdlReaderOptions.MaxDepth"/>, and <see cref="KdlReaderOptions.CommentHandling"/> are used while reading.
        ///   </para>
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(ref KdlReader reader, Type returnType, KdlSerializerOptions? options = null)
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, returnType);
            return ReadAsObject(ref reader, jsonTypeInfo);
        }

        /// <summary>
        /// Reads one KDL value (including objects or arrays) from the provided reader into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="reader">The reader to read.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or a value could not be read from the reader.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
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
        ///
        ///   <para>
        ///     The <see cref="KdlReaderOptions"/> used to create the instance of the <see cref="KdlReader"/> take precedence over the <see cref="KdlSerializerOptions"/> when they conflict.
        ///     Hence, <see cref="KdlReaderOptions.AllowTrailingCommas"/>, <see cref="KdlReaderOptions.MaxDepth"/>, and <see cref="KdlReaderOptions.CommentHandling"/> are used while reading.
        ///   </para>
        /// </remarks>
        public static TValue? Deserialize<TValue>(ref KdlReader reader, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return Read(ref reader, jsonTypeInfo);
        }

        /// <summary>
        /// Reads one KDL value (including objects or arrays) from the provided reader into an instance specified by the <paramref name="jsonTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="jsonTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="reader">The reader to read.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <paramref name="jsonTypeInfo"/> is not compatible with the KDL,
        /// or a value could not be read from the reader.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
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
        ///
        ///   <para>
        ///     The <see cref="KdlReaderOptions"/> used to create the instance of the <see cref="KdlReader"/> take precedence over the <see cref="KdlSerializerOptions"/> when they conflict.
        ///     Hence, <see cref="KdlReaderOptions.AllowTrailingCommas"/>, <see cref="KdlReaderOptions.MaxDepth"/>, and <see cref="KdlReaderOptions.CommentHandling"/> are used while reading.
        ///   </para>
        /// </remarks>
        public static object? Deserialize(ref KdlReader reader, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadAsObject(ref reader, jsonTypeInfo);
        }

        /// <summary>
        /// Reads one KDL value (including objects or arrays) from the provided reader into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="reader">The reader to read.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="returnType"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <paramref name="returnType"/> is not compatible with the KDL,
        /// or a value could not be read from the reader.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method on the provided <paramref name="context"/>
        /// did not return a compatible <see cref="KdlTypeInfo"/> for <paramref name="returnType"/>.
        /// </exception>
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
        ///   <para>
        ///     The <see cref="KdlReaderOptions"/> used to create the instance of the <see cref="KdlReader"/> take precedence over the <see cref="KdlSerializerOptions"/> when they conflict.
        ///     Hence, <see cref="KdlReaderOptions.AllowTrailingCommas"/>, <see cref="KdlReaderOptions.MaxDepth"/>, and <see cref="KdlReaderOptions.CommentHandling"/> are used while reading.
        ///   </para>
        /// </remarks>
        public static object? Deserialize(ref KdlReader reader, Type returnType, KdlSerializerContext context)
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            return ReadAsObject(ref reader, GetTypeInfo(context, returnType));
        }

        private static TValue? Read<TValue>(ref KdlReader reader, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);

            if (reader.CurrentState.Options.CommentHandling == KdlCommentHandling.Allow)
            {
                ThrowHelper.ThrowArgumentException_SerializerDoesNotSupportComments(nameof(reader));
            }

            ReadStack state = default;
            state.Initialize(jsonTypeInfo);
            KdlReader restore = reader;

            try
            {
                KdlReader scopedReader = GetReaderScopedToNextValue(ref reader, ref state);
                return jsonTypeInfo.Deserialize(ref scopedReader, ref state);
            }
            catch (KdlException)
            {
                reader = restore;
                throw;
            }
        }

        private static object? ReadAsObject(ref KdlReader reader, KdlTypeInfo jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);

            if (reader.CurrentState.Options.CommentHandling == KdlCommentHandling.Allow)
            {
                ThrowHelper.ThrowArgumentException_SerializerDoesNotSupportComments(nameof(reader));
            }

            ReadStack state = default;
            state.Initialize(jsonTypeInfo);
            KdlReader restore = reader;

            try
            {
                KdlReader scopedReader = GetReaderScopedToNextValue(ref reader, ref state);
                return jsonTypeInfo.DeserializeAsObject(ref scopedReader, ref state);
            }
            catch (KdlException)
            {
                reader = restore;
                throw;
            }
        }

        private static KdlReader GetReaderScopedToNextValue(ref KdlReader reader, scoped ref ReadStack state)
        {
            // Advances the provided reader, validating that it is pointing to a complete KDL value.
            // If successful, returns a new KdlReader that is scoped to the next value, reusing existing buffers.

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
                            ThrowHelper.ThrowKdlReaderException(ref reader, ExceptionResource.ExpectedOneCompleteToken);
                        }
                        break;
                    }
                }

                switch (reader.TokenType)
                {
                    // Any of the "value start" states are acceptable.
                    case KdlTokenType.StartObject:
                    case KdlTokenType.StartArray:
                        long startingOffset = reader.TokenStartIndex;

                        if (!reader.TrySkip())
                        {
                            ThrowHelper.ThrowKdlReaderException(ref reader, ExceptionResource.NotEnoughData);
                        }

                        long totalLength = reader.BytesConsumed - startingOffset;
                        ReadOnlySequence<byte> sequence = reader.OriginalSequence;

                        if (sequence.IsEmpty)
                        {
                            valueSpan = reader.OriginalSpan.Slice(
                                checked((int)startingOffset),
                                checked((int)totalLength));
                        }
                        else
                        {
                            valueSequence = sequence.Slice(startingOffset, totalLength);
                        }

                        Debug.Assert(reader.TokenType is KdlTokenType.EndObject or KdlTokenType.EndArray);
                        break;

                    // Single-token values
                    case KdlTokenType.Number:
                    case KdlTokenType.True:
                    case KdlTokenType.False:
                    case KdlTokenType.Null:
                        if (reader.HasValueSequence)
                        {
                            valueSequence = reader.ValueSequence;
                        }
                        else
                        {
                            valueSpan = reader.ValueSpan;
                        }

                        break;

                    // String's ValueSequence/ValueSpan omits the quotes, we need them back.
                    case KdlTokenType.String:
                        ReadOnlySequence<byte> originalSequence = reader.OriginalSequence;

                        if (originalSequence.IsEmpty)
                        {
                            // Since the quoted string fit in a ReadOnlySpan originally
                            // the contents length plus the two quotes can't overflow.
                            int payloadLength = reader.ValueSpan.Length + 2;
                            Debug.Assert(payloadLength > 1);

                            ReadOnlySpan<byte> readerSpan = reader.OriginalSpan;

                            Debug.Assert(
                                readerSpan[(int)reader.TokenStartIndex] == (byte)'"',
                                $"Calculated span starts with {readerSpan[(int)reader.TokenStartIndex]}");

                            Debug.Assert(
                                readerSpan[(int)reader.TokenStartIndex + payloadLength - 1] == (byte)'"',
                                $"Calculated span ends with {readerSpan[(int)reader.TokenStartIndex + payloadLength - 1]}");

                            valueSpan = readerSpan.Slice((int)reader.TokenStartIndex, payloadLength);
                        }
                        else
                        {
                            long payloadLength = reader.HasValueSequence
                                ? reader.ValueSequence.Length + 2
                                : reader.ValueSpan.Length + 2;

                            valueSequence = originalSequence.Slice(reader.TokenStartIndex, payloadLength);
                            Debug.Assert(
                                valueSequence.First.Span[0] == (byte)'"',
                                $"Calculated sequence starts with {valueSequence.First.Span[0]}");

                            Debug.Assert(
                                valueSequence.ToArray()[payloadLength - 1] == (byte)'"',
                                $"Calculated sequence ends with {valueSequence.ToArray()[payloadLength - 1]}");
                        }

                        break;

                    default:
                        byte displayByte = reader.HasValueSequence
                            ? reader.ValueSequence.First.Span[0]
                            : reader.ValueSpan[0];

                        ThrowHelper.ThrowKdlReaderException(
                            ref reader,
                            ExceptionResource.ExpectedStartOfValueNotFound,
                            displayByte);

                        break;
                }
            }
            catch (KdlReaderException ex)
            {
                // Re-throw with Path information.
                ThrowHelper.ReThrowWithPath(ref state, ex);
            }

            Debug.Assert(!valueSpan.IsEmpty ^ !valueSequence.IsEmpty);

            return valueSpan.IsEmpty
                ? new KdlReader(valueSequence, reader.CurrentState.Options)
                : new KdlReader(valueSpan, reader.CurrentState.Options);
        }
    }
}
