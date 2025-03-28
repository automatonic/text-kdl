namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// This class defines how the <see cref="KdlSerializer"/> deals with references on serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ReferenceResolver"/> to create on each serialization or deserialization call.</typeparam>
    public sealed class ReferenceHandler<T> : ReferenceHandler
        where T : ReferenceResolver, new()
    {
        /// <summary>
        /// Creates a new <see cref="ReferenceResolver"/> of type <typeparamref name="T"/> used for each serialization call.
        /// </summary>
        /// <returns>The new resolver to use for serialization and deserialization.</returns>
        public override ReferenceResolver CreateResolver() => new T();
    }
}
