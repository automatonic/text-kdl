namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Determines the string value that should be used when serializing an enum member.
    /// </summary>
    /// <remarks>
    /// Creates new attribute instance with a specified enum member name.
    /// </remarks>
    /// <param name="name">The name to apply to the current enum member.</param>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class KdlStringEnumMemberNameAttribute(string name) : Attribute
    {

        /// <summary>
        /// Gets the name of the enum member.
        /// </summary>
        public string Name { get; } = name;
    }
}
