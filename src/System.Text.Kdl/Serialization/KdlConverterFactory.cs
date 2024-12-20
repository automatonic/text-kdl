// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Supports converting several types by using a factory pattern.
    /// </summary>
    /// <remarks>
    /// This is useful for converters supporting generics, such as a converter for <see cref="System.Collections.Generic.List{T}"/>.
    /// </remarks>
    public abstract class KdlConverterFactory : KdlConverter
    {
        /// <summary>
        /// When overridden, constructs a new <see cref="KdlConverterFactory"/> instance.
        /// </summary>
        protected KdlConverterFactory() { }

        private protected override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.None;

        /// <summary>
        /// Create a converter for the provided <see cref="System.Type"/>.
        /// </summary>
        /// <param name="typeToConvert">The <see cref="System.Type"/> being converted.</param>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> being used.</param>
        /// <returns>
        /// An instance of a <see cref="KdlConverter{T}"/> where T is compatible with <paramref name="typeToConvert"/>.
        /// If <see langword="null"/> is returned, a <see cref="NotSupportedException"/> will be thrown.
        /// </returns>
        public abstract KdlConverter? CreateConverter(Type typeToConvert, KdlSerializerOptions options);

        internal KdlConverter GetConverterInternal(Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Assert(CanConvert(typeToConvert));

            KdlConverter? converter = CreateConverter(typeToConvert, options);
            switch (converter)
            {
                case null:
                    ThrowHelper.ThrowInvalidOperationException_SerializerConverterFactoryReturnsNull(GetType());
                    break;
                case KdlConverterFactory:
                    ThrowHelper.ThrowInvalidOperationException_SerializerConverterFactoryReturnsKdlConverterFactorty(GetType());
                    break;
            }

            return converter;
        }

        internal sealed override object? ReadAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override bool OnTryReadAsObject(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out object? value)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override bool TryReadAsObject(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out object? value)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override object? ReadAsPropertyNameAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override object? ReadAsPropertyNameCoreAsObject(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override object? ReadNumberWithCustomHandlingAsObject(ref KdlReader reader, KdlNumberHandling handling, KdlSerializerOptions options)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override void WriteAsObject(KdlWriter writer, object? value, KdlSerializerOptions options)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override bool OnTryWriteAsObject(
            KdlWriter writer,
            object? value,
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override bool TryWriteAsObject(
            KdlWriter writer,
            object? value,
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override void WriteAsPropertyNameAsObject(KdlWriter writer, object? value, KdlSerializerOptions options)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        /// <inheritdoc/>
        public sealed override Type? Type => null;

        internal sealed override void WriteAsPropertyNameCoreAsObject(
            KdlWriter writer,
            object? value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override void WriteNumberWithCustomHandlingAsObject(KdlWriter writer, object? value, KdlNumberHandling handling)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }
    }
}
