namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Signifies what default options are used by <see cref="KdlSerializerOptions"/>.
    /// </summary>
    public enum KdlSerializerDefaults
    {
        /// <summary>
        /// Specifies that general-purpose values should be used. These are the same settings applied if a <see cref="KdlSerializerDefaults"/> isn't specified.
        /// </summary>
        /// <remarks>
        /// This option implies that property names are treated as case-sensitive and that "PascalCase" name formatting should be employed.
        /// </remarks>
        General = 0,

        /// <summary>
        /// Specifies that values should be used more appropriate to web-based scenarios.
        /// </summary>
        /// <remarks>
        /// This option implies that property names are treated as case-insensitive and that "camelCase" name formatting should be employed.
        /// </remarks>
        Web = 1,
    }
}
