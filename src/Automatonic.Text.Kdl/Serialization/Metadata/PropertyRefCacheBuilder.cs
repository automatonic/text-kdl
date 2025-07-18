﻿using System.Diagnostics;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Defines builder type for constructing updated <see cref="PropertyRef"/> caches.
    /// </summary>
    internal sealed class PropertyRefCacheBuilder(PropertyRef[] originalCache)
    {
        public const int MaxCapacity = 64;
        private readonly List<PropertyRef> _propertyRefs = [];
        private readonly HashSet<PropertyRef> _added = [];

        /// <summary>
        /// Stores a reference to the original cache off which the current list is being built.
        /// </summary>
        public readonly PropertyRef[] OriginalCache = originalCache;
        public int Count => _propertyRefs.Count;
        public int TotalCount => OriginalCache.Length + _propertyRefs.Count;

        public PropertyRef[] ToArray() => [.. OriginalCache, .. _propertyRefs];

        public void TryAdd(PropertyRef propertyRef)
        {
            Debug.Assert(TotalCount < MaxCapacity, "Should have been checked by the caller.");

            if (_added.Add(propertyRef))
            {
                _propertyRefs.Add(propertyRef);
            }
        }
    }
}
