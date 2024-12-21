using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Text.Kdl.Serialization
{
    internal readonly struct ReferenceEqualsWrapper(object obj) : IEquatable<ReferenceEqualsWrapper>
    {
        private readonly object _object = obj;

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is ReferenceEqualsWrapper otherObj && Equals(otherObj);
        public bool Equals(ReferenceEqualsWrapper obj) => ReferenceEquals(_object, obj._object);
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(_object);
    }
}
