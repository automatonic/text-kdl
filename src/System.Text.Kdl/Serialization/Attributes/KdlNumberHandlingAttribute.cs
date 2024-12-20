// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a type, property, or field, indicates what <see cref="KdlNumberHandling"/>
    /// settings should be used when serializing or deserializing numbers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlNumberHandlingAttribute : KdlAttribute
    {
        /// <summary>
        /// Indicates what settings should be used when serializing or deserializing numbers.
        /// </summary>
        public KdlNumberHandling Handling { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="KdlNumberHandlingAttribute"/>.
        /// </summary>
        public KdlNumberHandlingAttribute(KdlNumberHandling handling)
        {
            if (!KdlSerializer.IsValidNumberHandlingValue(handling))
            {
                throw new ArgumentOutOfRangeException(nameof(handling));
            }
            Handling = handling;
        }
    }
}
