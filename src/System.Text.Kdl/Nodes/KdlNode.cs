using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Converters;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   The base class that represents a single node within a mutable KDL document.
    /// </summary>
    /// <seealso cref="KdlSerializerOptions.UnknownTypeHandling"/> to specify that a type
    /// declared as an <see cref="object"/> should be deserialized as a <see cref="KdlNode"/>.
    public abstract partial class KdlNode
    {
        // Default options instance used when calling built-in KdlNode converters.
        private protected static readonly KdlSerializerOptions s_defaultOptions = new();

        private KdlNode? _parent;
        private KdlNodeOptions? _options;

        /// <summary>
        /// The underlying KdlElement if the node is backed by a KdlElement.
        /// </summary>
        internal virtual KdlElement? UnderlyingElement => null;

        /// <summary>
        ///   Options to control the behavior.
        /// </summary>
        public KdlNodeOptions? Options
        {
            get
            {
                if (!_options.HasValue && Parent != null)
                {
                    // Remember the parent options; if node is re-parented later we still want to keep the
                    // original options since they may have affected the way the node was created as is the case
                    // with KdlObject's case-insensitive dictionary.
                    _options = Parent.Options;
                }

                return _options;
            }
        }

        internal KdlNode(KdlNodeOptions? options = null) => _options = options;

        /// <summary>
        ///   Casts to the derived <see cref="KdlArray"/> type.
        /// </summary>
        /// <returns>
        ///   A <see cref="KdlArray"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The node is not a <see cref="KdlArray"/>.
        /// </exception>
        public KdlArray AsArray()
        {
            KdlArray? jArray = this as KdlArray;

            if (jArray is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(KdlArray));
            }

            return jArray;
        }

        /// <summary>
        ///   Casts to the derived <see cref="KdlObject"/> type.
        /// </summary>
        /// <returns>
        ///   A <see cref="KdlObject"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The node is not a <see cref="KdlObject"/>.
        /// </exception>
        public KdlObject AsObject()
        {
            KdlObject? jObject = this as KdlObject;

            if (jObject is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(KdlObject));
            }

            return jObject;
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
                ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(KdlValue));
            }

            return jValue;
        }

        /// <summary>
        ///   Gets the parent <see cref="KdlNode"/>.
        ///   If there is no parent, <see langword="null"/> is returned.
        ///   A parent can either be a <see cref="KdlObject"/> or a <see cref="KdlArray"/>.
        /// </summary>
        public KdlNode? Parent
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

            var path = new ValueStringBuilder(stackalloc char[KdlConstants.StackallocCharThreshold]);
            path.Append('$');
            GetPath(ref path, null);
            return path.ToString();
        }

        internal abstract void GetPath(ref ValueStringBuilder path, KdlNode? child);

        /// <summary>
        ///   Gets the root <see cref="KdlNode"/>.
        /// </summary>
        /// <remarks>
        ///   The current node is returned if it is a root.
        /// </remarks>
        public KdlNode Root
        {
            get
            {
                KdlNode? parent = Parent;
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
        ///   If the underlying value is a <see cref="KdlElement"/> then {T} can also be the type of any primitive
        ///   value supported by current <see cref="KdlElement"/>.
        ///   Specifying the <see cref="object"/> type for {T} will always succeed and return the underlying value as <see cref="object"/>.<br />
        ///   The underlying value of a <see cref="KdlValue"/> after deserialization is an instance of <see cref="KdlElement"/>,
        ///   otherwise it's the value specified when the <see cref="KdlValue"/> was created.
        /// </remarks>
        /// <seealso cref="System.Text.Kdl.Nodes.KdlValue.TryGetValue"></seealso>
        /// <exception cref="FormatException">
        ///   The current <see cref="KdlNode"/> cannot be represented as a {T}.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The current <see cref="KdlNode"/> is not a <see cref="KdlValue"/> or
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
        ///   The current <see cref="KdlNode"/> is not a <see cref="KdlArray"/> or <see cref="KdlObject"/>.
        /// </exception>
        public KdlNode? this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        private protected virtual KdlNode? GetItem(int index)
        {
            ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(KdlArray), nameof(KdlObject));
            return null;
        }

        private protected virtual void SetItem(int index, KdlNode? node) =>
            ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(KdlArray), nameof(KdlObject));

        /// <summary>
        ///   Gets or sets the element with the specified property name.
        ///   If the property is not found, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="propertyName">The name of the property to return.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The current <see cref="KdlNode"/> is not a <see cref="KdlObject"/>.
        /// </exception>
        public KdlNode? this[string propertyName]
        {
            get => AsObject().GetItem(propertyName);
            set => AsObject().SetItem(propertyName, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="KdlNode"/>. All children nodes are recursively cloned.
        /// </summary>
        /// <returns>A new cloned instance of the current node.</returns>
        public KdlNode DeepClone() => DeepCloneCore();

        internal abstract KdlNode DeepCloneCore();

        /// <summary>
        /// Returns <see cref="KdlValueKind"/> of current instance.
        /// </summary>
        public KdlValueKind GetValueKind() => GetValueKindCore();

        private protected abstract KdlValueKind GetValueKindCore();

        /// <summary>
        /// Returns property name of the current node from the parent object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current parent is not a <see cref="KdlObject"/>.
        /// </exception>
        public string GetPropertyName()
        {
            KdlObject? parentObject = _parent as KdlObject;

            if (parentObject is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeParentWrongType(nameof(KdlObject));
            }

            return parentObject.GetPropertyName(this);
        }

        /// <summary>
        /// Returns index of the current node from the parent <see cref="KdlArray" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current parent is not a <see cref="KdlArray"/>.
        /// </exception>
        public int GetElementIndex()
        {
            KdlArray? parentArray = _parent as KdlArray;

            if (parentArray is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeParentWrongType(nameof(KdlArray));
            }

            return parentArray.GetElementIndex(this);
        }

        /// <summary>
        /// Compares the values of two nodes, including the values of all descendant nodes.
        /// </summary>
        /// <param name="node1">The <see cref="KdlNode"/> to compare.</param>
        /// <param name="node2">The <see cref="KdlNode"/> to compare.</param>
        /// <returns><c>true</c> if the tokens are equal; otherwise <c>false</c>.</returns>
        public static bool DeepEquals(KdlNode? node1, KdlNode? node2)
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

        internal abstract bool DeepEqualsCore(KdlNode node);

        /// <summary>
        /// Replaces this node with a new value.
        /// </summary>
        /// <typeparam name="T">The type of value to be replaced.</typeparam>
        /// <param name="value">Value that replaces this node.</param>
        [RequiresUnreferencedCode(KdlValue.CreateUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlValue.CreateDynamicCodeMessage)]
        public void ReplaceWith<T>(T value)
        {
            KdlNode? node;
            switch (_parent)
            {
                case KdlObject jsonObject:
                    node = ConvertFromValue(value);
                    jsonObject.SetItem(GetPropertyName(), node);
                    return;
                case KdlArray jsonArray:
                    node = ConvertFromValue(value);
                    jsonArray.SetItem(GetElementIndex(), node);
                    return;
            }
        }

        internal void AssignParent(KdlNode parent)
        {
            if (Parent != null)
            {
                ThrowHelper.ThrowInvalidOperationException_NodeAlreadyHasParent();
            }

            KdlNode? p = parent;
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
        /// to support arbitrary <see cref="KdlElement"/> and <see cref="KdlNode"/> values.
        /// TODO consider making public cf. https://github.com/dotnet/runtime/issues/70427
        /// </summary>
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal static KdlNode? ConvertFromValue<T>(T? value, KdlNodeOptions? options = null)
        {
            if (value is null)
            {
                return null;
            }

            if (value is KdlNode node)
            {
                return node;
            }

            if (value is KdlElement element)
            {
                return KdlNodeConverter.Create(element, options);
            }

            var jsonTypeInfo = (KdlTypeInfo<T>)KdlSerializerOptions.Default.GetTypeInfo(typeof(T));
            return KdlValue.CreateFromTypeInfo(value, jsonTypeInfo, options);
        }
    }
}
