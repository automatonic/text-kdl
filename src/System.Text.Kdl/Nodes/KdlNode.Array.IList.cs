using System.Collections;
using System.Diagnostics;

namespace System.Text.Kdl.Nodes
{
    public sealed partial class KdlNode : KdlVertex, IList<KdlVertex?>
    {
        /// <summary>
        ///   Adds a <see cref="KdlVertex"/> to the end of the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="item">
        ///   The <see cref="KdlVertex"/> to be added to the end of the <see cref="KdlNode"/>.
        /// </param>
        public void Add(KdlVertex? item)
        {
            item?.AssignParent(this);

            List.Add(item);
        }

        /// <summary>
        ///   Removes all elements from the <see cref="KdlNode"/>.
        /// </summary>
        public void Clear()
        {
            List<KdlVertex?>? list = _list;

            if (list is null)
            {
                _kdlElement = null;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DetachParentForListItem(list[i]);
                }

                list.Clear();
            }

            OrderedDictionary<string, KdlVertex?>? dictionary = _dictionary;

            if (dictionary is null)
            {
                _kdlElement = null;
                return;
            }

            foreach (KdlVertex? node in dictionary.Values)
            {
                DetachParentForDictionaryItem(node);
            }

            dictionary.Clear();
        }

        /// <summary>
        ///   Determines whether an element is in the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="KdlNode"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if <paramref name="item"/> is found in the <see cref="KdlNode"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(KdlVertex? item) => List.Contains(item);

        /// <summary>
        ///   The object to locate in the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="item">The <see cref="KdlVertex"/> to locate in the <see cref="KdlNode"/>.</param>
        /// <returns>
        ///  The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(KdlVertex? item) => List.IndexOf(item);

        /// <summary>
        ///   Inserts an element into the <see cref="KdlNode"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="KdlVertex"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> is less than 0 or <paramref name="index"/> is greater than <see cref="Count"/>.
        /// </exception>
        public void Insert(int index, KdlVertex? item)
        {
            item?.AssignParent(this);
            List.Insert(index, item);
        }

        /// <summary>
        ///   Removes the first occurrence of a specific <see cref="KdlVertex"/> from the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="item">
        ///   The <see cref="KdlVertex"/> to remove from the <see cref="KdlNode"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if <paramref name="item"/> is successfully removed; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Remove(KdlVertex? item)
        {
            if (List.Remove(item))
            {
                DetachParentForListItem(item);
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Removes the element at the specified index of the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> is less than 0 or <paramref name="index"/> is greater than <see cref="Count"/>.
        /// </exception>
        public void RemoveAt(int index)
        {
            KdlVertex? item = List[index];
            List.RemoveAt(index);
            DetachParentForListItem(item);
        }

        /// <summary>
        ///   Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The predicate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the <see cref="KdlNode"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public int RemoveAll(Func<KdlVertex?, bool> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(match));
            }

            return List.RemoveAll(node =>
            {
                if (match(node))
                {
                    DetachParentForListItem(node);
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        /// <summary>
        ///   Removes a range of elements from the <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> or <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="KdlNode"/>.
        /// </exception>
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(index));
            }

            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(count));
            }

            List<KdlVertex?> list = List;

            if (list.Count - index < count)
            {
                ThrowHelper.ThrowArgumentException_InvalidOffLen();
            }

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DetachParentForListItem(list[index + i]);
                    // There's no need to assign nulls because List<>.RemoveRange calls
                    // Array.Clear on the removed partition.
                }

                list.RemoveRange(index, count);
            }
        }

        #region Explicit interface implementation

        /// <summary>
        ///   Copies the entire <see cref="Array"/> to a compatible one-dimensional array,
        ///   starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">
        ///   The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        ///   from <see cref="KdlNode"/>. The Array must have zero-based indexing.</param>
        /// <param name="index">
        ///   The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
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
        void ICollection<KdlVertex?>.CopyTo(KdlVertex?[] array, int index) => List.CopyTo(array, index);

        /// <summary>
        ///   Returns an enumerator that iterates through the <see cref="KdlNode"/>.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{KdlVertex}"/> for the <see cref="KdlVertex"/>.</returns>
        public IEnumerator<KdlVertex?> GetEnumerator() => List.Concat(Dictionary.Values).GetEnumerator();

        /// <summary>
        ///   Returns an enumerator that iterates through the <see cref="KdlNode"/>.
        /// </summary>
        /// <returns>
        ///   An enumerator that iterates through the <see cref="KdlNode"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => List.Concat(Dictionary.Values).GetEnumerator();

        /// <summary>
        ///   Returns <see langword="false"/>.
        /// </summary>
        bool ICollection<KdlVertex?>.IsReadOnly => false;

        #endregion

        private void DetachParentForDictionaryItem(KdlVertex? item)
        {
            //TECHDEBT: Need to differentiate between cases. this may be true with properties
            //But may be avoided if only items?
            Debug.Assert(_dictionary != null, "Cannot have detachable nodes without a materialized dictionary.");
            if (item != null)
            {
                item.Parent = null;
            }
        }
    }
}
