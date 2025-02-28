namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Indicates that the property or field should be included for serialization and deserialization.
    /// </summary>
    /// <remarks>
    /// When applied to a public property, indicates that non-public getters and setters should be used for serialization and deserialization.
    ///
    /// Non-public properties and fields are not allowed when serializing and deserializing. If the attribute is used on a non-public property or field,
    /// an <see cref="InvalidOperationException"/> is thrown during the first serialization or deserialization of the declaring type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlIncludeAttribute : KdlAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KdlIncludeAttribute"/>.
        /// </summary>
        public KdlIncludeAttribute() { }
    }
}
