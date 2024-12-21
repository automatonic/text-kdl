namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Specifies the property order that is present in the KDL when serializing. Lower values are serialized first.
    /// If the attribute is not specified, the default value is 0.
    /// </summary>
    /// <remarks>If multiple properties have the same value, the ordering is undefined between them.</remarks>
    /// <remarks>
    /// Initializes a new instance of <see cref="KdlPropertyOrderAttribute"/> with the specified order.
    /// </remarks>
    /// <param name="order">The order of the property.</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlPropertyOrderAttribute(int order) : KdlAttribute
    {

        /// <summary>
        /// The serialization order of the property.
        /// </summary>
        public int Order { get; } = order;
    }
}
