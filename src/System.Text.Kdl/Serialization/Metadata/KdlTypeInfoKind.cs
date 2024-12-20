// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Determines the kind of contract metadata a given <see cref="KdlTypeInfo"/> is specifying.
    /// </summary>
    public enum KdlTypeInfoKind
    {
        /// <summary>
        /// Type is either a simple value or uses a custom converter.
        /// </summary>
        None = 0,
        /// <summary>
        /// Type is serialized as an object with properties.
        /// </summary>
        Object = 1,
        /// <summary>
        /// Type is serialized as a collection with elements.
        /// </summary>
        Enumerable = 2,
        /// <summary>
        /// Type is serialized as a dictionary with key/value pair entries.
        /// </summary>
        Dictionary = 3
    }
}
