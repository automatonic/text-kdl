// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    ///   Options to control <see cref="KdlNode"/> behavior.
    /// </summary>
    public struct KdlNodeOptions
    {
        /// <summary>
        ///   Specifies whether property names on <see cref="KdlObject"/> are case insensitive.
        /// </summary>
        public bool PropertyNameCaseInsensitive { get; set; }
    }
}
