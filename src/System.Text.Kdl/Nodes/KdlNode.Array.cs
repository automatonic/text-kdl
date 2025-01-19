using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Converters;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   Represents a mutable KDL array.
    /// </summary>
    /// <remarks>
    /// It is safe to perform multiple concurrent read operations on a <see cref="KdlNode"/>,
    /// but issues can occur if the collection is modified while it's being read.
    /// </remarks>
    [DebuggerDisplay("KdlNode[{List.Count}]")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class KdlNode : KdlVertex
    {


        private KdlElement? _kdlElement;
        private List<KdlVertex?>? _list;

        internal override KdlElement? UnderlyingElement => _kdlElement;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that is empty.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        public KdlNode(KdlNodeOptions? options = null) : base(options) { }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified params array.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        public KdlNode(KdlNodeOptions options, params KdlVertex?[] items) : base(options) => InitializeFromArray(items);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains the specified <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">The properties to be added.</param>
        /// <param name="options">Options to control the behavior.</param>
        public KdlNode(IEnumerable<KeyValuePair<string, KdlVertex?>> properties, KdlNodeOptions? options = null) : this(options)
        {
            int capacity = properties is ICollection<KeyValuePair<string, KdlVertex?>> propertiesCollection ? propertiesCollection.Count : 0;
            OrderedDictionary<string, KdlVertex?> dictionary = CreateDictionary(options, capacity);

            foreach (KeyValuePair<string, KdlVertex?> node in properties)
            {
                dictionary.Add(node.Key, node.Value);
                node.Value?.AssignParent(this);
            }

            _dictionary = dictionary;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified params span.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        public KdlNode(KdlNodeOptions options, params ReadOnlySpan<KdlVertex?> items) : base(options) => InitializeFromSpan(items);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified array.
        /// </summary>
        /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        public KdlNode(params KdlVertex?[] items) : base() => InitializeFromArray(items);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified span.
        /// </summary>
        /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        public KdlNode(params ReadOnlySpan<KdlVertex?> items) : base() => InitializeFromSpan(items);

        private protected override KdlValueKind GetValueKindCore() => KdlValueKind.Node;

        internal override KdlVertex DeepCloneCore()
        {
            //TECHDEBT: unify these as well
            GetUnderlyingRepresentation(out List<KdlVertex?>? list, out KdlElement? kdlElement);
            GetUnderlyingRepresentation(out OrderedDictionary<string, KdlVertex?>? dictionary, out kdlElement);

            if (list is null)
            {
                return kdlElement.HasValue
                    ? new KdlNode(kdlElement.Value.Clone(), Options)
                    : new KdlNode(Options);
            }

            if (dictionary is null)
            {
                return kdlElement.HasValue
                    ? new KdlNode(kdlElement.Value.Clone(), Options)
                    : new KdlNode(Options);
            }

            var kdlNode = new KdlNode(Options)
            {
                _list = new List<KdlVertex?>(list.Count),
                _dictionary = CreateDictionary(Options, Count)
            };

            for (int i = 0; i < list.Count; i++)
            {
                kdlNode.Add(list[i]?.DeepCloneCore());
            }
            foreach (KeyValuePair<string, KdlVertex?> item in dictionary)
            {
                kdlNode.Add(item.Key, item.Value?.DeepCloneCore());
            }

            return kdlNode;
        }

        internal override bool DeepEqualsCore(KdlVertex vertex)
        {
            switch (vertex)
            {
                case KdlValue value:
                    // KdlValue instances have special comparison semantics, dispatch to their implementation.
                    return value.DeepEqualsCore(this);
                case KdlNode array:
                    List<KdlVertex?> currentList = List;
                    List<KdlVertex?> otherList = array.List;

                    if (currentList.Count != otherList.Count)
                    {
                        return false;
                    }

                    for (int i = 0; i < currentList.Count; i++)
                    {
                        if (!DeepEquals(currentList[i], otherList[i]))
                        {
                            return false;
                        }
                    }

                    OrderedDictionary<string, KdlVertex?> currentDict = Dictionary;
                    OrderedDictionary<string, KdlVertex?> otherDict = array.Dictionary;

                    if (currentDict.Count != otherDict.Count)
                    {
                        return false;
                    }

                    foreach (KeyValuePair<string, KdlVertex?> item in currentDict)
                    {
                        otherDict.TryGetValue(item.Key, out KdlVertex? jsonNode);

                        if (!DeepEquals(item.Value, jsonNode))
                        {
                            return false;
                        }
                    }

                    return true;
                default:
                    Debug.Fail("Impossible case");
                    return false;
            }
        }

        internal int GetElementIndex(KdlVertex? node)
        {
            return List.IndexOf(node);
        }

        /// <summary>
        /// Returns an enumerable that wraps calls to <see cref="KdlVertex.GetValue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to obtain from the <see cref="KdlValue"/>.</typeparam>
        /// <returns>An enumerable iterating over values of the array.</returns>
        public IEnumerable<T> GetValues<T>()
        {
            foreach (KdlVertex? item in List)
            {
                yield return item is null ? (T)(object?)null! : item.GetValue<T>();
            }
        }

        private void InitializeFromArray(KdlVertex?[] items)
        {
            var list = new List<KdlVertex?>(items);

            for (int i = 0; i < list.Count; i++)
            {
                list[i]?.AssignParent(this);
            }

            _list = list;
        }

        private void InitializeFromSpan(ReadOnlySpan<KdlVertex?> items)
        {
            List<KdlVertex?> list = new(items.Length);

#if NET8_0_OR_GREATER
            list.AddRange(items);
#else
            foreach (KdlVertex? item in items)
            {
                list.Add(item);
            }
#endif

            for (int i = 0; i < list.Count; i++)
            {
                list[i]?.AssignParent(this);
            }

            _list = list;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified <see cref="KdlElement"/>.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlNode"/> class that contains items from the specified <see cref="KdlElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="KdlElement"/>.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <exception cref="InvalidOperationException">
        ///   The <paramref name="element"/> is not a <see cref="KdlValueKind.Node"/>.
        /// </exception>
        public static KdlNode? Create(KdlElement element, KdlNodeOptions? options = null)
        {
            return element.ValueKind switch
            {
                KdlValueKind.Null => null,
                KdlValueKind.Node => new KdlNode(element, options),
                _ => throw new InvalidOperationException(string.Format(SR.NodeElementWrongType, nameof(KdlValueKind.Node))),
            };
        }

        internal KdlNode(KdlElement element, KdlNodeOptions? options = null) : base(options)
        {
            Debug.Assert(element.ValueKind == KdlValueKind.Node);
            _kdlElement = element;
        }

        /// <summary>
        ///   Adds an object to the end of the <see cref="KdlNode"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to be added.</typeparam>
        /// <param name="value">
        ///   The object to be added to the end of the <see cref="KdlNode"/>.
        /// </param>
        [RequiresUnreferencedCode(KdlValue.CreateUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlValue.CreateDynamicCodeMessage)]
        public void Add<T>(T? value)
        {
            KdlVertex? nodeToAdd = ConvertFromValue(value, Options);
            Add(nodeToAdd);
        }

        /// <summary>
        /// Gets or creates the underlying list containing the element nodes of the array.
        /// </summary>
        private List<KdlVertex?> List => _list ?? InitializeList();

        private protected override KdlVertex? GetItem(int index)
        {
            //TECHDEBT: This is a pretty hacky way to do this
            //Consider a sorted dictionary that is keyed with "string or index" type?
            if (index < Dictionary.Count)
            {
                return GetAt(index).Value;
            }
            index -= Dictionary.Count;
            return List[index];
        }
        private protected override void SetItem(int index, KdlVertex? value)
        {
            //TECHDEBT: This is a pretty hacky way to do this
            //Consider a sorted dictionary that is keyed with "string or index" type?
            if (index < Dictionary.Count)
            {
                SetAt(index, value);
                return;
            }
            index -= Dictionary.Count;

            value?.AssignParent(this);
            DetachParentForListItem(List[index]);
            List[index] = value;
        }

        internal override void GetPath(ref ValueStringBuilder path, KdlVertex? child)
        {
            Parent?.GetPath(ref path, this);

            if (child != null)
            {
                int index = List.IndexOf(child);
                Debug.Assert(index >= 0);

                path.Append('[');
#if NET
                Span<char> chars = stackalloc char[KdlConstants.MaximumFormatUInt32Length];
                bool formatted = ((uint)index).TryFormat(chars, out int charsWritten);
                Debug.Assert(formatted);
                path.Append(chars[..charsWritten]);
#else
                path.Append(index.ToString());
#endif
                path.Append(']');
            }
            //TECHDEBT
            // Parent?.GetPath(ref path, this);

            // if (child != null)
            // {
            //     string propertyName = FindValue(child)!.Value.Key;
            //     if (propertyName.AsSpan().ContainsSpecialCharacters())
            //     {
            //         path.Append("['");
            //         path.Append(propertyName);
            //         path.Append("']");
            //     }
            //     else
            //     {
            //         path.Append('.');
            //         path.Append(propertyName);
            //     }
            // }
        }

        /// <inheritdoc/>
        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            GetUnderlyingRepresentation(out List<KdlVertex?>? list, out KdlElement? kdlElement);

            if (list is null && kdlElement.HasValue)
            {
                kdlElement.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteStartArray();

                foreach (KdlVertex? element in List)
                {
                    if (element is null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        element.WriteTo(writer, options);
                    }
                }

                writer.WriteEndArray();
            }

            //TECHDEBT
            // if (writer is null)
            // {
            //     ThrowHelper.ThrowArgumentNullException(nameof(writer));
            // }

            // GetUnderlyingRepresentation(out OrderedDictionary<string, KdlVertex?>? dictionary, out KdlElement? kdlElement);

            // if (dictionary is null && kdlElement.HasValue)
            // {
            //     // Write the element without converting to nodes.
            //     kdlElement.Value.WriteTo(writer);
            // }
            // else
            // {
            //     writer.WriteStartObject();

            //     foreach (KeyValuePair<string, KdlVertex?> entry in Dictionary)
            //     {
            //         writer.WritePropertyName(entry.Key);

            //         if (entry.Value is null)
            //         {
            //             writer.WriteNullValue();
            //         }
            //         else
            //         {
            //             entry.Value.WriteTo(writer, options);
            //         }
            //     }

            //     writer.WriteEndObject();
            // }
        }

        private List<KdlVertex?> InitializeList()
        {
            GetUnderlyingRepresentation(out List<KdlVertex?>? list, out KdlElement? kdlElement);

            if (list is null)
            {
                if (kdlElement.HasValue)
                {
                    KdlElement jElement = kdlElement.Value;
                    Debug.Assert(jElement.ValueKind == KdlValueKind.Node);

                    list = new List<KdlVertex?>(jElement.GetArrayLength());

                    foreach (KdlElement element in jElement.EnumerateArray())
                    {
                        KdlVertex? node = KdlNodeConverter.Create(element, Options);
                        node?.AssignParent(this);
                        list.Add(node);
                    }
                }
                else
                {
                    list = [];
                }

                // Ensure _kdlElement is written to after _list
                _list = list;
                Interlocked.MemoryBarrier();
                _kdlElement = null;
            }

            return list;
        }

        /// <summary>
        /// Provides a coherent view of the underlying representation of the current node.
        /// The kdlElement value should be consumed if and only if the list value is null.
        /// </summary>
        private void GetUnderlyingRepresentation(out List<KdlVertex?>? list, out KdlElement? kdlElement)
        {
            // Because KdlElement cannot be read atomically there might be torn reads,
            // however the order of read/write operations guarantees that that's only
            // possible if the value of _list is non-null.
            kdlElement = _kdlElement;
            Interlocked.MemoryBarrier();
            list = _list;
        }

        private static void DetachParentForListItem(KdlVertex? item)
        {
            if (item != null)
            {
                item.Parent = null;
            }
        }

        [ExcludeFromCodeCoverage] // Justification = "Design-time"
        private sealed class DebugView(KdlNode node)
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly KdlNode _node = node;

            public string Kdl => _node.ToKdlString();
            public string Path => _node.GetPath();

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
#pragma warning disable IDE0051 // Remove unused private members
            private DebugViewItem[] ListItems
#pragma warning restore IDE0051 // Remove unused private members
            {
                get
                {
                    DebugViewItem[] properties = new DebugViewItem[_node.List.Count];

                    for (int i = 0; i < _node.List.Count; i++)
                    {
                        properties[i].Value = _node.List[i];
                    }

                    return properties;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
#pragma warning disable IDE0051 // Remove unused private members
            private DebugViewProperty[] Properties
#pragma warning restore IDE0051 // Remove unused private members
            {
                get
                {
                    DebugViewProperty[] properties = new DebugViewProperty[_node.Count];

                    int i = 0;
                    foreach (KeyValuePair<string, KdlVertex?> item in (IEnumerable<KeyValuePair<string, KdlVertex?>>)_node)
                    {
                        properties[i].PropertyName = item.Key;
                        properties[i].Value = item.Value;
                        i++;
                    }

                    return properties;
                }
            }

            [DebuggerDisplay("{Display,nq}")]
            private struct DebugViewProperty
            {
                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public KdlVertex? Value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string PropertyName;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public readonly string Display
                {
                    get
                    {
                        if (Value == null)
                        {
                            return $"{PropertyName} = null";
                        }

                        if (Value is KdlValue)
                        {
                            return $"{PropertyName} = {Value.ToKdlString()}";
                        }

                        if (Value is KdlNode kdlNode)
                        {
                            return $"{PropertyName} = KdlNode[{kdlNode.Count}]";
                        }

                        KdlNode kdlNode2 = (KdlNode)Value;
                        return $"{PropertyName} = KdlNode[{kdlNode2.Count}]";
                    }
                }

            }

            [DebuggerDisplay("{Display,nq}")]
            private struct DebugViewItem
            {
                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public KdlVertex? Value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public readonly string Display
                {
                    get
                    {
                        if (Value == null)
                        {
                            return $"null";
                        }

                        if (Value is KdlValue)
                        {
                            return Value.ToKdlString();
                        }

                        if (Value is KdlNode kdlNode)
                        {
                            return $"KdlNode[{kdlNode.Count}]";
                        }

                        KdlNode kdlNode2 = (KdlNode)Value;
                        return $"KdlNode[{kdlNode2.List.Count}]";
                    }
                }
            }
        }
    }
}
