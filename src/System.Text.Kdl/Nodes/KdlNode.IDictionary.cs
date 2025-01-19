using System.Text.Kdl.Serialization.Converters;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlNode : IDictionary<string, KdlVertex?>
    {
        private OrderedDictionary<string, KdlVertex?>? _dictionary;

        /// <summary>
        ///   Adds an element with the provided property name and value to the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="propertyName">The property name of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/>is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   An element with the same property name already exists in the <see cref="KdlNode"/>.
        /// </exception>
        public void Add(string propertyName, KdlVertex? value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            Dictionary.Add(propertyName, value);
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
        public void Add(KeyValuePair<string, KdlVertex?> property) => Add(property.Key, property.Value);

        /// <summary>
        ///   Determines whether the <see cref="KdlNode"/> contains an element with the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name to locate in the <see cref="KdlNode"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlNode"/> contains an element with the specified property name; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        public bool ContainsKey(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            return Dictionary.ContainsKey(propertyName);
        }

        /// <summary>
        ///   Gets the number of elements contained in <see cref="KdlNode"/>.
        /// </summary>
        public int Count => Dictionary.Count + List.Count;

        /// <summary>
        ///   Removes the element with the specified property name from the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="propertyName">The property name of the element to remove.</param>
        /// <returns>
        ///   <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        public bool Remove(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            bool success = Dictionary.Remove(propertyName, out KdlVertex? removedNode);
            if (success)
            {
                DetachParentForDictionaryItem(removedNode);
            }

            return success;
        }

        /// <summary>
        ///   Determines whether the <see cref="KdlNode"/> contains a specific property name and <see cref="KdlVertex"/> reference.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="KdlNode"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlNode"/> contains an element with the property name; otherwise, <see langword="false"/>.
        /// </returns>
        bool ICollection<KeyValuePair<string, KdlVertex?>>.Contains(KeyValuePair<string, KdlVertex?> item) =>
            ((IDictionary<string, KdlVertex?>)Dictionary).Contains(item);

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
        void ICollection<KeyValuePair<string, KdlVertex?>>.CopyTo(KeyValuePair<string, KdlVertex?>[] array, int index) =>
            ((IDictionary<string, KdlVertex?>)Dictionary).CopyTo(array, index);

        /// <summary>
        ///   Returns an enumerator that iterates through the <see cref="KdlNode"/>.
        /// </summary>
        /// <returns>
        ///   An enumerator that iterates through the <see cref="KdlNode"/>.
        /// </returns>
        IEnumerator<KeyValuePair<string, KdlVertex?>> IEnumerable<KeyValuePair<string, KdlVertex?>>.GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        ///   Removes a key and value from the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="item">
        ///   The KeyValuePair structure representing the property name and value to remove from the <see cref="KdlNode"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.
        /// </returns>
        bool ICollection<KeyValuePair<string, KdlVertex?>>.Remove(KeyValuePair<string, KdlVertex?> item) => Remove(item.Key);

        /// <summary>
        ///   Gets a collection containing the property names in the <see cref="KdlNode"/>.
        /// </summary>
        ICollection<string> IDictionary<string, KdlVertex?>.Keys => Dictionary.Keys;

        /// <summary>
        ///   Gets a collection containing the property values in the <see cref="KdlNode"/>.
        /// </summary>
        ICollection<KdlVertex?> IDictionary<string, KdlVertex?>.Values => Dictionary.Values;

        /// <summary>
        ///   Gets the value associated with the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name of the value to get.</param>
        /// <param name="jsonNode">
        ///   When this method returns, contains the value associated with the specified property name, if the property name is found;
        ///   otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlNode"/> contains an element with the specified property name; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        bool IDictionary<string, KdlVertex?>.TryGetValue(string propertyName, out KdlVertex? jsonNode)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            return Dictionary.TryGetValue(propertyName, out jsonNode);
        }

        /// <summary>
        ///   Returns <see langword="false"/>.
        /// </summary>
        bool ICollection<KeyValuePair<string, KdlVertex?>>.IsReadOnly => false;


        private OrderedDictionary<string, KdlVertex?> InitializeDictionary()
        {
            GetUnderlyingRepresentation(out OrderedDictionary<string, KdlVertex?>? dictionary, out KdlElement? kdlElement);

            if (dictionary is null)
            {
                dictionary = CreateDictionary(Options);

                if (kdlElement.HasValue)
                {
                    foreach (KdlProperty jElementProperty in kdlElement.Value.EnumerateObject())
                    {
                        KdlVertex? node = KdlNodeConverter.Create(jElementProperty.Value, Options);
                        if (node != null)
                        {
                            node.Parent = this;
                        }

                        dictionary.Add(jElementProperty.Name, node);
                    }
                }

                // Ensure _kdlElement is written to after _dictionary
                _dictionary = dictionary;
                Interlocked.MemoryBarrier();
                _kdlElement = null;
            }

            return dictionary;
        }

        private static OrderedDictionary<string, KdlVertex?> CreateDictionary(KdlNodeOptions? options, int capacity = 0)
        {
            StringComparer comparer = options?.PropertyNameCaseInsensitive ?? false
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            return new(capacity, comparer);
        }

        /// <summary>
        /// Provides a coherent view of the underlying representation of the current node.
        /// The kdlElement value should be consumed if and only if dictionary value is null.
        /// </summary>
        private void GetUnderlyingRepresentation(out OrderedDictionary<string, KdlVertex?>? dictionary, out KdlElement? kdlElement)
        {
            // Because KdlElement cannot be read atomically there might be torn reads,
            // however the order of read/write operations guarantees that that's only
            // possible if the value of _dictionary is non-null.
            kdlElement = _kdlElement;
            Interlocked.MemoryBarrier();
            dictionary = _dictionary;
        }
    }
}
