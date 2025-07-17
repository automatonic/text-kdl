using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Graph
{
    /// <summary>
    /// Represents a mutable KDL value.
    /// </summary>
    public abstract partial class KdlValue : KdlElement
    {
        internal const string CreateUnreferencedCodeMessage =
            "Creating KdlValue instances with non-primitive types is not compatible with trimming. It can result in non-primitive types being serialized, which may have their members trimmed.";
        internal const string CreateDynamicCodeMessage =
            "Creating KdlValue instances with non-primitive types requires generating code at runtime.";

        private protected KdlValue(KdlElementOptions? options)
            : base(options) { }

        /// <summary>
        ///   Tries to obtain the current KDL value and returns a value that indicates whether the operation succeeded.
        /// </summary>
        /// <remarks>
        ///   {T} can be the type or base type of the underlying value.
        ///   If the underlying value is a <see cref="KdlReadOnlyElement"/> then {T} can also be the type of any primitive
        ///   value supported by current <see cref="KdlReadOnlyElement"/>.
        ///   Specifying the <see cref="object"/> type for {T} will always succeed and return the underlying value as <see cref="object"/>.<br />
        ///   The underlying value of a <see cref="KdlValue"/> after deserialization is an instance of <see cref="KdlReadOnlyElement"/>,
        ///   otherwise it's the value specified when the <see cref="KdlValue"/> was created.
        /// </remarks>
        /// <seealso cref="KdlElement.GetValue{T}"></seealso>
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
        [RequiresUnreferencedCode(
            CreateUnreferencedCodeMessage
                + " Use the overload that takes a KdlTypeInfo, or make sure all of the required types are preserved."
        )]
        [RequiresDynamicCode(CreateDynamicCodeMessage)]
        public static KdlValue? Create<T>(T? value, KdlElementOptions? options = null)
        {
            if (value is null)
            {
                return null;
            }

            if (value is KdlElement)
            {
                ThrowHelper.ThrowArgumentException_NodeValueNotAllowed(nameof(value));
            }

            if (value is KdlReadOnlyElement element)
            {
                return CreateFromElement(ref element, options);
            }

            var kdlTypeInfo = (KdlTypeInfo<T>)KdlSerializerOptions.Default.GetTypeInfo(typeof(T));
            return CreateFromTypeInfo(value, kdlTypeInfo, options);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </returns>
        /// <typeparam name="T">The type of value to create.</typeparam>
        /// <param name="value">The value to create.</param>
        /// <param name="kdlTypeInfo">The <see cref="KdlTypeInfo"/> that will be used to serialize the value.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create<T>(
            T? value,
            KdlTypeInfo<T> kdlTypeInfo,
            KdlElementOptions? options = null
        )
        {
            if (kdlTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdlTypeInfo));
            }

            if (value is null)
            {
                return null;
            }

            if (value is KdlElement)
            {
                ThrowHelper.ThrowArgumentException_NodeValueNotAllowed(nameof(value));
            }

            kdlTypeInfo.EnsureConfigured();

            if (
                value is KdlReadOnlyElement element
                && kdlTypeInfo.EffectiveConverter.IsInternalConverter
            )
            {
                return CreateFromElement(ref element, options);
            }

            return CreateFromTypeInfo(value, kdlTypeInfo, options);
        }

        internal override bool DeepEqualsCore(KdlElement otherNode)
        {
            if (GetValueKind() != otherNode.GetValueKind())
            {
                return false;
            }

            // Fall back to slow path that converts the nodes to KdlElement.
            KdlReadOnlyElement thisElement = ToKdlElement(
                this,
                out KdlReadOnlyDocument? thisDocument
            );
            KdlReadOnlyElement otherElement = ToKdlElement(
                otherNode,
                out KdlReadOnlyDocument? otherDocument
            );
            try
            {
                return KdlReadOnlyElement.DeepEquals(thisElement, otherElement);
            }
            finally
            {
                thisDocument?.Dispose();
                otherDocument?.Dispose();
            }

            static KdlReadOnlyElement ToKdlElement(
                KdlElement vertex,
                out KdlReadOnlyDocument? backingDocument
            )
            {
                if (vertex.UnderlyingReadOnlyElement is { } element)
                {
                    backingDocument = null;
                    return element;
                }

                KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(
                    options: default,
                    KdlSerializerOptions.BufferSizeDefault,
                    out PooledByteBufferWriter output
                );

                try
                {
                    vertex.WriteTo(writer);
                    writer.Flush();
                    KdlReader reader = new(output.WrittenMemory.Span);
                    backingDocument = KdlReadOnlyDocument.ParseValue(ref reader);
                    return backingDocument.RootElement;
                }
                finally
                {
                    KdlWriterCache.ReturnWriterAndBuffer(writer, output);
                }
            }
        }

        internal sealed override void GetPath(ref ValueStringBuilder path, KdlElement? child)
        {
            Debug.Assert(child == null);

            Parent?.GetPath(ref path, this);
        }

        internal static KdlValue CreateFromTypeInfo<T>(
            T value,
            KdlTypeInfo<T> kdlTypeInfo,
            KdlElementOptions? options = null
        )
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);
            Debug.Assert(value != null);

            if (
                KdlValue<T>.TypeIsSupportedPrimitive
                && kdlTypeInfo is { EffectiveConverter.IsInternalConverter: true }
                && (kdlTypeInfo.EffectiveNumberHandling & KdlNumberHandling.WriteAsString) is 0
            )
            {
                // If the type is using the built-in converter for a known primitive,
                // switch to the more efficient KdlValuePrimitive<T> implementation.
                return new KdlValuePrimitive<T>(value, kdlTypeInfo.EffectiveConverter, options);
            }

            return new KdlValueCustomized<T>(value, kdlTypeInfo, options);
        }

        internal static KdlValue? CreateFromElement(
            ref readonly KdlReadOnlyElement element,
            KdlElementOptions? options = null
        )
        {
            switch (element.ValueKind)
            {
                case KdlValueKind.Null:
                    return null;

                case KdlValueKind.Node:
                    // Force usage of KdlNode and KdlNode instead of supporting those in an KdlValue.
                    ThrowHelper.ThrowInvalidOperationException_NodeElementCannotBeObjectOrArray();
                    return null;

                default:
                    return new KdlValueOfElement(element, options);
            }
        }
    }
}
