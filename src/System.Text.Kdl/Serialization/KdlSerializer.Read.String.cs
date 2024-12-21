using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    /// <summary>
    /// Provides functionality to serialize objects or value types to KDL and
    /// deserialize KDL into objects or value types.
    /// </summary>
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Parses the text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string kdl, KdlSerializerOptions? options = null)
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }

            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return ReadFromSpan(kdl.AsSpan(), jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into an instance of the type specified by a generic type parameter.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="kdl">The KDL text to parse.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the span beyond a single KDL value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        /// <remarks>Using a UTF-16 span is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> kdl, KdlSerializerOptions? options = null)
        {
            KdlTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return ReadFromSpan(kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> or <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <paramref name="returnType"/> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] string kdl, Type returnType, KdlSerializerOptions? options = null)
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, returnType);
            return ReadFromSpanAsObject(kdl.AsSpan(), jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into an instance of a specified type.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="kdl">The KDL text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <paramref name="returnType"/> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the span beyond a single KDL value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <remarks>Using a UTF-16 span is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> kdl, Type returnType, KdlSerializerOptions? options = null)
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            // default/null span is treated as empty

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(options, returnType);
            return ReadFromSpanAsObject(kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        public static TValue? Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string kdl, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadFromSpan(kdl.AsSpan(), jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// <typeparamref name="TValue" /> is not compatible with the KDL.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        public static TValue? Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> kdl, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadFromSpan(kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into an instance specified by the <paramref name="jsonTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="jsonTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> is <see langword="null"/>.
        ///
        /// -or-
        ///
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        public static object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] string kdl, KdlTypeInfo jsonTypeInfo)
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadFromSpanAsObject(kdl.AsSpan(), jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into an instance specified by the <paramref name="jsonTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="jsonTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="jsonTypeInfo"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single KDL value.</exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        public static object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> kdl, KdlTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            jsonTypeInfo.EnsureConfigured();
            return ReadFromSpanAsObject(kdl, jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> or <paramref name="returnType"/> is <see langword="null"/>.
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
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        public static object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] string kdl, Type returnType, KdlSerializerContext context)
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }
            if (context is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(context));
            }

            KdlTypeInfo jsonTypeInfo = GetTypeInfo(context, returnType);
            return ReadFromSpanAsObject(kdl.AsSpan(), jsonTypeInfo);
        }

        /// <summary>
        /// Parses the text representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="kdl"/> or <paramref name="returnType"/> is <see langword="null"/>.
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
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        public static object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> kdl, Type returnType, KdlSerializerContext context)
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
            return ReadFromSpanAsObject(kdl, jsonTypeInfo);
        }

        private static TValue? ReadFromSpan<TValue>(ReadOnlySpan<char> kdl, KdlTypeInfo<TValue> jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            byte[]? tempArray = null;

            // For performance, avoid obtaining actual byte count unless memory usage is higher than the threshold.
            Span<byte> utf8 =
                // Use stack memory
                kdl.Length <= (KdlConstants.StackallocByteThreshold / KdlConstants.MaxExpansionFactorWhileTranscoding) ? stackalloc byte[KdlConstants.StackallocByteThreshold] :
                // Use a pooled array
                kdl.Length <= (KdlConstants.ArrayPoolMaxSizeBeforeUsingNormalAlloc / KdlConstants.MaxExpansionFactorWhileTranscoding) ? tempArray = ArrayPool<byte>.Shared.Rent(kdl.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) :
                // Use a normal alloc since the pool would create a normal alloc anyway based on the threshold (per current implementation)
                // and by using a normal alloc we can avoid the Clear().
                new byte[KdlReaderHelper.GetUtf8ByteCount(kdl)];

            try
            {
                int actualByteCount = KdlReaderHelper.GetUtf8FromText(kdl, utf8);
                utf8 = utf8[..actualByteCount];
                return ReadFromSpan(utf8, jsonTypeInfo, actualByteCount);
            }
            finally
            {
                if (tempArray != null)
                {
                    utf8.Clear();
                    ArrayPool<byte>.Shared.Return(tempArray);
                }
            }
        }

        private static object? ReadFromSpanAsObject(ReadOnlySpan<char> kdl, KdlTypeInfo jsonTypeInfo)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            byte[]? tempArray = null;

            // For performance, avoid obtaining actual byte count unless memory usage is higher than the threshold.
            Span<byte> utf8 =
                // Use stack memory
                kdl.Length <= (KdlConstants.StackallocByteThreshold / KdlConstants.MaxExpansionFactorWhileTranscoding) ? stackalloc byte[KdlConstants.StackallocByteThreshold] :
                // Use a pooled array
                kdl.Length <= (KdlConstants.ArrayPoolMaxSizeBeforeUsingNormalAlloc / KdlConstants.MaxExpansionFactorWhileTranscoding) ? tempArray = ArrayPool<byte>.Shared.Rent(kdl.Length * KdlConstants.MaxExpansionFactorWhileTranscoding) :
                // Use a normal alloc since the pool would create a normal alloc anyway based on the threshold (per current implementation)
                // and by using a normal alloc we can avoid the Clear().
                new byte[KdlReaderHelper.GetUtf8ByteCount(kdl)];

            try
            {
                int actualByteCount = KdlReaderHelper.GetUtf8FromText(kdl, utf8);
                utf8 = utf8[..actualByteCount];
                return ReadFromSpanAsObject(utf8, jsonTypeInfo, actualByteCount);
            }
            finally
            {
                if (tempArray != null)
                {
                    utf8.Clear();
                    ArrayPool<byte>.Shared.Return(tempArray);
                }
            }
        }
    }
}
