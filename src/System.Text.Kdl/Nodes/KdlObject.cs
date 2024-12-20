using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   Represents a mutable KDL object.
    /// </summary>
    /// <remarks>
    /// It's safe to perform multiple concurrent read operations on a <see cref="KdlObject"/>,
    /// but issues can occur if the collection is modified while it's being read.
    /// </remarks>
    [DebuggerDisplay("KdlObject[{Count}]")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class KdlObject : KdlNode
    {
        private KdlElement? _jsonElement;

        internal override KdlElement? UnderlyingElement => _jsonElement;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlObject"/> class that is empty.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        public KdlObject(KdlNodeOptions? options = null) : base(options) { }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlObject"/> class that contains the specified <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">The properties to be added.</param>
        /// <param name="options">Options to control the behavior.</param>
        public KdlObject(IEnumerable<KeyValuePair<string, KdlNode?>> properties, KdlNodeOptions? options = null) : this(options)
        {
            int capacity = properties is ICollection<KeyValuePair<string, KdlNode?>> propertiesCollection ? propertiesCollection.Count : 0;
            OrderedDictionary<string, KdlNode?> dictionary = CreateDictionary(options, capacity);

            foreach (KeyValuePair<string, KdlNode?> node in properties)
            {
                dictionary.Add(node.Key, node.Value);
                node.Value?.AssignParent(this);
            }

            _dictionary = dictionary;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlObject"/> class that contains properties from the specified <see cref="KdlElement"/>.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlObject"/> class that contains properties from the specified <see cref="KdlElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="KdlElement"/>.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>A <see cref="KdlObject"/>.</returns>
        public static KdlObject? Create(KdlElement element, KdlNodeOptions? options = null)
        {
            return element.ValueKind switch
            {
                KdlValueKind.Null => null,
                KdlValueKind.Object => new KdlObject(element, options),
                _ => throw new InvalidOperationException(string.Format(SR.NodeElementWrongType, nameof(KdlValueKind.Object)))
            };
        }

        internal KdlObject(KdlElement element, KdlNodeOptions? options = null) : this(options)
        {
            Debug.Assert(element.ValueKind == KdlValueKind.Object);
            _jsonElement = element;
        }

        /// <summary>
        /// Gets or creates the underlying dictionary containing the properties of the object.
        /// </summary>
        private OrderedDictionary<string, KdlNode?> Dictionary => _dictionary ?? InitializeDictionary();

        private protected override KdlNode? GetItem(int index) => GetAt(index).Value;
        private protected override void SetItem(int index, KdlNode? value) => SetAt(index, value);

        internal override KdlNode DeepCloneCore()
        {
            GetUnderlyingRepresentation(out OrderedDictionary<string, KdlNode?>? dictionary, out KdlElement? jsonElement);

            if (dictionary is null)
            {
                return jsonElement.HasValue
                    ? new KdlObject(jsonElement.Value.Clone(), Options)
                    : new KdlObject(Options);
            }

            var jObject = new KdlObject(Options)
            {
                _dictionary = CreateDictionary(Options, Count)
            };

            foreach (KeyValuePair<string, KdlNode?> item in dictionary)
            {
                jObject.Add(item.Key, item.Value?.DeepCloneCore());
            }

            return jObject;
        }

        internal string GetPropertyName(KdlNode? node)
        {
            KeyValuePair<string, KdlNode?>? item = FindValue(node);
            return item.HasValue ? item.Value.Key : string.Empty;
        }

        /// <summary>
        ///   Returns the value of a property with the specified name.
        /// </summary>
        /// <param name="propertyName">The name of the property to return.</param>
        /// <param name="jsonNode">The KDL value of the property with the specified name.</param>
        /// <returns>
        ///   <see langword="true"/> if a property with the specified name was found; otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetPropertyValue(string propertyName, out KdlNode? jsonNode)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            return Dictionary.TryGetValue(propertyName, out jsonNode);
        }

        /// <inheritdoc/>
        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            GetUnderlyingRepresentation(out OrderedDictionary<string, KdlNode?>? dictionary, out KdlElement? jsonElement);

            if (dictionary is null && jsonElement.HasValue)
            {
                // Write the element without converting to nodes.
                jsonElement.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<string, KdlNode?> entry in Dictionary)
                {
                    writer.WritePropertyName(entry.Key);

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

        private protected override KdlValueKind GetValueKindCore() => KdlValueKind.Object;

        internal override bool DeepEqualsCore(KdlNode node)
        {
            switch (node)
            {
                case KdlArray:
                    return false;
                case KdlValue value:
                    // KdlValue instances have special comparison semantics, dispatch to their implementation.
                    return value.DeepEqualsCore(this);
                case KdlObject jsonObject:
                    OrderedDictionary<string, KdlNode?> currentDict = Dictionary;
                    OrderedDictionary<string, KdlNode?> otherDict = jsonObject.Dictionary;

                    if (currentDict.Count != otherDict.Count)
                    {
                        return false;
                    }

                    foreach (KeyValuePair<string, KdlNode?> item in currentDict)
                    {
                        otherDict.TryGetValue(item.Key, out KdlNode? jsonNode);

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

        internal KdlNode? GetItem(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            if (TryGetPropertyValue(propertyName, out KdlNode? value))
            {
                return value;
            }

            // Return null for missing properties.
            return null;
        }

        internal override void GetPath(ref ValueStringBuilder path, KdlNode? child)
        {
            Parent?.GetPath(ref path, this);

            if (child != null)
            {
                string propertyName = FindValue(child)!.Value.Key;
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

        internal void SetItem(string propertyName, KdlNode? value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            OrderedDictionary<string, KdlNode?> dict = Dictionary;

            if (
#if NET10_0_OR_GREATER
                !dict.TryAdd(propertyName, value, out int index)
#else
                !dict.TryAdd(propertyName, value)
#endif
                )
            {
#if !NET10_0_OR_GREATER
                int index = dict.IndexOf(propertyName);
#endif
                Debug.Assert(index >= 0);
                KdlNode? replacedValue = dict.GetAt(index).Value;

                if (ReferenceEquals(value, replacedValue))
                {
                    return;
                }

                DetachParent(replacedValue);
                dict.SetAt(index, value);
            }

            value?.AssignParent(this);
        }

        private void DetachParent(KdlNode? item)
        {
            Debug.Assert(_dictionary != null, "Cannot have detachable nodes without a materialized dictionary.");

            if (item != null)
            {
                item.Parent = null;
            }
        }

        private KeyValuePair<string, KdlNode?>? FindValue(KdlNode? value)
        {
            foreach (KeyValuePair<string, KdlNode?> item in Dictionary)
            {
                if (ReferenceEquals(item.Value, value))
                {
                    return item;
                }
            }

            return null;
        }

        [ExcludeFromCodeCoverage] // Justification = "Design-time"
        private sealed class DebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly KdlObject _node;

            public DebugView(KdlObject node)
            {
                _node = node;
            }

            public string Kdl => _node.ToKdlString();
            public string Path => _node.GetPath();

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DebugViewProperty[] Items
            {
                get
                {
                    DebugViewProperty[] properties = new DebugViewProperty[_node.Count];

                    int i = 0;
                    foreach (KeyValuePair<string, KdlNode?> item in _node)
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
                public KdlNode? Value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string PropertyName;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string Display
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

                        if (Value is KdlObject jsonObject)
                        {
                            return $"{PropertyName} = KdlObject[{jsonObject.Count}]";
                        }

                        KdlArray jsonArray = (KdlArray)Value;
                        return $"{PropertyName} = KdlArray[{jsonArray.Count}]";
                    }
                }

            }
        }
    }
}
