namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Specifies the property name that is present in the KDL when serializing and deserializing.
    /// This overrides any naming policy specified by <see cref="KdlNamingPolicy"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of <see cref="KdlPropertyNameAttribute"/> with the specified property name.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlPropertyNameAttribute(string name) : KdlAttribute
    {

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; } = name;
    }
}
