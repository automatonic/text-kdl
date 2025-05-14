using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Converters;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static ValueTask<TValue?> DeserializeAsync<TValue>(
            Stream utf8Kdl,
            KdlSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            return kdlTypeInfo.DeserializeAsync(utf8Kdl, cancellationToken);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>(
            Stream utf8Kdl,
            KdlSerializerOptions? options = null)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            return kdlTypeInfo.Deserialize(utf8Kdl);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <paramref name="returnType"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// the <paramref name="returnType"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static ValueTask<object?> DeserializeAsync(
            Stream utf8Kdl,
            Type returnType,
            KdlSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(options, returnType);
            return kdlTypeInfo.DeserializeAsObjectAsync(utf8Kdl, cancellationToken);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <paramref name="returnType"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// the <paramref name="returnType"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(
            Stream utf8Kdl,
            Type returnType,
            KdlSerializerOptions? options = null)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(options, returnType);
            return kdlTypeInfo.DeserializeAsObject(utf8Kdl);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        public static ValueTask<TValue?> DeserializeAsync<TValue>(
            Stream utf8Kdl,
            KdlTypeInfo<TValue> kdlTypeInfo,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return kdlTypeInfo.DeserializeAsync(utf8Kdl, cancellationToken);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into an instance specified by the <paramref name="kdlTypeInfo"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="kdlTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// or when there is remaining data in the Stream.
        /// </exception>
        public static ValueTask<object?> DeserializeAsync(
            Stream utf8Kdl,
            KdlTypeInfo kdlTypeInfo,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return kdlTypeInfo.DeserializeAsObjectAsync(utf8Kdl, cancellationToken);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        public static TValue? Deserialize<TValue>(
            Stream utf8Kdl,
            KdlTypeInfo<TValue> kdlTypeInfo)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return kdlTypeInfo.Deserialize(utf8Kdl);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into an instance specified by the <paramref name="kdlTypeInfo"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="kdlTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// or when there is remaining data in the Stream.
        /// </exception>
        public static object? Deserialize(
            Stream utf8Kdl,
            KdlTypeInfo kdlTypeInfo)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return kdlTypeInfo.DeserializeAsObject(utf8Kdl);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <paramref name="returnType"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/>, <paramref name="returnType"/>, or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// the <paramref name="returnType"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method on the provided <paramref name="context"/>
        /// did not return a compatible <see cref="KdlTypeInfo"/> for <paramref name="returnType"/>.
        /// </exception>
        public static ValueTask<object?> DeserializeAsync(
            Stream utf8Kdl,
            Type returnType,
            KdlSerializerContext context,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(context, returnType);
            return kdlTypeInfo.DeserializeAsObjectAsync(utf8Kdl, cancellationToken);
        }

        /// <summary>
        /// Reads the UTF-8 encoded text representing a single KDL value into a <paramref name="returnType"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/>, <paramref name="returnType"/>, or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// the <paramref name="returnType"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method on the provided <paramref name="context"/>
        /// did not return a compatible <see cref="KdlTypeInfo"/> for <paramref name="returnType"/>.
        /// </exception>
        public static object? Deserialize(
            Stream utf8Kdl,
            Type returnType,
            KdlSerializerContext context)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(context, returnType);
            return kdlTypeInfo.DeserializeAsObject(utf8Kdl);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text into an <see cref="IAsyncEnumerable{TValue}" />
        /// that can be used to deserialize root-level KDL arrays in a streaming manner.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <returns>An <see cref="IAsyncEnumerable{TValue}" /> representation of the provided KDL array.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(
            Stream utf8Kdl,
            KdlSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return DeserializeAsyncEnumerable<TValue>(utf8Kdl, topLevelValues: false, options, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text into an <see cref="IAsyncEnumerable{TValue}" />
        /// that can be used to deserialize sequences of KDL values in a streaming manner.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <returns>An <see cref="IAsyncEnumerable{TValue}" /> representation of the provided KDL sequence.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="topLevelValues"><see langword="true"/> to deserialize from a sequence of top-level KDL values, or <see langword="false"/> to deserialize from a single top-level array.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// When <paramref name="topLevelValues"/> is set to <see langword="true" />, treats the stream as a sequence of
        /// whitespace separated top-level KDL values and attempts to deserialize each value into <typeparamref name="TValue"/>.
        /// When <paramref name="topLevelValues"/> is set to <see langword="false" />, treats the stream as a KDL array and
        /// attempts to serialize each element into <typeparamref name="TValue"/>.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(
            Stream utf8Kdl,
            bool topLevelValues,
            KdlSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncEnumerableCore(utf8Kdl, kdlTypeInfo, topLevelValues, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text into an <see cref="IAsyncEnumerable{TValue}" />
        /// that can be used to deserialize root-level KDL arrays in a streaming manner.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <returns>An <see cref="IAsyncEnumerable{TValue}" /> representation of the provided KDL array.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the element type to convert.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(
            Stream utf8Kdl,
            KdlTypeInfo<TValue> kdlTypeInfo,
            CancellationToken cancellationToken = default)
        {
            return DeserializeAsyncEnumerable(utf8Kdl, kdlTypeInfo, topLevelValues: false, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text into an <see cref="IAsyncEnumerable{TValue}" />
        /// that can be used to deserialize sequences of KDL values in a streaming manner.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <returns>An <see cref="IAsyncEnumerable{TValue}" /> representation of the provided KDL sequence.</returns>
        /// <param name="utf8Kdl">KDL data to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the element type to convert.</param>
        /// <param name="topLevelValues">Whether to deserialize from a sequence of top-level KDL values.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the read operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// When <paramref name="topLevelValues"/> is set to <see langword="true" />, treats the stream as a sequence of
        /// whitespace separated top-level KDL values and attempts to deserialize each value into <typeparamref name="TValue"/>.
        /// When <paramref name="topLevelValues"/> is set to <see langword="false" />, treats the stream as a KDL array and
        /// attempts to serialize each element into <typeparamref name="TValue"/>.
        /// </remarks>
        public static IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(
            Stream utf8Kdl,
            KdlTypeInfo<TValue> kdlTypeInfo,
            bool topLevelValues,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return DeserializeAsyncEnumerableCore(utf8Kdl, kdlTypeInfo, topLevelValues, cancellationToken);
        }

        private static IAsyncEnumerable<T?> DeserializeAsyncEnumerableCore<T>(
            Stream utf8Kdl,
            KdlTypeInfo<T> kdlTypeInfo,
            bool topLevelValues,
            CancellationToken cancellationToken)
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);

            KdlTypeInfo<List<T?>> listTypeInfo;
            KdlReaderOptions readerOptions = kdlTypeInfo.Options.GetReaderOptions();
            if (topLevelValues)
            {
                listTypeInfo = GetOrAddListTypeInfoForRootLevelValueMode(kdlTypeInfo);
                readerOptions.AllowMultipleValues = true;
            }
            else
            {
                listTypeInfo = GetOrAddListTypeInfoForArrayMode(kdlTypeInfo);
            }

            return CreateAsyncEnumerableFromArray(utf8Kdl, listTypeInfo, readerOptions, cancellationToken);

            static async IAsyncEnumerable<T?> CreateAsyncEnumerableFromArray(
                Stream utf8Kdl,
                KdlTypeInfo<List<T?>> listTypeInfo,
                KdlReaderOptions readerOptions,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                Debug.Assert(listTypeInfo.IsConfigured);

                ReadBufferState bufferState = new(listTypeInfo.Options.DefaultBufferSize);
                ReadStack readStack = default;
                readStack.Initialize(listTypeInfo, supportContinuation: true);
                KdlReaderState kdlReaderState = new(readerOptions);

                try
                {
                    bool success;
                    do
                    {
                        bufferState = await bufferState.ReadFromStreamAsync(utf8Kdl, cancellationToken, fillBuffer: false).ConfigureAwait(false);
                        success = listTypeInfo.ContinueDeserialize(
                            ref bufferState,
                            ref kdlReaderState,
                            ref readStack,
                            out List<T?>? _);

                        if (readStack.Current.ReturnValue is { } returnValue)
                        {
                            var list = (List<T?>)returnValue;
                            foreach (T? item in list)
                            {
                                yield return item;
                            }

                            list.Clear();
                        }

                    } while (!success);
                }
                finally
                {
                    bufferState.Dispose();
                }
            }

            static KdlTypeInfo<List<T?>> GetOrAddListTypeInfoForArrayMode(KdlTypeInfo<T> elementTypeInfo)
            {
                if (elementTypeInfo._asyncEnumerableArrayTypeInfo != null)
                {
                    return (KdlTypeInfo<List<T?>>)elementTypeInfo._asyncEnumerableArrayTypeInfo;
                }

                var converter = new ListOfTConverter<List<T>, T>();
                var listTypeInfo = new KdlTypeInfo<List<T?>>(converter, elementTypeInfo.Options)
                {
                    CreateObject = static () => [],
                    ElementTypeInfo = elementTypeInfo,
                };

                listTypeInfo.EnsureConfigured();
                elementTypeInfo._asyncEnumerableArrayTypeInfo = listTypeInfo;
                return listTypeInfo;
            }

            static KdlTypeInfo<List<T?>> GetOrAddListTypeInfoForRootLevelValueMode(KdlTypeInfo<T> elementTypeInfo)
            {
                if (elementTypeInfo._asyncEnumerableRootLevelValueTypeInfo != null)
                {
                    return (KdlTypeInfo<List<T?>>)elementTypeInfo._asyncEnumerableRootLevelValueTypeInfo;
                }

                var converter = new RootLevelListConverter<T>(elementTypeInfo);
                var listTypeInfo = new KdlTypeInfo<List<T?>>(converter, elementTypeInfo.Options)
                {
                    ElementTypeInfo = elementTypeInfo,
                };

                listTypeInfo.EnsureConfigured();
                elementTypeInfo._asyncEnumerableRootLevelValueTypeInfo = listTypeInfo;
                return listTypeInfo;
            }
        }
    }
}
