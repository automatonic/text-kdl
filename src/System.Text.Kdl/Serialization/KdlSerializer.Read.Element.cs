using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.RandomAccess;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Converts the <see cref="KdlReadOnlyElement"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="element">The <see cref="KdlReadOnlyElement"/> to convert.</param>
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
        public static TValue? Deserialize<TValue>(this KdlReadOnlyElement element, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            ReadOnlySpan<byte> utf8Kdl = element.GetRawValue().Span;
            return ReadFromSpan(utf8Kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyElement"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="element">The <see cref="KdlReadOnlyElement"/> to convert.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <paramref name="returnType"/> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(this KdlReadOnlyElement element, Type returnType, KdlSerializerOptions? options = null)
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, returnType);
            ReadOnlySpan<byte> utf8Kdl = element.GetRawValue().Span;
            return ReadFromSpanAsObject(utf8Kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyElement"/> representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="element">The <see cref="KdlReadOnlyElement"/> to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        public static TValue? Deserialize<TValue>(this KdlReadOnlyElement element, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            ReadOnlySpan<byte> utf8Kdl = element.GetRawValue().Span;
            return ReadFromSpan(utf8Kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyElement"/> representing a single KDL value into an instance specified by the <paramref name="jsonTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="jsonTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="element">The <see cref="KdlReadOnlyElement"/> to convert.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        public static object? Deserialize(this KdlReadOnlyElement element, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            ReadOnlySpan<byte> utf8Kdl = element.GetRawValue().Span;
            return ReadFromSpanAsObject(utf8Kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Converts the <see cref="KdlReadOnlyElement"/> representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="element">The <see cref="KdlReadOnlyElement"/> to convert.</param>
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
        public static object? Deserialize(this KdlReadOnlyElement element, Type returnType, KdlSerializerContext context)
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
            ReadOnlySpan<byte> utf8Kdl = element.GetRawValue().Span;
            return ReadFromSpanAsObject(utf8Kdl, jsonTypeInfo);
        }
    }
}
