namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a type, determines the <see cref="KdlUnmappedMemberHandling"/> configuration
    /// for the specific type, overriding the global <see cref="KdlSerializerOptions.UnmappedMemberHandling"/> setting.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of <see cref="KdlUnmappedMemberHandlingAttribute"/>.
    /// </remarks>
    /// <param name="unmappedMemberHandling">The handling to apply to the current member.</param>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = false
    )]
    public class KdlUnmappedMemberHandlingAttribute(
        KdlUnmappedMemberHandling unmappedMemberHandling
    ) : KdlAttribute
    {
        /// <summary>
        /// Specifies the unmapped member handling setting for the attribute.
        /// </summary>
        public KdlUnmappedMemberHandling UnmappedMemberHandling { get; } = unmappedMemberHandling;
    }
}
