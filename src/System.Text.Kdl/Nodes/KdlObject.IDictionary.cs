using System.Collections;
using System.Collections.Generic;
using System.Text.Kdl.Serialization.Converters;
using System.Threading;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlObject : IDictionary<string, KdlNode?>
    {
        private OrderedDictionary<string, KdlNode?>? _dictionary;

        /// <summary>
        ///   Adds an element with the provided property name and value to the <see cref="KdlObject"/>.
        /// </summary>
        /// <param name="propertyName">The property name of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/>is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   An element with the same property name already exists in the <see cref="KdlObject"/>.
        /// </exception>
        public void Add(string propertyName, KdlNode? value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            Dictionary.Add(propertyName, value);
            value?.AssignParent(this);
        }

        /// <summary>
        ///   Adds the specified property to the <see cref="KdlObject"/>.
        /// </summary>
        /// <param name="property">
        ///   The KeyValuePair structure representing the property name and value to add to the <see cref="KdlObject"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   An element with the same property name already exists in the <see cref="KdlObject"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   The property name of <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public void Add(KeyValuePair<string, KdlNode?> property) => Add(property.Key, property.Value);

        /// <summary>
        ///   Removes all elements from the <see cref="KdlObject"/>.
        /// </summary>
        public void Clear()
        {
            OrderedDictionary<string, KdlNode?>? dictionary = _dictionary;

            if (dictionary is null)
            {
                _jsonElement = null;
                return;
            }

            foreach (KdlNode? node in dictionary.Values)
            {
                DetachParent(node);
            }

            dictionary.Clear();
        }

        /// <summary>
        ///   Determines whether the <see cref="KdlObject"/> contains an element with the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name to locate in the <see cref="KdlObject"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlObject"/> contains an element with the specified property name; otherwise, <see langword="false"/>.
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
        ///   Gets the number of elements contained in <see cref="KdlObject"/>.
        /// </summary>
        public int Count => Dictionary.Count;

        /// <summary>
        ///   Removes the element with the specified property name from the <see cref="KdlObject"/>.
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

            bool success = Dictionary.Remove(propertyName, out KdlNode? removedNode);
            if (success)
            {
                DetachParent(removedNode);
            }

            return success;
        }

        /// <summary>
        ///   Determines whether the <see cref="KdlObject"/> contains a specific property name and <see cref="KdlNode"/> reference.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="KdlObject"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlObject"/> contains an element with the property name; otherwise, <see langword="false"/>.
        /// </returns>
        bool ICollection<KeyValuePair<string, KdlNode?>>.Contains(KeyValuePair<string, KdlNode?> item) =>
            ((IDictionary<string, KdlNode?>)Dictionary).Contains(item);

        /// <summary>
        ///   Copies the elements of the <see cref="KdlObject"/> to an array of type KeyValuePair starting at the specified array index.
        /// </summary>
        /// <param name="array">
        ///   The one-dimensional Array that is the destination of the elements copied from <see cref="KdlObject"/>.
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
        void ICollection<KeyValuePair<string, KdlNode?>>.CopyTo(KeyValuePair<string, KdlNode?>[] array, int index) =>
            ((IDictionary<string, KdlNode?>)Dictionary).CopyTo(array, index);

        /// <summary>
        ///   Returns an enumerator that iterates through the <see cref="KdlObject"/>.
        /// </summary>
        /// <returns>
        ///   An enumerator that iterates through the <see cref="KdlObject"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<string, KdlNode?>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        ///   Removes a key and value from the <see cref="KdlObject"/>.
        /// </summary>
        /// <param name="item">
        ///   The KeyValuePair structure representing the property name and value to remove from the <see cref="KdlObject"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.
        /// </returns>
        bool ICollection<KeyValuePair<string, KdlNode?>>.Remove(KeyValuePair<string, KdlNode?> item) => Remove(item.Key);

        /// <summary>
        ///   Gets a collection containing the property names in the <see cref="KdlObject"/>.
        /// </summary>
        ICollection<string> IDictionary<string, KdlNode?>.Keys => Dictionary.Keys;

        /// <summary>
        ///   Gets a collection containing the property values in the <see cref="KdlObject"/>.
        /// </summary>
        ICollection<KdlNode?> IDictionary<string, KdlNode?>.Values => Dictionary.Values;

        /// <summary>
        ///   Gets the value associated with the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name of the value to get.</param>
        /// <param name="jsonNode">
        ///   When this method returns, contains the value associated with the specified property name, if the property name is found;
        ///   otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="KdlObject"/> contains an element with the specified property name; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        bool IDictionary<string, KdlNode?>.TryGetValue(string propertyName, out KdlNode? jsonNode)
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
        bool ICollection<KeyValuePair<string, KdlNode?>>.IsReadOnly => false;

        /// <summary>
        ///   Returns an enumerator that iterates through the <see cref="KdlObject"/>.
        /// </summary>
        /// <returns>
        ///   An enumerator that iterates through the <see cref="KdlObject"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        private OrderedDictionary<string, KdlNode?> InitializeDictionary()
        {
            GetUnderlyingRepresentation(out OrderedDictionary<string, KdlNode?>? dictionary, out KdlElement? jsonElement);

            if (dictionary is null)
            {
                dictionary = CreateDictionary(Options);

                if (jsonElement.HasValue)
                {
                    foreach (KdlProperty jElementProperty in jsonElement.Value.EnumerateObject())
                    {
                        KdlNode? node = KdlNodeConverter.Create(jElementProperty.Value, Options);
                        if (node != null)
                        {
                            node.Parent = this;
                        }

                        dictionary.Add(jElementProperty.Name, node);
                    }
                }

                // Ensure _jsonElement is written to after _dictionary
                _dictionary = dictionary;
                Interlocked.MemoryBarrier();
                _jsonElement = null;
            }

            return dictionary;
        }

        private static OrderedDictionary<string, KdlNode?> CreateDictionary(KdlNodeOptions? options, int capacity = 0)
        {
            StringComparer comparer = options?.PropertyNameCaseInsensitive ?? false
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            return new(capacity, comparer);
        }

        /// <summary>
        /// Provides a coherent view of the underlying representation of the current node.
        /// The jsonElement value should be consumed if and only if dictionary value is null.
        /// </summary>
        private void GetUnderlyingRepresentation(out OrderedDictionary<string, KdlNode?>? dictionary, out KdlElement? jsonElement)
        {
            // Because KdlElement cannot be read atomically there might be torn reads,
            // however the order of read/write operations guarantees that that's only
            // possible if the value of _dictionary is non-null.
            jsonElement = _jsonElement;
            Interlocked.MemoryBarrier();
            dictionary = _dictionary;
        }
    }
}
