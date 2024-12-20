using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Writes one KDL value (including objects or arrays) to the provided writer.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="writer">The writer to write.</param>
        /// <param name="value">The value to convert and write.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static void Serialize<TValue>(
            KdlWriter writer,
            TValue value,
            KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            jsonTypeInfo.Serialize(writer, value);
        }

        /// <summary>
        /// Writes one KDL value (including objects or arrays) to the provided writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value">The value to convert and write.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="writer"/> or <paramref name="inputType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static void Serialize(
            KdlWriter writer,
            object? value,
            Type inputType,
            KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, inputType);
            jsonTypeInfo.SerializeAsObject(writer, value);
        }

        /// <summary>
        /// Writes one KDL value (including objects or arrays) to the provided writer.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="writer">The writer to write.</param>
        /// <param name="value">The value to convert and write.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> or <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static void Serialize<TValue>(KdlWriter writer, TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            jsonTypeInfo.Serialize(writer, value);
        }

        /// <summary>
        /// Writes one KDL value (including objects or arrays) to the provided writer.
        /// </summary>
        /// <param name="writer">The writer to write.</param>
        /// <param name="value">The value to convert and write.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> or <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="jsonTypeInfo"/>.
        /// </exception>
        public static void Serialize(KdlWriter writer, object? value, KdlTypeInfo jsonTypeInfo)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            jsonTypeInfo.SerializeAsObject(writer, value);
        }

        /// <summary>
        /// Writes one KDL value (including objects or arrays) to the provided writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value">The value to convert and write.</param>
        /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="inputType"/> is not compatible with <paramref name="value"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="writer"/> or <paramref name="inputType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method of the provided
        /// <paramref name="context"/> returns <see langword="null"/> for the type to convert.
        /// </exception>
        public static void Serialize(KdlWriter writer, object? value, Type inputType, KdlSerializerContext context)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(context, inputType);
            jsonTypeInfo.SerializeAsObject(writer, value);
        }
    }
}
