using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        // We flush the Stream when the buffer is >=90% of capacity.
        // This threshold is a compromise between buffer utilization and minimizing cases where the buffer
        // needs to be expanded\doubled because it is not large enough to write the current property or element.
        // We check for flush after each KDL property and element is written to the buffer.
        // Once the buffer is expanded to contain the largest single element\property, a 90% threshold
        // means the buffer may be expanded a maximum of 4 times: 1-(1/(2^4))==.9375.
        internal const float FlushThreshold = .90f;

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the write operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static Task SerializeAsync<TValue>(
            Stream utf8Kdl,
            TValue value,
            KdlSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return jsonTypeInfo.SerializeAsync(utf8Kdl, value, cancellationToken);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static void Serialize<TValue>(
            Stream utf8Kdl,
            TValue value,
            KdlSerializerOptions? options = null)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            jsonTypeInfo.Serialize(utf8Kdl, value);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the write operation.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="inputType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static Task SerializeAsync(
            Stream utf8Kdl,
            object? value,
            Type inputType,
            KdlSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, inputType);
            return jsonTypeInfo.SerializeAsObjectAsync(utf8Kdl, value, cancellationToken);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> or <paramref name="inputType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static void Serialize(
            Stream utf8Kdl,
            object? value,
            Type inputType,
            KdlSerializerOptions? options = null)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, inputType);
            jsonTypeInfo.SerializeAsObject(utf8Kdl, value);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the write operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        public static Task SerializeAsync<TValue>(
            Stream utf8Kdl,
            TValue value,
            KdlTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return jsonTypeInfo.SerializeAsync(utf8Kdl, value, cancellationToken);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        public static void Serialize<TValue>(
            Stream utf8Kdl,
            TValue value,
            KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            jsonTypeInfo.Serialize(utf8Kdl, value);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the write operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="jsonTypeInfo"/>.
        /// </exception>
        public static Task SerializeAsync(
            Stream utf8Kdl,
            object? value,
            KdlTypeInfo jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return jsonTypeInfo.SerializeAsObjectAsync(utf8Kdl, value, cancellationToken);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="jsonTypeInfo"/>.
        /// </exception>
        public static void Serialize(
            Stream utf8Kdl,
            object? value,
            KdlTypeInfo jsonTypeInfo)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            jsonTypeInfo.SerializeAsObject(utf8Kdl, value);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> that can be used to cancel the write operation.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/>, <paramref name="inputType"/>, or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        public static Task SerializeAsync(
            Stream utf8Kdl,
            object? value,
            Type inputType,
            KdlSerializerContext context,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(context, inputType);
            return jsonTypeInfo.SerializeAsObjectAsync(utf8Kdl, value, cancellationToken);
        }

        /// <summary>
        /// Converts the provided value to UTF-8 encoded KDL text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="utf8Kdl">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="utf8Kdl"/>, <paramref name="inputType"/>, or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        public static void Serialize(
            Stream utf8Kdl,
            object? value,
            Type inputType,
            KdlSerializerContext context)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(context, inputType);
            jsonTypeInfo.SerializeAsObject(utf8Kdl, value);
        }
    }
}
