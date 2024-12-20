namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a constructor, indicates that the constructor should be used to create
    /// instances of the type on deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class KdlConstructorAttribute : KdlAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KdlConstructorAttribute"/>.
        /// </summary>
        public KdlConstructorAttribute() { }
    }
}
