using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        /// <summary>
        /// Parses the UTF-8 encoded text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <typeparamref name="TValue"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static TValue? Deserialize<TValue>(
            ReadOnlySpan<byte> utf8Kdl,
            KdlSerializerOptions? options = null
        )
        {
            KdlTypeInfo<TValue> kdlTypeInfo = GetTypeInfo<TValue>(options);
            return ReadFromSpan(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Parses the UTF-8 encoded text representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <paramref name="returnType"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
        public static object? Deserialize(
            ReadOnlySpan<byte> utf8Kdl,
            Type returnType,
            KdlSerializerOptions? options = null
        )
        {
            if (returnType is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(returnType));
            }

            KdlTypeInfo kdlTypeInfo = GetTypeInfo(options, returnType);
            return ReadFromSpanAsObject(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Parses the UTF-8 encoded text representing a single KDL value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the KDL value into.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the KDL,
        /// or when there is remaining data in the buffer.
        /// </exception>
        public static TValue? Deserialize<TValue>(
            ReadOnlySpan<byte> utf8Kdl,
            KdlTypeInfo<TValue> kdlTypeInfo
        )
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return ReadFromSpan(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Parses the UTF-8 encoded text representing a single KDL value into an instance specified by the <paramref name="kdlTypeInfo"/>.
        /// </summary>
        /// <returns>A <paramref name="kdlTypeInfo"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="kdlTypeInfo">Metadata about the type to convert.</param>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// or there is remaining data in the buffer.
        /// </exception>
        public static object? Deserialize(ReadOnlySpan<byte> utf8Kdl, KdlTypeInfo kdlTypeInfo)
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            kdlTypeInfo.EnsureConfigured();
            return ReadFromSpanAsObject(utf8Kdl, kdlTypeInfo);
        }

        /// <summary>
        /// Parses the UTF-8 encoded text representing a single KDL value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the KDL value.</returns>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="context">A metadata provider for serializable types.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        /// The KDL is invalid,
        /// <paramref name="returnType"/> is not compatible with the KDL,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="Automatonic.Text.Kdl.Serialization.KdlConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlSerializerContext.GetTypeInfo(Type)"/> method on the provided <paramref name="context"/>
        /// did not return a compatible <see cref="KdlTypeInfo"/> for <paramref name="returnType"/>.
        /// </exception>
        public static object? Deserialize(
            ReadOnlySpan<byte> utf8Kdl,
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

            return ReadFromSpanAsObject(utf8Kdl, GetTypeInfo(context, returnType));
        }

        private static TValue? ReadFromSpan<TValue>(
            ReadOnlySpan<byte> utf8Kdl,
            KdlTypeInfo<TValue> kdlTypeInfo,
            int? actualByteCount = null
        )
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);

            var readerState = new KdlReaderState(kdlTypeInfo.Options.GetReaderOptions());
            var reader = new KdlReader(utf8Kdl, isFinalBlock: true, readerState);

            ReadStack state = default;
            state.Initialize(kdlTypeInfo);

            TValue? value = kdlTypeInfo.Deserialize(ref reader, ref state);
            return value;
        }

        private static object? ReadFromSpanAsObject(
            ReadOnlySpan<byte> utf8Kdl,
            KdlTypeInfo kdlTypeInfo,
            int? actualByteCount = null
        )
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);

            var readerState = new KdlReaderState(kdlTypeInfo.Options.GetReaderOptions());
            var reader = new KdlReader(utf8Kdl, isFinalBlock: true, readerState);

            ReadStack state = default;
            state.Initialize(kdlTypeInfo);

            object? value = kdlTypeInfo.DeserializeAsObject(ref reader, ref state);
            return value;
        }
    }
}
