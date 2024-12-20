namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Specifies that the type should have its <see cref="OnSerialized"/> method called after serialization occurs.
    /// </summary>
    /// <remarks>
    /// This behavior is only supported on types representing KDL objects.
    /// Types that have a custom converter or represent either collections or primitive values do not support this behavior.
    /// </remarks>
    public interface IKdlOnSerialized
    {
        /// <summary>
        /// The method that is called after serialization.
        /// </summary>
        void OnSerialized();
    }
}
