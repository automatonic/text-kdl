// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the provided value into a <see cref="KdlDocument"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="KdlDocument"/> representation of the KDL value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static KdlDocument SerializeToDocument<TValue>(TValue value, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return WriteDocument(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlDocument"/>.
        /// </summary>
        /// <returns>A <see cref="KdlDocument"/> representation of the value.</returns>
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
        public static KdlDocument SerializeToDocument(object? value, Type inputType, KdlSerializerOptions? options = null)
        {
            ValidateInputType(value, inputType);
            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, inputType);
            return WriteDocumentAsObject(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlDocument"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="KdlDocument"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static KdlDocument SerializeToDocument<TValue>(TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return WriteDocument(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlDocument"/>.
        /// </summary>
        /// <returns>A <see cref="KdlDocument"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> does not match the type of <paramref name="jsonTypeInfo"/>.
        /// </exception>
        public static KdlDocument SerializeToDocument(object? value, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return WriteDocumentAsObject(value, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the provided value into a <see cref="KdlDocument"/>.
        /// </summary>
        /// <returns>A <see cref="KdlDocument"/> representation of the value.</returns>
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
        public static KdlDocument SerializeToDocument(object? value, Type inputType, KdlSerializerContext context)
        {
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            ValidateInputType(value, inputType);
            return WriteDocumentAsObject(value, GetTypeInfo(context, inputType));
        }

        private static KdlDocument WriteDocument<TValue>(in TValue value, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            KdlSerializerOptions options = jsonTypeInfo.Options;

            // For performance, share the same buffer across serialization and deserialization.
            // The PooledByteBufferWriter is cleared and returned when KdlDocument.Dispose() is called.
            PooledByteBufferWriter output = new(options.DefaultBufferSize);
            KdlWriter writer = KdlWriterCache.RentWriter(options, output);

            try
            {
                jsonTypeInfo.Serialize(writer, value);
                return KdlDocument.ParseRented(output, options.GetDocumentOptions());
            }
            finally
            {
                KdlWriterCache.ReturnWriter(writer);
            }
        }

        private static KdlDocument WriteDocumentAsObject(object? value, KdlTypeInfo jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            KdlSerializerOptions options = jsonTypeInfo.Options;

            // For performance, share the same buffer across serialization and deserialization.
            // The PooledByteBufferWriter is cleared and returned when KdlDocument.Dispose() is called.
            PooledByteBufferWriter output = new(options.DefaultBufferSize);
            KdlWriter writer = KdlWriterCache.RentWriter(options, output);

            try
            {
                jsonTypeInfo.SerializeAsObject(writer, value);
                return KdlDocument.ParseRented(output, options.GetDocumentOptions());
            }
            finally
            {
                KdlWriterCache.ReturnWriter(writer);
            }
        }
    }
}
