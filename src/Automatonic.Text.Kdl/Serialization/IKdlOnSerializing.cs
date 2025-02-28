namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Specifies that the type should have its <see cref="OnSerializing"/> method called before serialization occurs.
    /// </summary>
    /// <remarks>
    /// This behavior is only supported on types representing KDL objects.
    /// Types that have a custom converter or represent either collections or primitive values do not support this behavior.
    /// </remarks>
    public interface IKdlOnSerializing
    {
        /// <summary>
        /// The method that is called before serialization.
        /// </summary>
        void OnSerializing();
    }
}
