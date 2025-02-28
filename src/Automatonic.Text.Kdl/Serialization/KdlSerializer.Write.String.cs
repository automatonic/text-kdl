using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8. See also <see cref="SerializeToUtf8Bytes{TValue}(TValue, KdlSerializerOptions?)"/>
        /// and <see cref="SerializeAsync{TValue}(IO.Stream, TValue, KdlSerializerOptions?, Threading.CancellationToken)"/>.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static string Serialize<TValue>(TValue value, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return WriteString(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inputType"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8. See also <see cref="SerializeToUtf8Bytes(object?, Type, KdlSerializerOptions?)"/>
        /// and <see cref="SerializeAsync(IO.Stream, object?, Type, KdlSerializerOptions?, Threading.CancellationToken)"/>.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static string Serialize(
            object? value,
            Type inputType,
            KdlSerializerOptions? options = null)
        {
            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, inputType);
            return WriteStringAsObject(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8. See also <see cref="SerializeToUtf8Bytes{TValue}(TValue, KdlTypeInfo{TValue})"/>
        /// and <see cref="SerializeAsync{TValue}(IO.Stream, TValue, KdlTypeInfo{TValue}, Threading.CancellationToken)"/>.
        /// </remarks>
        public static string Serialize<TValue>(TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return WriteString(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="jsonTypeInfo"/>.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8. See also <see cref="SerializeToUtf8Bytes(object?, KdlTypeInfo)"/>
        /// and <see cref="SerializeAsync(IO.Stream, object?, KdlTypeInfo, Threading.CancellationToken)"/>.
        /// </remarks>
        public static string Serialize(object? value, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return WriteStringAsObject(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method of the provided
        /// <paramref name="context"/> returns <see langword="null"/> for the type to convert.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inputType"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8. See also <see cref="SerializeToUtf8Bytes(object?, Type, KdlSerializerContext)"/>
        /// and <see cref="SerializeAsync(IO.Stream, object?, Type, KdlSerializerContext, Threading.CancellationToken)"/>.
        /// </remarks>
        public static string Serialize(object? value, Type inputType, KdlSerializerContext context)
        {
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(context, inputType);
            return WriteStringAsObject(value, jsonTypeInfo);
        }

        private static string WriteString<TValue>(in TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);

            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(jsonTypeInfo.Options, out PooledByteBufferWriter output);

            try
            {
                jsonTypeInfo.Serialize(writer, value);
                return KdlReaderHelper.TranscodeHelper(output.WrittenMemory.Span);
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }

        private static string WriteStringAsObject(object? value, KdlTypeInfo jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);

            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(jsonTypeInfo.Options, out PooledByteBufferWriter output);

            try
            {
                jsonTypeInfo.SerializeAsObject(writer, value);
                return KdlReaderHelper.TranscodeHelper(output.WrittenMemory.Span);
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }
    }
}
