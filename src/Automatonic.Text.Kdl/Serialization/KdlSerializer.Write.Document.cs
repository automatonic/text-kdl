using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the provided value into a <see cref="KdlReadOnlyDocument"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="KdlReadOnlyDocument"/> representation of the KDL value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static KdlReadOnlyDocument SerializeToDocument<TValue>(TValue value, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            return WriteDocument(value, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlReadOnlyDocument"/>.
        /// </summary>
        /// <returns>A <see cref="KdlReadOnlyDocument"/> representation of the value.</returns>
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
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="inputType"/>  or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static KdlReadOnlyDocument SerializeToDocument(object? value, Type inputType, KdlSerializerOptions? options = null)
        {
            ValidateInputType(value, inputType);
            KdlTypeInfo kdlTypeInfo = GetTypeInfo(options, inputType);
            return WriteDocumentAsObject(value, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlReadOnlyDocument"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="KdlReadOnlyDocument"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static KdlReadOnlyDocument SerializeToDocument<TValue>(TValue value, KdlTypeInfo<TValue> kdlTypeInfo)
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return WriteDocument(value, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlReadOnlyDocument"/>.
        /// </summary>
        /// <returns>A <see cref="KdlReadOnlyDocument"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="kdlTypeInfo"/>.
        /// </exception>
        public static KdlReadOnlyDocument SerializeToDocument(object? value, KdlTypeInfo kdlTypeInfo)
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return WriteDocumentAsObject(value, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlReadOnlyDocument"/>.
        /// </summary>
        /// <returns>A <see cref="KdlReadOnlyDocument"/> representation of the value.</returns>
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
        public static KdlReadOnlyDocument SerializeToDocument(object? value, Type inputType, KdlSerializerContext context)
        {
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            return WriteDocumentAsObject(value, GetTypeInfo(context, inputType));
        }

        private static KdlReadOnlyDocument WriteDocument<TValue>(in TValue value, KdlTypeInfo<TValue> kdlTypeInfo)
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);
            KdlSerializerOptions options = kdlTypeInfo.Options;

            // For performance, share the same buffer across serialization and deserialization.
            // The PooledByteBufferWriter is cleared and returned when KdlDocument.Dispose() is called.
            PooledByteBufferWriter output = new(options.DefaultBufferSize);
            KdlWriter writer = KdlWriterCache.RentWriter(options, output);

            try
            {
                kdlTypeInfo.Serialize(writer, value);
                return KdlReadOnlyDocument.ParseRented(output, options.GetDocumentOptions());
            }
            finally
            {
                KdlWriterCache.ReturnWriter(writer);
            }
        }

        private static KdlReadOnlyDocument WriteDocumentAsObject(object? value, KdlTypeInfo kdlTypeInfo)
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);
            KdlSerializerOptions options = kdlTypeInfo.Options;

            // For performance, share the same buffer across serialization and deserialization.
            // The PooledByteBufferWriter is cleared and returned when KdlDocument.Dispose() is called.
            PooledByteBufferWriter output = new(options.DefaultBufferSize);
            KdlWriter writer = KdlWriterCache.RentWriter(options, output);

            try
            {
                kdlTypeInfo.SerializeAsObject(writer, value);
                return KdlReadOnlyDocument.ParseRented(output, options.GetDocumentOptions());
            }
            finally
            {
                KdlWriterCache.ReturnWriter(writer);
            }
        }
    }
}
