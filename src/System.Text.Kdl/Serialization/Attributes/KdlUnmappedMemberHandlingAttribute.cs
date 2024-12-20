// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a type, determines the <see cref="KdlUnmappedMemberHandling"/> configuration
    /// for the specific type, overriding the global <see cref="KdlSerializerOptions.UnmappedMemberHandling"/> setting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
        AllowMultiple = false, Inherited = false)]
    public class KdlUnmappedMemberHandlingAttribute : KdlAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KdlUnmappedMemberHandlingAttribute"/>.
        /// </summary>
        /// <param name="unmappedMemberHandling">The handling to apply to the current member.</param>
        public KdlUnmappedMemberHandlingAttribute(KdlUnmappedMemberHandling unmappedMemberHandling)
        {
            UnmappedMemberHandling = unmappedMemberHandling;
        }

        /// <summary>
        /// Specifies the unmapped member handling setting for the attribute.
        /// </summary>
        public KdlUnmappedMemberHandling UnmappedMemberHandling { get; }
    }
}
