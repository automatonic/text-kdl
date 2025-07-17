namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// The generation mode for the Automatonic.Text.Kdl source generator.
    /// </summary>
    [Flags]
    public enum KdlSourceGenerationMode
    {
        /// <summary>
        /// When specified on <see cref="KdlSourceGenerationOptionsAttribute.GenerationMode"/>, indicates that both type-metadata initialization logic
        /// and optimized serialization logic should be generated for all types. When specified on <see cref="KdlSerializableAttribute.GenerationMode"/>,
        /// indicates that the setting on <see cref="KdlSourceGenerationOptionsAttribute.GenerationMode"/> should be used.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Instructs the KDL source generator to generate type-metadata initialization logic.
        /// </summary>
        /// <remarks>
        /// This mode supports all <see cref="KdlSerializer"/> features.
        /// </remarks>
        Metadata = 1,

        /// <summary>
        /// Instructs the KDL source generator to generate optimized serialization logic.
        /// </summary>
        /// <remarks>
        /// This mode supports only a subset of <see cref="KdlSerializer"/> features.
        /// </remarks>
        Serialization = 2,
    }
}
