// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Converters;
using System.Threading;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   Represents a mutable KDL array.
    /// </summary>
    /// <remarks>
    /// It is safe to perform multiple concurrent read operations on a <see cref="KdlArray"/>,
    /// but issues can occur if the collection is modified while it's being read.
    /// </remarks>
    [DebuggerDisplay("KdlArray[{List.Count}]")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class KdlArray : KdlNode
    {
        private KdlElement? _jsonElement;
        private List<KdlNode?>? _list;

        internal override KdlElement? UnderlyingElement => _jsonElement;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlArray"/> class that is empty.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        public KdlArray(KdlNodeOptions? options = null) : base(options) { }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlArray"/> class that contains items from the specified params array.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        /// <param name="items">The items to add to the new <see cref="KdlArray"/>.</param>
        public KdlArray(KdlNodeOptions options, params KdlNode?[] items) : base(options)
        {
            InitializeFromArray(items);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlArray"/> class that contains items from the specified params span.
        /// </summary>
        /// <param name="options">Options to control the behavior.</param>
        /// <param name="items">The items to add to the new <see cref="KdlArray"/>.</param>
        public KdlArray(KdlNodeOptions options, params ReadOnlySpan<KdlNode?> items) : base(options)
        {
            InitializeFromSpan(items);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlArray"/> class that contains items from the specified array.
        /// </summary>
        /// <param name="items">The items to add to the new <see cref="KdlArray"/>.</param>
        public KdlArray(params KdlNode?[] items) : base()
        {
            InitializeFromArray(items);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlArray"/> class that contains items from the specified span.
        /// </summary>
        /// <param name="items">The items to add to the new <see cref="KdlArray"/>.</param>
        public KdlArray(params ReadOnlySpan<KdlNode?> items) : base()
        {
            InitializeFromSpan(items);
        }

        private protected override KdlValueKind GetValueKindCore() => KdlValueKind.Array;

        internal override KdlNode DeepCloneCore()
        {
            GetUnderlyingRepresentation(out List<KdlNode?>? list, out KdlElement? jsonElement);

            if (list is null)
            {
                return jsonElement.HasValue
                    ? new KdlArray(jsonElement.Value.Clone(), Options)
                    : new KdlArray(Options);
            }

            var jsonArray = new KdlArray(Options)
            {
                _list = new List<KdlNode?>(list.Count)
            };

            for (int i = 0; i < list.Count; i++)
            {
                jsonArray.Add(list[i]?.DeepCloneCore());
            }

            return jsonArray;
        }

        internal override bool DeepEqualsCore(KdlNode node)
        {
            switch (node)
            {
                case KdlObject:
                    return false;
                case KdlValue value:
                    // KdlValue instances have special comparison semantics, dispatch to their implementation.
                    return value.DeepEqualsCore(this);
                case KdlArray array:
                    List<KdlNode?> currentList = List;
                    List<KdlNode?> otherList = array.List;

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

                    return true;
                default:
                    Debug.Fail("Impossible case");
                    return false;
            }
        }

        internal int GetElementIndex(KdlNode? node)
        {
            return List.IndexOf(node);
        }

        /// <summary>
        /// Returns an enumerable that wraps calls to <see cref="KdlNode.GetValue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to obtain from the <see cref="KdlValue"/>.</typeparam>
        /// <returns>An enumerable iterating over values of the array.</returns>
        public IEnumerable<T> GetValues<T>()
        {
            foreach (KdlNode? item in List)
            {
                yield return item is null ? (T)(object?)null! : item.GetValue<T>();
            }
        }

        private void InitializeFromArray(KdlNode?[] items)
        {
            var list = new List<KdlNode?>(items);

            for (int i = 0; i < list.Count; i++)
            {
                list[i]?.AssignParent(this);
            }

            _list = list;
        }

        private void InitializeFromSpan(ReadOnlySpan<KdlNode?> items)
        {
            List<KdlNode?> list = new(items.Length);

#if NET8_0_OR_GREATER
            list.AddRange(items);
#else
            foreach (KdlNode? item in items)
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
        ///   Initializes a new instance of the <see cref="KdlArray"/> class that contains items from the specified <see cref="KdlElement"/>.
        /// </summary>
        /// <returns>
        ///   The new instance of the <see cref="KdlArray"/> class that contains items from the specified <see cref="KdlElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="KdlElement"/>.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <exception cref="InvalidOperationException">
        ///   The <paramref name="element"/> is not a <see cref="KdlValueKind.Array"/>.
        /// </exception>
        public static KdlArray? Create(KdlElement element, KdlNodeOptions? options = null)
        {
            return element.ValueKind switch
            {
                KdlValueKind.Null => null,
                KdlValueKind.Array => new KdlArray(element, options),
                _ => throw new InvalidOperationException(string.Format(SR.NodeElementWrongType, nameof(KdlValueKind.Array))),
            };
        }

        internal KdlArray(KdlElement element, KdlNodeOptions? options = null) : base(options)
        {
            Debug.Assert(element.ValueKind == KdlValueKind.Array);
            _jsonElement = element;
        }

        /// <summary>
        ///   Adds an object to the end of the <see cref="KdlArray"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to be added.</typeparam>
        /// <param name="value">
        ///   The object to be added to the end of the <see cref="KdlArray"/>.
        /// </param>
        [RequiresUnreferencedCode(KdlValue.CreateUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlValue.CreateDynamicCodeMessage)]
        public void Add<T>(T? value)
        {
            KdlNode? nodeToAdd = ConvertFromValue(value, Options);
            Add(nodeToAdd);
        }

        /// <summary>
        /// Gets or creates the underlying list containing the element nodes of the array.
        /// </summary>
        private List<KdlNode?> List => _list ?? InitializeList();

        private protected override KdlNode? GetItem(int index)
        {
            return List[index];
        }

        private protected override void SetItem(int index, KdlNode? value)
        {
            value?.AssignParent(this);
            DetachParent(List[index]);
            List[index] = value;
        }

        internal override void GetPath(ref ValueStringBuilder path, KdlNode? child)
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
                path.Append(chars.Slice(0, charsWritten));
#else
                path.Append(index.ToString());
#endif
                path.Append(']');
            }
        }

        /// <inheritdoc/>
        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            GetUnderlyingRepresentation(out List<KdlNode?>? list, out KdlElement? jsonElement);

            if (list is null && jsonElement.HasValue)
            {
                jsonElement.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteStartArray();

                foreach (KdlNode? element in List)
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
        }

        private List<KdlNode?> InitializeList()
        {
            GetUnderlyingRepresentation(out List<KdlNode?>? list, out KdlElement? jsonElement);

            if (list is null)
            {
                if (jsonElement.HasValue)
                {
                    KdlElement jElement = jsonElement.Value;
                    Debug.Assert(jElement.ValueKind == KdlValueKind.Array);

                    list = new List<KdlNode?>(jElement.GetArrayLength());

                    foreach (KdlElement element in jElement.EnumerateArray())
                    {
                        KdlNode? node = KdlNodeConverter.Create(element, Options);
                        node?.AssignParent(this);
                        list.Add(node);
                    }
                }
                else
                {
                    list = new();
                }

                // Ensure _jsonElement is written to after _list
                _list = list;
                Interlocked.MemoryBarrier();
                _jsonElement = null;
            }

            return list;
        }

        /// <summary>
        /// Provides a coherent view of the underlying representation of the current node.
        /// The jsonElement value should be consumed if and only if the list value is null.
        /// </summary>
        private void GetUnderlyingRepresentation(out List<KdlNode?>? list, out KdlElement? jsonElement)
        {
            // Because KdlElement cannot be read atomically there might be torn reads,
            // however the order of read/write operations guarantees that that's only
            // possible if the value of _list is non-null.
            jsonElement = _jsonElement;
            Interlocked.MemoryBarrier();
            list = _list;
        }

        [ExcludeFromCodeCoverage] // Justification = "Design-time"
        private sealed class DebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly KdlArray _node;

            public DebugView(KdlArray node)
            {
                _node = node;
            }

            public string Kdl => _node.ToKdlString();
            public string Path => _node.GetPath();

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DebugViewItem[] Items
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

            [DebuggerDisplay("{Display,nq}")]
            private struct DebugViewItem
            {
                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public KdlNode? Value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string Display
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

                        if (Value is KdlObject jsonObject)
                        {
                            return $"KdlObject[{jsonObject.Count}]";
                        }

                        KdlArray jsonArray = (KdlArray)Value;
                        return $"KdlArray[{jsonArray.List.Count}]";
                    }
                }
            }
        }
    }
}
