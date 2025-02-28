namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Prevents a property or field from being serialized or deserialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlIgnoreAttribute : KdlAttribute
    {
        /// <summary>
        /// Specifies the condition that must be met before a property or field will be ignored.
        /// </summary>
        /// <remarks>The default value is <see cref="KdlIgnoreCondition.Always"/>.</remarks>
        public KdlIgnoreCondition Condition { get; set; } = KdlIgnoreCondition.Always;

        /// <summary>
        /// Initializes a new instance of <see cref="KdlIgnoreAttribute"/>.
        /// </summary>
        public KdlIgnoreAttribute() { }
    }
}
