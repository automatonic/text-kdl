using System.Diagnostics.CodeAnalysis;
using System.Text;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization.Converters;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Graph
{
    /// <summary>
    ///   The base class that represents a single node within a mutable KDL document.
    /// </summary>
    /// <seealso cref="KdlSerializerOptions.UnknownTypeHandling"/> to specify that a type
    /// declared as an <see cref="object"/> should be deserialized as a <see cref="KdlElement"/>.
    public abstract partial class KdlElement
    {
        // Default options instance used when calling built-in KdlVertex converters.
        private protected static readonly KdlSerializerOptions s_defaultOptions = new();

        private KdlElement? _parent;
        private KdlElementOptions? _options;

        /// <summary>
        /// The underlying KdlElement if the node is backed by a KdlElement.
        /// </summary>
        internal virtual KdlReadOnlyElement? UnderlyingReadOnlyElement => null;

        /// <summary>
        ///   Options to control the behavior.
        /// </summary>
        public KdlElementOptions? Options
        {
            get
            {
                if (!_options.HasValue && Parent != null)
                {
                    // Remember the parent options; if node is re-parented later we still want to keep the
                    // original options since they may have affected the way the node was created as is the case
                    // with KdlNode's case-insensitive dictionary.
                    _options = Parent.Options;
                }

                return _options;
            }
        }

        internal KdlElement(KdlElementOptions? options = null) => _options = options;

        /// <summary>
        ///   Casts to the derived <see cref="KdlNode"/> type.
        /// </summary>
        /// <returns>
        ///   A <see cref="KdlNode"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The node is not a <see cref="KdlNode"/>.
        /// </exception>
        public KdlNode AsArray()
        {
            KdlNode? jArray = this as KdlNode;

            if (jArray is null)
            {
                ThrowHelper.ThrowInvalidOperationException_ElementWrongType(nameof(KdlNode));
            }

            return jArray;
        }

        /// <summary>
        ///   Casts to the derived <see cref="KdlNode"/> type.
        /// </summary>
        /// <returns>
        ///   A <see cref="KdlNode"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The node is not a <see cref="KdlNode"/>.
        /// </exception>
        public KdlNode AsNode()
        {
            KdlNode? node = this as KdlNode;

            if (node is null)
            {
                ThrowHelper.ThrowInvalidOperationException_ElementWrongType(nameof(KdlNode));
            }

            return node;
        }

        /// <summary>
        ///   Casts to the derived <see cref="KdlValue"/> type.
        /// </summary>
        /// <returns>
        ///   A <see cref="KdlValue"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The node is not a <see cref="KdlValue"/>.
        /// </exception>
        public KdlValue AsValue()
        {
            KdlValue? jValue = this as KdlValue;

            if (jValue is null)
            {
                ThrowHelper.ThrowInvalidOperationException_ElementWrongType(nameof(KdlValue));
            }

            return jValue;
        }

        /// <summary>
        ///   Gets the parent <see cref="KdlElement"/>.
        ///   If there is no parent, <see langword="null"/> is returned.
        ///   A parent can either be a <see cref="KdlNode"/> or a <see cref="KdlNode"/>.
        /// </summary>
        public KdlElement? Parent
        {
            get => _parent;
            internal set => _parent = value;
        }

        /// <summary>
        ///   Gets the KDL path.
        /// </summary>
        /// <returns>The KDL Path value.</returns>
        public string GetPath()
        {
            if (Parent == null)
            {
                return "$";
            }

            var path = new ValueStringBuilder(
                stackalloc char[KdlConstants.StackallocCharThreshold]
            );
            path.Append('$');
            GetPath(ref path, null);
            return path.ToString();
        }

        internal abstract void GetPath(ref ValueStringBuilder path, KdlElement? child);

        /// <summary>
        ///   Gets the root <see cref="KdlElement"/>.
        /// </summary>
        /// <remarks>
        ///   The current node is returned if it is a root.
        /// </remarks>
        public KdlElement Root
        {
            get
            {
                KdlElement? parent = Parent;
                if (parent == null)
                {
                    return this;
                }

                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }

                return parent;
            }
        }

        /// <summary>
        ///   Gets the value for the current <see cref="KdlValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to obtain from the <see cref="KdlValue"/>.</typeparam>
        /// <returns>A value converted from the <see cref="KdlValue"/> instance.</returns>
        /// <remarks>
        ///   {T} can be the type or base type of the underlying value.
        ///   If the underlying value is a <see cref="KdlReadOnlyElement"/> then {T} can also be the type of any primitive
        ///   value supported by current <see cref="KdlReadOnlyElement"/>.
        ///   Specifying the <see cref="object"/> type for {T} will always succeed and return the underlying value as <see cref="object"/>.<br />
        ///   The underlying value of a <see cref="KdlValue"/> after deserialization is an instance of <see cref="KdlReadOnlyElement"/>,
        ///   otherwise it's the value specified when the <see cref="KdlValue"/> was created.
        /// </remarks>
        /// <seealso cref="Automatonic.Text.Kdl.Graph.KdlValue.TryGetValue"></seealso>
        /// <exception cref="FormatException">
        ///   The current <see cref="KdlElement"/> cannot be represented as a {T}.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The current <see cref="KdlElement"/> is not a <see cref="KdlValue"/> or
        ///   is not compatible with {T}.
        /// </exception>
        public virtual T GetValue<T>() =>
            throw new InvalidOperationException(string.Format(SR.NodeWrongType, nameof(KdlValue)));

        /// <summary>
        ///   Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> is less than 0 or <paramref name="index"/> is greater than the number of properties.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The current <see cref="KdlElement"/> is not a <see cref="KdlNode"/> or <see cref="KdlNode"/>.
        /// </exception>
        public KdlElement? this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        private protected virtual KdlElement? GetItem(int index)
        {
            ThrowHelper.ThrowInvalidOperationException_ElementWrongType(
                nameof(KdlNode),
                nameof(KdlNode)
            );
            return null;
        }

        private protected virtual void SetItem(int index, KdlElement? node) =>
            ThrowHelper.ThrowInvalidOperationException_ElementWrongType(
                nameof(KdlNode),
                nameof(KdlNode)
            );

        /// <summary>
        ///   Gets or sets the element with the specified property name.
        ///   If the property is not found, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="propertyName">The name of the property to return.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The current <see cref="KdlElement"/> is not a <see cref="KdlNode"/>.
        /// </exception>
        public KdlElement? this[KdlEntryKey propertyName]
        {
            get => AsNode().GetItem(propertyName);
            set => AsNode().SetItem(propertyName, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="KdlElement"/>. All children nodes are recursively cloned.
        /// </summary>
        /// <returns>A new cloned instance of the current node.</returns>
        public KdlElement DeepClone() => DeepCloneCore();

        internal abstract KdlElement DeepCloneCore();

        /// <summary>
        /// Returns <see cref="KdlValueKind"/> of current instance.
        /// </summary>
        public KdlValueKind GetValueKind() => GetValueKindCore();

        private protected abstract KdlValueKind GetValueKindCore();

        /// <summary>
        /// Returns property name of the current node from the parent object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current parent is not a <see cref="KdlNode"/>.
        /// </exception>
        public KdlEntryKey? GetPropertyName()
        {
            KdlNode? parentObject = _parent as KdlNode;

            if (parentObject is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeParentWrongType(nameof(KdlNode));
            }

            return parentObject.GetPropertyName(this);
        }

        /// <summary>
        /// Returns index of the current node from the parent <see cref="KdlNode" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current parent is not a <see cref="KdlNode"/>.
        /// </exception>
        public int GetElementIndex()
        {
            KdlNode? parentArray = _parent as KdlNode;

            if (parentArray is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeParentWrongType(nameof(KdlNode));
            }

            return parentArray.GetElementIndex(this);
        }

        /// <summary>
        /// Compares the values of two nodes, including the values of all descendant nodes.
        /// </summary>
        /// <param name="node1">The <see cref="KdlElement"/> to compare.</param>
        /// <param name="node2">The <see cref="KdlElement"/> to compare.</param>
        /// <returns><c>true</c> if the tokens are equal; otherwise <c>false</c>.</returns>
        public static bool DeepEquals(KdlElement? node1, KdlElement? node2)
        {
            if (node1 is null)
            {
                return node2 is null;
            }
            else if (node2 is null)
            {
                return false;
            }

            return node1.DeepEqualsCore(node2);
        }

        internal abstract bool DeepEqualsCore(KdlElement elementNode);

        /// <summary>
        /// Replaces this node with a new value.
        /// </summary>
        /// <typeparam name="T">The type of value to be replaced.</typeparam>
        /// <param name="value">Value that replaces this node.</param>
        [RequiresUnreferencedCode(KdlValue.CreateUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlValue.CreateDynamicCodeMessage)]
        [SuppressMessage(
            "Performance",
            "CA1822:Mark members as static",
            Justification = "<Pending>"
        )]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void ReplaceWith<T>(T value)
        {
            //TECHDEBT: Disabled. We have to know if we are already a prop or indexed, can we just look at GetPropertyName? What will be fastest?
            // KdlVertex? node;
            // switch (_parent)
            // {
            //     case KdlNode kdlNode:
            //         node = ConvertFromValue(value);
            //         kdlNode.SetItem(GetPropertyName(), node);
            //         return;
            //     case KdlNode kdlNode:
            //         node = ConvertFromValue(value);
            //         kdlNode.SetItem(GetElementIndex(), node);
            //         return;
            // }
        }

        internal void AssignParent(KdlElement parent)
        {
            if (Parent != null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeAlreadyHasParent();
            }

            KdlElement? p = parent;
            while (p != null)
            {
                if (p == this)
                {
                    ThrowHelper.ThrowInvalidOperationException_NodeCycleDetected();
                }

                p = p.Parent;
            }

            Parent = parent;
        }

        /// <summary>
        /// Adaptation of the equivalent KdlValue.Create factory method extended
        /// to support arbitrary <see cref="KdlReadOnlyElement"/> and <see cref="KdlElement"/> values.
        /// TODO consider making public cf. https://github.com/dotnet/runtime/issues/70427
        /// </summary>
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal static KdlElement? ConvertFromValue<T>(T? value, KdlElementOptions? options = null)
        {
            if (value is null)
            {
                return null;
            }

            if (value is KdlElement elementNode)
            {
                return elementNode;
            }

            if (value is KdlReadOnlyElement element)
            {
                return KdlVertexConverter.Create(element, options);
            }

            var kdlTypeInfo = (KdlTypeInfo<T>)KdlSerializerOptions.Default.GetTypeInfo(typeof(T));
            return KdlValue.CreateFromTypeInfo(value, kdlTypeInfo, options);
        }
    }
}
