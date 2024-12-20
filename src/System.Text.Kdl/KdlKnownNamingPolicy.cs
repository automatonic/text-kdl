// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// The <see cref="Kdl.KdlNamingPolicy"/> to be used at run time.
    /// </summary>
    public enum KdlKnownNamingPolicy
    {
        /// <summary>
        /// Specifies that JSON property names should not be converted.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.CamelCase"/> be used to convert JSON property names.
        /// </summary>
        CamelCase = 1,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.SnakeCaseLower"/> be used to convert JSON property names.
        /// </summary>
        SnakeCaseLower = 2,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.SnakeCaseUpper"/> be used to convert JSON property names.
        /// </summary>
        SnakeCaseUpper = 3,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.KebabCaseLower"/> be used to convert JSON property names.
        /// </summary>
        KebabCaseLower = 4,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.KebabCaseUpper"/> be used to convert JSON property names.
        /// </summary>
        KebabCaseUpper = 5
    }
}
