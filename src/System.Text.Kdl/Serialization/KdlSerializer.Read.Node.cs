using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the <see cref="KdlVertex"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlVertex"/> to convert.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>(this KdlVertex? node, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return ReadFromNode(node, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlVertex"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlVertex"/> to convert.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="KdlException">
        /// <paramref name="returnType"/> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(this KdlVertex? node, Type returnType, KdlSerializerOptions? options = null)
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, returnType);
            return ReadFromNodeAsObject(node, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlVertex"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlVertex"/> to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        public static TValue? Deserialize<TValue>(this KdlVertex? node, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadFromNode(node, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlVertex"/> representing a single KDL value into an instance specified by the <paramref name="jsonTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="jsonTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlVertex"/> to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static object? Deserialize(this KdlVertex? node, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadFromNodeAsObject(node, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlVertex"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlVertex"/> to convert.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="returnType"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
        /// <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <paramref name="returnType" /> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method of the provided
        /// <paramref name="context"/> returns <see langword="null"/> for the type to convert.
        /// </exception>
        public static object? Deserialize(this KdlVertex? node, Type returnType, KdlSerializerContext context)
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(context, returnType);
            return ReadFromNodeAsObject(node, jsonTypeInfo);
        }

        private static TValue? ReadFromNode<TValue>(KdlVertex? node, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            KdlSerializerOptions options = jsonTypeInfo.Options;

            // For performance, share the same buffer across serialization and deserialization.
            using var output = new PooledByteBufferWriter(options.DefaultBufferSize);
            using (var writer = new KdlWriter(output, options.GetWriterOptions()))
            {
                if (node is null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    node.WriteTo(writer, options);
                }
            }

            return ReadFromSpan(output.WrittenMemory.Span, jsonTypeInfo);
        }

        private static object? ReadFromNodeAsObject(KdlVertex? node, KdlTypeInfo jsonTypeInfo)
        {
            KdlSerializerOptions options = jsonTypeInfo.Options;

            // For performance, share the same buffer across serialization and deserialization.
            using var output = new PooledByteBufferWriter(options.DefaultBufferSize);
            using (var writer = new KdlWriter(output, options.GetWriterOptions()))
            {
                if (node is null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    node.WriteTo(writer, options);
                }
            }

            return ReadFromSpanAsObject(output.WrittenMemory.Span, jsonTypeInfo);
        }
    }
}
