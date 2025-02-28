using System.ComponentModel;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides serialization metadata about a collection type.
    /// </summary>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class KdlCollectionInfoValues<TCollection>
    {
        /// <summary>
        /// A <see cref="Func{TResult}"/> to create an instance of the collection when deserializing.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public Func<TCollection>? ObjectCreator { get; init; }

        /// <summary>
        /// If a dictionary type, the <see cref="KdlTypeInfo"/> instance representing the key type.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public KdlTypeInfo? KeyInfo { get; init; }

        /// <summary>
        /// A <see cref="KdlTypeInfo"/> instance representing the element type.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public KdlTypeInfo ElementInfo { get; init; } = null!;

        /// <summary>
        /// The <see cref="KdlNumberHandling"/> option to apply to number collection elements.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public KdlNumberHandling NumberHandling { get; init; }

        /// <summary>
        /// An optimized serialization implementation assuming pre-determined <see cref="KdlSourceGenerationOptionsAttribute"/> defaults.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public Action<KdlWriter, TCollection>? SerializeHandler { get; init; }
    }
}
