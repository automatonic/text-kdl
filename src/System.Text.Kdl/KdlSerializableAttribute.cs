#if !BUILDING_SOURCE_GENERATOR
using System.Text.Kdl.Serialization.Metadata;
#endif

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Instructs the System.Text.Kdl source generator to generate source code to help optimize performance
    /// when serializing and deserializing instances of the specified type and types in its object graph.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]

#if BUILDING_SOURCE_GENERATOR
    internal
#else
    public
#endif
    sealed class KdlSerializableAttribute : KdlAttribute
    {
#pragma warning disable IDE0060
        /// <summary>
        /// Initializes a new instance of <see cref="KdlSerializableAttribute"/> with the specified type.
        /// </summary>
        /// <param name="type">The type to generate source code for.</param>
        public KdlSerializableAttribute(Type type) { }
#pragma warning restore IDE0060

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
