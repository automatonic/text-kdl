namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// The <see cref="Kdl.KdlNamingPolicy"/> to be used at run time.
    /// </summary>
    public enum KdlKnownNamingPolicy
    {
        /// <summary>
        /// Specifies that KDL property names should not be converted.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.CamelCase"/> be used to convert KDL property names.
        /// </summary>
        CamelCase = 1,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.SnakeCaseLower"/> be used to convert KDL property names.
        /// </summary>
        SnakeCaseLower = 2,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.SnakeCaseUpper"/> be used to convert KDL property names.
        /// </summary>
        SnakeCaseUpper = 3,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.KebabCaseLower"/> be used to convert KDL property names.
        /// </summary>
        KebabCaseLower = 4,

        /// <summary>
        /// Specifies that the built-in <see cref="Kdl.KdlNamingPolicy.KebabCaseUpper"/> be used to convert KDL property names.
        /// </summary>
        KebabCaseUpper = 5,
    }
}
