using System.Collections;
using System.Text.Kdl.Serialization.Converters;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlNode : IDictionary<KdlEntryKey, KdlElement?>
    {
        private OrderedDictionary<KdlEntryKey, KdlElement?>? _dictionary;

        /// <summary>
        ///   Adds an element with the provided property name and value to the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="key">The property name of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="key"/>is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   An element with the same property name already exists in the <see cref="KdlNode"/>.
        /// </exception>
        public void Add(KdlEntryKey key, KdlElement? value)
        {
            if (key is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(key));
            }

            Dictionary.Add(key, value);
            value?.AssignParent(this);
        }

        /// <summary>
        ///   Adds the specified property to the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="property">
        ///   The KeyValuePair structure representing the property name and value to add to the <see cref="KdlNode"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   An element with the same property name already exists in the <see cref="KdlNode"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   The property name of <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public void Add(KeyValuePair<KdlEntryKey, KdlElement?> property) => Add(property.Key, property.Value);

        /// <summary>
        ///   Determines whether the <see cref="KdlNode"/> contains an element with the specified property name.
        /// </summary>
        /// <param name="key">The property name to locate in the <see cref="KdlNode"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlNode"/> contains an element with the specified property name; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public bool ContainsKey(KdlEntryKey key)
        {
            if (key is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(key));
            }

            return Dictionary.ContainsKey(key);
        }

        /// <summary>
        ///   Gets the number of elements contained in <see cref="KdlNode"/>.
        /// </summary>
        public int Count => Dictionary.Count;

        /// <summary>
        ///   Removes the element with the specified property name from the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="key">The property name of the element to remove.</param>
        /// <returns>
        ///   <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public bool Remove(KdlEntryKey key)
        {
            if (key is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(key));
            }

            bool success = Dictionary.Remove(key, out KdlElement? removedNode);
            if (success)
            {
                DetachParentForDictionaryItem(removedNode);
            }

            return success;
        }

        /// <summary>
        ///   Determines whether the <see cref="KdlNode"/> contains a specific property name and <see cref="KdlElement"/> reference.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="KdlNode"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlNode"/> contains an element with the property name; otherwise, <see langword="false"/>.
        /// </returns>
        bool ICollection<KeyValuePair<KdlEntryKey, KdlElement?>>.Contains(KeyValuePair<KdlEntryKey, KdlElement?> item) =>
            ((IDictionary<KdlEntryKey, KdlElement?>)Dictionary).Contains(item);

        /// <summary>
        ///   Copies the elements of the <see cref="KdlNode"/> to an array of type KeyValuePair starting at the specified array index.
        /// </summary>
        /// <param name="array">
        ///   The one-dimensional Array that is the destination of the elements copied from <see cref="KdlNode"/>.
        /// </param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The number of elements in the source ICollection is greater than the available space from <paramref name="index"/>
        ///   to the end of the destination <paramref name="array"/>.
        /// </exception>
        void ICollection<KeyValuePair<KdlEntryKey, KdlElement?>>.CopyTo(KeyValuePair<KdlEntryKey, KdlElement?>[] array, int index) =>
            ((IDictionary<KdlEntryKey, KdlElement?>)Dictionary).CopyTo(array, index);

        /// <summary>
        ///   Returns an enumerator that iterates through the <see cref="KdlNode"/>.
        /// </summary>
        /// <returns>
        ///   An enumerator that iterates through the <see cref="KdlNode"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<KdlEntryKey, KdlElement?>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        ///   Removes a key and value from the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="item">
        ///   The KeyValuePair structure representing the property name and value to remove from the <see cref="KdlNode"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.
        /// </returns>
        bool ICollection<KeyValuePair<KdlEntryKey, KdlElement?>>.Remove(KeyValuePair<KdlEntryKey, KdlElement?> item) => Remove(item.Key);

        /// <summary>
        ///   Gets a collection containing the property names in the <see cref="KdlNode"/>.
        /// </summary>
        ICollection<KdlEntryKey> IDictionary<KdlEntryKey, KdlElement?>.Keys => Dictionary.Keys;

        /// <summary>
        ///   Gets a collection containing the property values in the <see cref="KdlNode"/>.
        /// </summary>
        ICollection<KdlElement?> IDictionary<KdlEntryKey, KdlElement?>.Values => Dictionary.Values;

        /// <summary>
        ///   Gets the value associated with the specified property name.
        /// </summary>
        /// <param name="key">The property name of the value to get.</param>
        /// <param name="kdlVertex">
        ///   When this method returns, contains the value associated with the specified property name, if the property name is found;
        ///   otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlNode"/> contains an element with the specified property name; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        bool IDictionary<KdlEntryKey, KdlElement?>.TryGetValue(KdlEntryKey key, out KdlElement? kdlVertex)
        {
            if (key is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(key));
            }

            return Dictionary.TryGetValue(key, out kdlVertex);
        }

        /// <summary>
        ///   Returns <see langword="false"/>.
        /// </summary>
        bool ICollection<KeyValuePair<KdlEntryKey, KdlElement?>>.IsReadOnly => false;

        private OrderedDictionary<KdlEntryKey, KdlElement?> InitializeDictionary()
        {
            GetUnderlyingRepresentation(out OrderedDictionary<KdlEntryKey, KdlElement?>? dictionary, out KdlReadOnlyElement? kdlElement);

            if (dictionary is null)
            {
                dictionary = CreateDictionary(Options);

                if (kdlElement.HasValue)
                {
                    foreach (IKdlEntry jElementProperty in kdlElement.Value.EnumerateNode())
                    {
                        KdlElement? node = KdlVertexConverter.Create(jElementProperty.Value, Options);
                        if (node != null)
                        {
                            node.Parent = this;
                        }

                        dictionary.Add(jElementProperty.Key, node);
                    }
                }

                // Ensure _kdlElement is written to after _dictionary
                _dictionary = dictionary;
                Interlocked.MemoryBarrier();
                _kdlElement = null;
            }

            return dictionary;
        }

        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static OrderedDictionary<KdlEntryKey, KdlElement?> CreateDictionary(KdlElementOptions? options, int capacity = 0)
        {
            //TECHDEBT: Need a key comparer
            // StringComparer comparer = options?.PropertyNameCaseInsensitive ?? false
            //     ? StringComparer.OrdinalIgnoreCase
            //     : StringComparer.Ordinal;

            // return new(capacity, comparer);
            return new(capacity);
        }

        /// <summary>
        /// Provides a coherent view of the underlying representation of the current node.
        /// The kdlElement value should be consumed if and only if dictionary value is null.
        /// </summary>
        private void GetUnderlyingRepresentation(out OrderedDictionary<KdlEntryKey, KdlElement?>? dictionary, out KdlReadOnlyElement? kdlElement)
        {
            // Because KdlElement cannot be read atomically there might be torn reads,
            // however the order of read/write operations guarantees that that's only
            // possible if the value of _dictionary is non-null.
            kdlElement = _kdlElement;
            Interlocked.MemoryBarrier();
            dictionary = _dictionary;
        }

        public void Clear() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
