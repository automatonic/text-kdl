namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Specifies that the type should have its <see cref="OnDeserializing"/> method called before deserialization occurs.
    /// </summary>
    /// <remarks>
    /// This behavior is only supported on types representing KDL objects.
    /// Types that have a custom converter or represent either collections or primitive values do not support this behavior.
    /// </remarks>
    public interface IKdlOnDeserializing
    {
        /// <summary>
        /// The method that is called before deserialization.
        /// </summary>
        void OnDeserializing();
    }
}
