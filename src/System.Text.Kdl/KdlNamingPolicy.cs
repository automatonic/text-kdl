namespace System.Text.Kdl
{
    /// <summary>
    /// Determines the naming policy used to convert a string-based name to another format, such as a camel-casing format.
    /// </summary>
#if BUILDING_SOURCE_GENERATOR
    internal
#else
    public
#endif
    abstract class KdlNamingPolicy
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KdlNamingPolicy"/>.
        /// </summary>
        protected KdlNamingPolicy() { }

        /// <summary>
        /// Returns the naming policy for camel-casing.
        /// </summary>
        public static KdlNamingPolicy CamelCase { get; } = new KdlCamelCaseNamingPolicy();

        /// <summary>
        /// Returns the naming policy for lower snake-casing.
        /// </summary>
        public static KdlNamingPolicy SnakeCaseLower { get; } = new KdlSnakeCaseLowerNamingPolicy();

        /// <summary>
        /// Returns the naming policy for upper snake-casing.
        /// </summary>
        public static KdlNamingPolicy SnakeCaseUpper { get; } = new KdlSnakeCaseUpperNamingPolicy();

        /// <summary>
        /// Returns the naming policy for lower kebab-casing.
        /// </summary>
        public static KdlNamingPolicy KebabCaseLower { get; } = new KdlKebabCaseLowerNamingPolicy();

        /// <summary>
        /// Returns the naming policy for upper kebab-casing.
        /// </summary>
        public static KdlNamingPolicy KebabCaseUpper { get; } = new KdlKebabCaseUpperNamingPolicy();

        /// <summary>
        /// When overridden in a derived class, converts the specified name according to the policy.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>The converted name.</returns>
        public abstract string ConvertName(string name);
    }
}
