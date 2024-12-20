namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a type, indicates that the type should be serialized polymorphically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class KdlPolymorphicAttribute : KdlAttribute
    {
        /// <summary>
        /// Gets or sets a custom type discriminator property name for the polymorphic type.
        /// Uses the default '$type' property name if left unset.
        /// </summary>
        public string? TypeDiscriminatorPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the behavior when serializing an undeclared derived runtime type.
        /// </summary>
        public KdlUnknownDerivedTypeHandling UnknownDerivedTypeHandling { get; set; }

        /// <summary>
        /// When set to <see langword="true"/>, instructs the deserializer to ignore any
        /// unrecognized type discriminator id's and reverts to the contract of the base type.
        /// Otherwise, it will fail the deserialization.
        /// </summary>
        public bool IgnoreUnrecognizedTypeDiscriminators { get; set; }
    }
}
