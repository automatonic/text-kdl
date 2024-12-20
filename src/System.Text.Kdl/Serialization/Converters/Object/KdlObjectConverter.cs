// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Base class for non-enumerable, non-primitive objects where public properties
    /// are (de)serialized as a KDL object.
    /// </summary>
    internal abstract class KdlObjectConverter<T> : KdlResumableConverter<T>
    {
        private protected sealed override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.Object;
        internal override bool CanPopulate => true;
    }
}
