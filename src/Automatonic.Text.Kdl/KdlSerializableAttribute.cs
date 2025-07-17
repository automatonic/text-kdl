using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Instructs the Automatonic.Text.Kdl source generator to generate source code to help optimize performance
    /// when serializing and deserializing instances of the specified type and types in its object graph.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of <see cref="KdlSerializableAttribute"/> with the specified type.
    /// </remarks>
    /// <param name="type">The type to generate source code for.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public
#pragma warning disable CS9113 // Parameter is unread.
    sealed class KdlSerializableAttribute(Type type) : KdlAttribute
#pragma warning restore CS9113 // Parameter is unread.
    {
        /// <summary>
        /// The name of the property for the generated <see cref="KdlTypeInfo{T}"/> for
        /// the type on the generated, derived <see cref="KdlSerializerContext"/> type.
        /// </summary>
        /// <remarks>
        /// Useful to resolve a name collision with another type in the compilation closure.
        /// </remarks>
        public string? TypeInfoPropertyName { get; set; }

        /// <summary>
        /// Determines what the source generator should generate for the type. If the value is <see cref="KdlSourceGenerationMode.Default"/>,
        /// then the setting specified on <see cref="KdlSourceGenerationOptionsAttribute.GenerationMode"/> will be used.
        /// </summary>
        public KdlSourceGenerationMode GenerationMode { get; set; }
    }
}
