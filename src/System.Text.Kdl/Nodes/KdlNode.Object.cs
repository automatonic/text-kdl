using System.Diagnostics;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   Represents a mutable KDL object. Was "Node" but was aliases as "Vertex" for graph theory reasons along with "Node" being the top level structure of KDL.
    /// </summary>
    /// <remarks>
    /// It's safe to perform multiple concurrent read operations on a <see cref="KdlNode"/>,
    /// but issues can occur if the collection is modified while it's being read.
    /// </remarks>
    /// <remarks>
    ///   Initializes a new instance of the <see cref="KdlNode"/> class that is empty.
    /// </remarks>
    /// <param name="options">Options to control the behavior.</param>
    public sealed partial class KdlNode : KdlVertex
    {
        /// <summary>
        /// Gets or creates the underlying dictionary containing the properties of the object.
        /// </summary>
        private OrderedDictionary<string, KdlVertex?> Dictionary => _dictionary ?? InitializeDictionary();

        internal string GetPropertyName(KdlVertex? node)
        {
            KeyValuePair<string, KdlVertex?>? item = FindValue(node);
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
        public bool TryGetPropertyValue(string propertyName, out KdlVertex? jsonNode)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            return Dictionary.TryGetValue(propertyName, out jsonNode);
        }

        internal KdlVertex? GetItem(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            if (TryGetPropertyValue(propertyName, out KdlVertex? value))
            {
                return value;
            }

            // Return null for missing properties.
            return null;
        }

        internal void SetItem(string propertyName, KdlVertex? value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            OrderedDictionary<string, KdlVertex?> dict = Dictionary;

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
                KdlVertex? replacedValue = dict.GetAt(index).Value;

                if (ReferenceEquals(value, replacedValue))
                {
                    return;
                }

                DetachParentForDictionaryItem(replacedValue);
                dict.SetAt(index, value);
            }

            value?.AssignParent(this);
        }


        private KeyValuePair<string, KdlVertex?>? FindValue(KdlVertex? value)
        {
            foreach (KeyValuePair<string, KdlVertex?> item in Dictionary)
            {
                if (ReferenceEquals(item.Value, value))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
