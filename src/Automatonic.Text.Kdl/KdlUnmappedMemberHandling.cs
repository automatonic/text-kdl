namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Determines how <see cref="KdlSerializer"/> handles KDL properties that
    /// cannot be mapped to a specific .NET member when deserializing object types.
    /// </summary>
    public enum KdlUnmappedMemberHandling
    {
        /// <summary>
        /// Silently skips any unmapped properties. This is the default behavior.
        /// </summary>
        Skip = 0,

        /// <summary>
        /// Throws an exception when an unmapped property is encountered.
        /// </summary>
        Disallow = 1,
    }
}
