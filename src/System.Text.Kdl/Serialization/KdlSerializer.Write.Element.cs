using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the provided value into a <see cref="KdlElement"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="KdlElement"/> representation of the KDL value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static KdlElement SerializeToElement<TValue>(TValue value, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return WriteElement(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlElement"/>.
        /// </summary>
        /// <returns>A <see cref="KdlElement"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inputType"/> is <see langword="null"/>.
        /// </exception>
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static KdlElement SerializeToElement(object? value, Type inputType, KdlSerializerOptions? options = null)
        {
            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, inputType);
            return WriteElementAsObject(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlElement"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="KdlElement"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static KdlElement SerializeToElement<TValue>(TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return WriteElement(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlElement"/>.
        /// </summary>
        /// <returns>A <see cref="KdlElement"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="jsonTypeInfo"/>.
        /// </exception>
        public static KdlElement SerializeToElement(object? value, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return WriteElementAsObject(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlElement"/>.
        /// </summary>
        /// <returns>A <see cref="KdlElement"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method of the provided
        /// <paramref name="context"/> returns <see langword="null"/> for the type to convert.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inputType"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public static KdlElement SerializeToElement(object? value, Type inputType, KdlSerializerContext context)
        {
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo typeInfo = GetTypeInfo(context, inputType);
            return WriteElementAsObject(value, typeInfo);
        }

        private static KdlElement WriteElement<TValue>(in TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            KdlSerializerOptions options = jsonTypeInfo.Options;

            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(jsonTypeInfo.Options, out PooledByteBufferWriter output);

            try
            {
                jsonTypeInfo.Serialize(writer, value);
                return KdlElement.ParseValue(output.WrittenMemory.Span, options.GetDocumentOptions());
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }

        private static KdlElement WriteElementAsObject(object? value, KdlTypeInfo jsonTypeInfo)
        {
            KdlSerializerOptions options = jsonTypeInfo.Options;
            Debug.Assert(options != null);

            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(jsonTypeInfo.Options, out PooledByteBufferWriter output);

            try
            {
                jsonTypeInfo.SerializeAsObject(writer, value);
                return KdlElement.ParseValue(output.WrittenMemory.Span, options.GetDocumentOptions());
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }
    }
}
