// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    /// Represents a mutable KDL value.
    /// </summary>
    public abstract partial class KdlValue : KdlNode
    {
        internal const string CreateUnreferencedCodeMessage = "Creating KdlValue instances with non-primitive types is not compatible with trimming. It can result in non-primitive types being serialized, which may have their members trimmed.";
        internal const string CreateDynamicCodeMessage = "Creating KdlValue instances with non-primitive types requires generating code at runtime.";

        private protected KdlValue(KdlNodeOptions? options) : base(options) { }

        /// <summary>
        ///   Tries to obtain the current KDL value and returns a value that indicates whether the operation succeeded.
        /// </summary>
        /// <remarks>
        ///   {T} can be the type or base type of the underlying value.
        ///   If the underlying value is a <see cref="KdlElement"/> then {T} can also be the type of any primitive
        ///   value supported by current <see cref="KdlElement"/>.
        ///   Specifying the <see cref="object"/> type for {T} will always succeed and return the underlying value as <see cref="object"/>.<br />
        ///   The underlying value of a <see cref="KdlValue"/> after deserialization is an instance of <see cref="KdlElement"/>,
        ///   otherwise it's the value specified when the <see cref="KdlValue"/> was created.
        /// </remarks>
        /// <seealso cref="KdlNode.GetValue{T}"></seealso>
        /// <typeparam name="T">The type of value to obtain.</typeparam>
        /// <param name="value">When this method returns, contains the parsed value.</param>
        /// <returns><see langword="true"/> if the value can be successfully obtained; otherwise, <see langword="false"/>.</returns>
        public abstract bool TryGetValue<T>([NotNullWhen(true)] out T? value);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </returns>
        /// <typeparam name="T">The type of value to create.</typeparam>
        /// <param name="value">The value to create.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [RequiresUnreferencedCode(CreateUnreferencedCodeMessage + " Use the overload that takes a KdlTypeInfo, or make sure all of the required types are preserved.")]
        [RequiresDynamicCode(CreateDynamicCodeMessage)]
        public static KdlValue? Create<T>(T? value, KdlNodeOptions? options = null)
        {
            if (value is null)
            {
                return null;
            }

            if (value is KdlNode)
            {
                ThrowHelper.ThrowArgumentException_NodeValueNotAllowed(nameof(value));
            }

            if (value is KdlElement element)
            {
                return CreateFromElement(ref element, options);
            }

            var jsonTypeInfo = (KdlTypeInfo<T>)KdlSerializerOptions.Default.GetTypeInfo(typeof(T));
            return CreateFromTypeInfo(value, jsonTypeInfo, options);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </returns>
        /// <typeparam name="T">The type of value to create.</typeparam>
        /// <param name="value">The value to create.</param>
        /// <param name="jsonTypeInfo">The <see cref="KdlTypeInfo"/> that will be used to serialize the value.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create<T>(T? value, KdlTypeInfo<T> jsonTypeInfo, KdlNodeOptions? options = null)
        {
            if (jsonTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(jsonTypeInfo));
            }

            if (value is null)
            {
                return null;
            }

            if (value is KdlNode)
            {
                ThrowHelper.ThrowArgumentException_NodeValueNotAllowed(nameof(value));
            }

            jsonTypeInfo.EnsureConfigured();

            if (value is KdlElement element && jsonTypeInfo.EffectiveConverter.IsInternalConverter)
            {
                return CreateFromElement(ref element, options);
            }

            return CreateFromTypeInfo(value, jsonTypeInfo, options);
        }

        internal override bool DeepEqualsCore(KdlNode otherNode)
        {
            if (GetValueKind() != otherNode.GetValueKind())
            {
                return false;
            }

            // Fall back to slow path that converts the nodes to KdlElement.
            KdlElement thisElement = ToKdlElement(this, out KdlDocument? thisDocument);
            KdlElement otherElement = ToKdlElement(otherNode, out KdlDocument? otherDocument);
            try
            {
                return KdlElement.DeepEquals(thisElement, otherElement);
            }
            finally
            {
                thisDocument?.Dispose();
                otherDocument?.Dispose();
            }

            static KdlElement ToKdlElement(KdlNode node, out KdlDocument? backingDocument)
            {
                if (node.UnderlyingElement is { } element)
                {
                    backingDocument = null;
                    return element;
                }

                KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(
                    options: default,
                    KdlSerializerOptions.BufferSizeDefault,
                    out PooledByteBufferWriter output);

                try
                {
                    node.WriteTo(writer);
                    writer.Flush();
                    KdlReader reader = new(output.WrittenMemory.Span);
                    backingDocument = KdlDocument.ParseValue(ref reader);
                    return backingDocument.RootElement;
                }
                finally
                {
                    KdlWriterCache.ReturnWriterAndBuffer(writer, output);
                }
            }
        }

        internal sealed override void GetPath(ref ValueStringBuilder path, KdlNode? child)
        {
            Debug.Assert(child == null);

            Parent?.GetPath(ref path, this);
        }

        internal static KdlValue CreateFromTypeInfo<T>(T value, KdlTypeInfo<T> jsonTypeInfo, KdlNodeOptions? options = null)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            Debug.Assert(value != null);

            if (KdlValue<T>.TypeIsSupportedPrimitive &&
                jsonTypeInfo is { EffectiveConverter.IsInternalConverter: true } &&
                (jsonTypeInfo.EffectiveNumberHandling & KdlNumberHandling.WriteAsString) is 0)
            {
                // If the type is using the built-in converter for a known primitive,
                // switch to the more efficient KdlValuePrimitive<T> implementation.
                return new KdlValuePrimitive<T>(value, jsonTypeInfo.EffectiveConverter, options);
            }

            return new KdlValueCustomized<T>(value, jsonTypeInfo, options);
        }

        internal static KdlValue? CreateFromElement(ref readonly KdlElement element, KdlNodeOptions? options = null)
        {
            switch (element.ValueKind)
            {
                case KdlValueKind.Null:
                    return null;

                case KdlValueKind.Object or KdlValueKind.Array:
                    // Force usage of KdlArray and KdlObject instead of supporting those in an KdlValue.
                    ThrowHelper.ThrowInvalidOperationException_NodeElementCannotBeObjectOrArray();
                    return null;

                default:
                    return new KdlValueOfElement(element, options);
            }
        }
    }
}
