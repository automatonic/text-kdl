﻿using System.Diagnostics;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// The default ReferenceResolver implementation to handle duplicate object references.
    /// </summary>
    internal sealed class PreserveReferenceResolver : ReferenceResolver
    {
        private uint _referenceCount;
        private readonly Dictionary<string, object>? _referenceIdToObjectMap;
        private readonly Dictionary<object, string>? _objectToReferenceIdMap;

        public PreserveReferenceResolver(bool writing)
        {
            if (writing)
            {
                // Comparer used here does a reference equality comparison on serialization, which is where we use the objects as the dictionary keys.
                _objectToReferenceIdMap = new Dictionary<object, string>(
                    ReferenceEqualityComparer.Instance
                );
            }
            else
            {
                _referenceIdToObjectMap = [];
            }
        }

        public override void AddReference(string referenceId, object value)
        {
            Debug.Assert(_referenceIdToObjectMap != null);

            if (!_referenceIdToObjectMap.TryAdd(referenceId, value))
            {
                ThrowHelper.ThrowKdlException_MetadataDuplicateIdFound(referenceId);
            }
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            Debug.Assert(_objectToReferenceIdMap != null);

            if (_objectToReferenceIdMap.TryGetValue(value, out string? referenceId))
            {
                alreadyExists = true;
            }
            else
            {
                _referenceCount++;
                referenceId = _referenceCount.ToString();
                _objectToReferenceIdMap.Add(value, referenceId);
                alreadyExists = false;
            }

            return referenceId;
        }

        public override object ResolveReference(string referenceId)
        {
            Debug.Assert(_referenceIdToObjectMap != null);

            if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object? value))
            {
                ThrowHelper.ThrowKdlException_MetadataReferenceNotFound(referenceId);
            }

            return value;
        }
    }
}
