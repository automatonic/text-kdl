using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the <see cref="KdlReadOnlyDocument"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="document">The <see cref="KdlReadOnlyDocument"/> to convert.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>(this KdlReadOnlyDocument document, KdlSerializerOptions? options = null)
        {
            if (document is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(document));
            }

            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            ReadOnlySpan<byte> utf8Kdl = document.GetRootRawValue().Span;
            return ReadFromSpan(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyDocument"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="document">The <see cref="KdlReadOnlyDocument"/> to convert.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="document"/> or <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <paramref name="returnType"/> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(this KdlReadOnlyDocument document, Type returnType, KdlSerializerOptions? options = null)
        {
            if (document is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(document));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(options, returnType);
            ReadOnlySpan<byte> utf8Kdl = document.GetRootRawValue().Span;
            return ReadFromSpanAsObject(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyDocument"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="document">The <see cref="KdlReadOnlyDocument"/> to convert.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
        /// <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        public static TValue? Deserialize<TValue>(this KdlReadOnlyDocument document, KdlTypeInfo<TValue> kdlTypeInfo)
        {
            if (document is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(document));
            }
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            ReadOnlySpan<byte> utf8Kdl = document.GetRootRawValue().Span;
            return ReadFromSpan(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyDocument"/> representing a single KDL value into an instance specified by the <paramref name="kdlTypeInfo"/>.
        /// </summary>
        /// <param name="document">The <see cref="KdlReadOnlyDocument"/> to convert.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <returns>A <paramref name="kdlTypeInfo"/> representation of the KDL value.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
        /// <paramref name="kdlTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static object? Deserialize(this KdlReadOnlyDocument document, KdlTypeInfo kdlTypeInfo)
        {
            if (document is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(document));
            }
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            ReadOnlySpan<byte> utf8Kdl = document.GetRootRawValue().Span;
            return ReadFromSpanAsObject(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyDocument"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="document">The <see cref="KdlReadOnlyDocument"/> to convert.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
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
        public static object? Deserialize(this KdlReadOnlyDocument document, Type returnType, KdlSerializerContext context)
        {
            if (document is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(document));
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
            ReadOnlySpan<byte> utf8Kdl = document.GetRootRawValue().Span;
            return ReadFromSpanAsObject(utf8Kdl, kdlTypeInfo);
        }
    }
}
