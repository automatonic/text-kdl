namespace System.Text.Kdl.Nodes
{
    public partial class KdlNode : IList<KeyValuePair<KdlEntryKey, KdlElement?>>
    {
        /// <summary>Gets the property the specified index.</summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <returns>The property at the specified index as a key/value pair.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        public KeyValuePair<KdlEntryKey, KdlElement?> GetAt(int index) => Dictionary.GetAt(index);

        /// <summary>Sets a new property at the specified index.</summary>
        /// <param name="index">The zero-based index of the property to set.</param>
        /// <param name="propertyName">The property name to store at the specified index.</param>
        /// <param name="value">The KDL value to store at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is already specified in a different index.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="value"/> already has a parent.</exception>
        public void SetAt(int index, KdlEntryKey propertyName, KdlElement? value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            OrderedDictionary<KdlEntryKey, KdlElement?> dictionary = Dictionary;
            KeyValuePair<KdlEntryKey, KdlElement?> existing = dictionary.GetAt(index);
            dictionary.SetAt(index, propertyName, value);
            DetachParentForDictionaryItem(existing.Value);
            value?.AssignParent(this);
        }

        /// <summary>Sets a new property value at the specified index.</summary>
        /// <param name="index">The zero-based index of the property to set.</param>
        /// <param name="value">The KDL value to store at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="value"/> already has a parent.</exception>
        public void SetAt(int index, KdlElement? value)
        {
            OrderedDictionary<KdlEntryKey, KdlElement?> dictionary = Dictionary;
            KeyValuePair<KdlEntryKey, KdlElement?> existing = dictionary.GetAt(index);
            dictionary.SetAt(index, value);
            DetachParentForDictionaryItem(existing.Value);
            value?.AssignParent(this);
        }

        /// <summary>Determines the index of a specific property name in the object.</summary>
        /// <param name="propertyName">The property name to locate.</param>
        /// <returns>The index of <paramref name="propertyName"/> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        public int IndexOf(KdlEntryKey propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            return Dictionary.IndexOf(propertyName);
        }

        /// <summary>Inserts a property into the object at the specified index.</summary>
        /// <param name="index">The zero-based index at which the property should be inserted.</param>
        /// <param name="propertyName">The property name to insert.</param>
        /// <param name="value">The KDL value to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="KdlNode"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.</exception>
        public void Insert(int index, KdlEntryKey propertyName, KdlElement? value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            Dictionary.Insert(index, propertyName, value);
            value?.AssignParent(this);
        }

        /// <inheritdoc />
        KeyValuePair<KdlEntryKey, KdlElement?> IList<KeyValuePair<KdlEntryKey, KdlElement?>>.this[int index]
        {
            get => GetAt(index);
            set => SetAt(index, value.Key, value.Value);
        }

        /// <inheritdoc />
        int IList<KeyValuePair<KdlEntryKey, KdlElement?>>.IndexOf(KeyValuePair<KdlEntryKey, KdlElement?> item) => ((IList<KeyValuePair<KdlEntryKey, KdlElement?>>)Dictionary).IndexOf(item);

        /// <inheritdoc />
        void IList<KeyValuePair<KdlEntryKey, KdlElement?>>.Insert(int index, KeyValuePair<KdlEntryKey, KdlElement?> item) => Insert(index, item.Key, item.Value);

        /// <inheritdoc />
        void IList<KeyValuePair<KdlEntryKey, KdlElement?>>.RemoveAt(int index)
        {
            KeyValuePair<KdlEntryKey, KdlElement?> existing = Dictionary.GetAt(index);
            Dictionary.RemoveAt(index);
            DetachParentForDictionaryItem(existing.Value);
        }
    }
}
