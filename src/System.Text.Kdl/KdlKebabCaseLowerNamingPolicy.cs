// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl
{
    internal sealed class KdlKebabCaseLowerNamingPolicy : KdlSeparatorNamingPolicy
    {
        public KdlKebabCaseLowerNamingPolicy()
            : base(lowercase: true, separator: '-')
        {
        }
    }
}
