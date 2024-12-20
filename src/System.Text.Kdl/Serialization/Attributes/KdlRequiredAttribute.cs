using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Indicates that the annotated member must bind to a KDL property on deserialization.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> token in KDL will not trigger a validation error.
    /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
    /// this attribute will be mapped to <see cref="KdlPropertyInfo.IsRequired"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlRequiredAttribute : KdlAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KdlRequiredAttribute"/>.
        /// </summary>
        public KdlRequiredAttribute() { }
    }
}
