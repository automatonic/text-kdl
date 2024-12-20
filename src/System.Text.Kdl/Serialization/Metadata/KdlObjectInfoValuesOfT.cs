using System.ComponentModel;
using System.Reflection;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides serialization metadata about an object type with constructors, properties, and fields.
    /// </summary>
    /// <typeparam name="T">The object type to serialize or deserialize.</typeparam>
    /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class KdlObjectInfoValues<T>
    {
        /// <summary>
        /// Provides a mechanism to create an instance of the class or struct when deserializing, using a parameterless constructor.
        /// </summary>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public Func<T>? ObjectCreator { get; init; }

        /// <summary>
        /// Provides a mechanism to create an instance of the class or struct when deserializing, using a parameterized constructor.
        /// </summary>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public Func<object[], T>? ObjectWithParameterizedConstructorCreator { get; init; }

        /// <summary>
        /// Provides a mechanism to initialize metadata for properties and fields of the class or struct.
        /// </summary>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public Func<KdlSerializerContext, KdlPropertyInfo[]>? PropertyMetadataInitializer { get; init; }

        /// <summary>
        /// Provides a mechanism to initialize metadata for a parameterized constructor of the class or struct to be used when deserializing.
        /// </summary>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public Func<KdlParameterInfoValues[]>? ConstructorParameterMetadataInitializer { get; init; }

        /// <summary>
        /// Provides a delayed attribute provider corresponding to the deserialization constructor.
        /// </summary>
        public Func<ICustomAttributeProvider>? ConstructorAttributeProviderFactory { get; init; }

        /// <summary>
        /// Specifies how number properties and fields should be processed when serializing and deserializing.
        /// </summary>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public KdlNumberHandling NumberHandling { get; init; }

        /// <summary>
        /// Provides a serialization implementation for instances of the class or struct which assumes options specified by <see cref="KdlSourceGenerationOptionsAttribute"/>.
        /// </summary>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public Action<KdlWriter, T>? SerializeHandler { get; init; }
    }
}
