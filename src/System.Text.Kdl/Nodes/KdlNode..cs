using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   Represents a mutable KDL node.
    /// </summary>
    /// <remarks>
    /// It is safe to perform multiple concurrent read operations on a <see cref="KdlNode"/>,
    /// but issues can occur if the collection is modified while it's being read.
    /// </remarks>
    [DebuggerDisplay("KdlNode[{List.Count}]")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class KdlNode : KdlElement
    {


        private KdlReadOnlyElement? _kdlElement;

        internal override KdlReadOnlyElement? UnderlyingReadOnlyElement => _kdlElement;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that is empty.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        public KdlNode(KdlElementOptions? options = null) : base(options) { }

        //TECHDEBT: May want these for argument inflow?
        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified array.
        /// </summary>
        /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        // public KdlNode(params KdlVertex?[] items) : base() => InitializeFromArray(items);

        // /// <summary>
        // ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified span.
        // /// </summary>
        // /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        // public KdlNode(params ReadOnlySpan<KdlVertex?> items) : base() => InitializeFromSpan(items);

        // /// <summary>
        // ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified params array.
        // /// </summary>
        // /// <param name="options">Options to control the behavior.</param>
        // /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        // public KdlNode(KdlNodeOptions options, params KdlVertex?[] items) : base(options) => InitializeFromArray(items);

        // /// <summary>
        // ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified params span.
        // /// </summary>
        // /// <param name="options">Options to control the behavior.</param>
        // /// <param name="items">The items to add to the new <see cref="KdlNode"/>.</param>
        // public KdlNode(KdlNodeOptions options, params ReadOnlySpan<KdlVertex?> items) : base(options) => InitializeFromSpan(items);

        // private void InitializeFromArray(KdlVertex?[] items)
        // {
        //     var list = new List<KdlVertex?>(items);

        //     for (int i = 0; i < list.Count; i++)
        //     {
        //         list[i]?.AssignParent(this);
        //     }

        //     _list = list;
        // }

        // private void InitializeFromSpan(ReadOnlySpan<KdlVertex?> items)
        // {
        //     List<KdlVertex?> list = [.. items];

        //     for (int i = 0; i < list.Count; i++)
        //     {
        //         list[i]?.AssignParent(this);
        //     }

        //     _list = list;
        // }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains the specified <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">The properties to be added.</param>
        /// <param name="options">Options to control the behavior.</param>
        public KdlNode(IEnumerable<KeyValuePair<KdlEntryKey, KdlElement?>> properties, KdlElementOptions? options = null) : this(options)
        {
            int capacity = properties is ICollection<KeyValuePair<KdlEntryKey, KdlElement?>> propertiesCollection ? propertiesCollection.Count : 0;
            OrderedDictionary<KdlEntryKey, KdlElement?> dictionary = CreateDictionary(options, capacity);

            foreach (KeyValuePair<KdlEntryKey, KdlElement?> node in properties)
            {
                dictionary.Add(node.Key, node.Value);
                node.Value?.AssignParent(this);
            }

            _dictionary = dictionary;
        }



        private protected override KdlValueKind GetValueKindCore() => KdlValueKind.Node;

        internal override KdlElement DeepCloneCore()
        {
            //TECHDEBT: unify these as well
            GetUnderlyingRepresentation(out OrderedDictionary<KdlEntryKey, KdlElement?>? dictionary, out var kdlElement);

            if (dictionary is null)
            {
                return kdlElement.HasValue
                    ? new KdlNode(kdlElement.Value.Clone(), Options)
                    : new KdlNode(Options);
            }

            var kdlNode = new KdlNode(Options)
            {
                _dictionary = CreateDictionary(Options, Count)
            };

            foreach (KeyValuePair<KdlEntryKey, KdlElement?> item in dictionary)
            {
                kdlNode.Add(item.Key, item.Value?.DeepCloneCore());
            }

            return kdlNode;
        }

        internal override bool DeepEqualsCore(KdlElement elementNode)
        {
            switch (elementNode)
            {
                case KdlValue value:
                    // KdlValue instances have special comparison semantics, dispatch to their implementation.
                    return value.DeepEqualsCore(this);
                case KdlNode node:
                    OrderedDictionary<KdlEntryKey, KdlElement?> currentDict = Dictionary;
                    OrderedDictionary<KdlEntryKey, KdlElement?> otherDict = node.Dictionary;

                    if (currentDict.Count != otherDict.Count)
                    {
                        return false;
                    }

                    foreach (KeyValuePair<KdlEntryKey, KdlElement?> item in currentDict)
                    {
                        otherDict.TryGetValue(item.Key, out KdlElement? jsonNode);

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


        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlNode"/> class that contains items from the specified <see cref="KdlReadOnlyElement"/>.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlNode"/> class that contains items from the specified <see cref="KdlReadOnlyElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="KdlReadOnlyElement"/>.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <exception cref="InvalidOperationException">
        ///   The <paramref name="element"/> is not a <see cref="KdlValueKind.Node"/>.
        /// </exception>
        public static KdlNode? Create(KdlReadOnlyElement element, KdlElementOptions? options = null)
        {
            return element.ValueKind switch
            {
                KdlValueKind.Null => null,
                KdlValueKind.Node => new KdlNode(element, options),
                _ => throw new InvalidOperationException(string.Format(SR.NodeElementWrongType, nameof(KdlValueKind.Node))),
            };
        }

        internal KdlNode(KdlReadOnlyElement element, KdlElementOptions? options = null) : base(options)
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
            KdlElement? nodeToAdd = ConvertFromValue(value, Options);
            Add(nodeToAdd);
        }


        internal override void GetPath(ref ValueStringBuilder path, KdlElement? child)
        {
            Parent?.GetPath(ref path, this);

            if (child != null)
            {
                //TECHDEBT: will need to handle entries other than property names
                var propertyName = FindValue(child)!.Value.Key.PropertyName;
                if (propertyName.AsSpan().ContainsSpecialCharacters())
                {
                    path.Append("['");
                    path.Append(propertyName);
                    path.Append("']");
                }
                else
                {
                    path.Append('.');
                    path.Append(propertyName);
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            GetUnderlyingRepresentation(out OrderedDictionary<KdlEntryKey, KdlElement?>? dictionary, out KdlReadOnlyElement? kdlElement);

            if (dictionary is null && kdlElement.HasValue)
            {
                // Write the element without converting to nodes.
                kdlElement.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<KdlEntryKey, KdlElement?> entry in Dictionary)
                {
                    writer.WritePropertyName(entry.Key.PropertyName ?? "");

                    if (entry.Value is null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        entry.Value.WriteTo(writer, options);
                    }
                }

                writer.WriteEndObject();
            }
        }

        private void DetachParentForDictionaryItem(KdlElement? item)
        {
            //TECHDEBT: Need to differentiate between cases. this may be true with properties
            //But may be avoided if only items?
            Debug.Assert(_dictionary != null, "Cannot have detachable nodes without a materialized dictionary.");
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
            private DebugViewEntry[] Entries
#pragma warning restore IDE0051 // Remove unused private members
            {
                get
                {
                    DebugViewEntry[] properties = new DebugViewEntry[_node.Count];

                    int i = 0;
                    foreach (var item in _node)
                    {
                        properties[i].EntryName = item.Key.Display;
                        properties[i].Value = item.Value;
                        i++;
                    }

                    return properties;
                }
            }

            [DebuggerDisplay("{Display,nq}")]
            private struct DebugViewEntry
            {
                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public KdlElement? Value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string EntryName;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public readonly string Display
                {
                    get
                    {
                        if (Value == null)
                        {
                            return $"{EntryName} = null";
                        }

                        if (Value is KdlValue)
                        {
                            return $"{EntryName} = {Value.ToKdlString()}";
                        }

                        if (Value is KdlNode kdlNode)
                        {
                            return $"{EntryName} = KdlNode[{kdlNode.Count}]";
                        }

                        KdlNode kdlNode2 = (KdlNode)Value;
                        return $"{EntryName} = KdlNode[{kdlNode2.Count}]";
                    }
                }
            }
        }
    }
}
