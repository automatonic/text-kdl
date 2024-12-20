using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.Kdl;
using System.Text.Kdl.Serialization.Metadata;

[assembly: MetadataUpdateHandler(typeof(KdlSerializerOptionsUpdateHandler))]

#pragma warning disable IDE0060

namespace System.Text.Kdl
{
    /// <summary>Handler used to clear KdlSerializerOptions reflection cache upon a metadata update.</summary>
    internal static class KdlSerializerOptionsUpdateHandler
    {
        public static void ClearCache(Type[]? types)
        {
            // Ignore the types, and just clear out all reflection caches from serializer options.
            foreach (KeyValuePair<KdlSerializerOptions, object?> options in KdlSerializerOptions.TrackedOptionsInstances.All)
            {
                options.Key.ClearCaches();
            }

            DefaultKdlTypeInfoResolver.ClearMemberAccessorCaches();
        }
    }
}
