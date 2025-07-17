using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the <see cref="KdlElement"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlElement"/> to convert.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>(
            this KdlElement? node,
            KdlSerializerOptions? options = null
        )
        {
            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            return ReadFromNode(node, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlElement"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlElement"/> to convert.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="KdlException">
        /// <paramref name="returnType"/> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(
            this KdlElement? node,
            Type returnType,
            KdlSerializerOptions? options = null
        )
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(options, returnType);
            return ReadFromNodeAsObject(node, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlElement"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlElement"/> to convert.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        public static TValue? Deserialize<TValue>(
            this KdlElement? node,
            KdlTypeInfo<TValue> kdlTypeInfo
        )
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return ReadFromNode(node, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlElement"/> representing a single KDL value into an instance specified by the <paramref name="kdlTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="kdlTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlElement"/> to convert.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static object? Deserialize(this KdlElement? node, KdlTypeInfo kdlTypeInfo)
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return ReadFromNodeAsObject(node, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlElement"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="node">The <see cref="KdlElement"/> to convert.</param>
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
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method of the provided
        /// <paramref name="context"/> returns <see langword="null"/> for the type to convert.
        /// </exception>
        public static object? Deserialize(
            this KdlElement? node,
            Type returnType,
            KdlSerializerContext context
        )
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(context, returnType);
            return ReadFromNodeAsObject(node, kdlTypeInfo);
        }

        private static TValue? ReadFromNode<TValue>(
            KdlElement? node,
            KdlTypeInfo<TValue> kdlTypeInfo
        )
        {
            KdlSerializerOptions options = kdlTypeInfo.Options;

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

            return ReadFromSpan(output.WrittenMemory.Span, kdlTypeInfo);
        }

        private static object? ReadFromNodeAsObject(KdlElement? node, KdlTypeInfo kdlTypeInfo)
        {
            KdlSerializerOptions options = kdlTypeInfo.Options;

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

            return ReadFromSpanAsObject(output.WrittenMemory.Span, kdlTypeInfo);
        }
    }
}
