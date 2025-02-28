namespace Automatonic.Text.Kdl.Serialization
{

    /// <summary>
    /// Determines how deserialization will handle object creation for fields or properties.
    /// </summary>
    public enum KdlObjectCreationHandling
    {
        /// <summary>
        /// A new instance will always be created when deserializing a field or property.
        /// </summary>
        Replace = 0,

        /// <summary>
        /// Attempt to populate any instances already found on a deserialized field or property.
        /// </summary>
        Populate = 1,
    }
}
