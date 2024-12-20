// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl
{
    /// <summary>
    ///   Specifies the data type of a KDL value.
    /// </summary>
    public enum KdlValueKind : byte
    {
        /// <summary>
        ///   Indicates that there is no value (as distinct from <see cref="Null"/>).
        /// </summary>
        Undefined,

        /// <summary>
        ///   Indicates that a value is a KDL object.
        /// </summary>
        Object,

        /// <summary>
        ///   Indicates that a value is a KDL array.
        /// </summary>
        Array,

        /// <summary>
        ///   Indicates that a value is a KDL string.
        /// </summary>
        String,

        /// <summary>
        ///   Indicates that a value is a KDL number.
        /// </summary>
        Number,

        /// <summary>
        ///   Indicates that a value is the KDL value <c>true</c>.
        /// </summary>
        True,

        /// <summary>
        ///   Indicates that a value is the KDL value <c>false</c>.
        /// </summary>
        False,

        /// <summary>
        ///   Indicates that a value is the KDL value <c>null</c>.
        /// </summary>
        Null,
    }
}
