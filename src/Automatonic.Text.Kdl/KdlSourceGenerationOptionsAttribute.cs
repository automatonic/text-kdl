namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Specifies compile-time source generator configuration when applied to <see cref="KdlSerializerContext"/> class declarations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class KdlSourceGenerationOptionsAttribute : KdlAttribute
    {
        /// <summary>
        /// Constructs a new <see cref="KdlSourceGenerationOptionsAttribute"/> instance.
        /// </summary>
        public KdlSourceGenerationOptionsAttribute() { }

        /// <summary>
        /// Constructs a new <see cref="KdlSourceGenerationOptionsAttribute"/> instance with a predefined set of options determined by the specified <see cref="KdlSerializerDefaults"/>.
        /// </summary>
        /// <param name="defaults">The <see cref="KdlSerializerDefaults"/> to reason about.</param>
        /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="defaults"/> parameter.</exception>
        public KdlSourceGenerationOptionsAttribute(KdlSerializerDefaults defaults)
        {
            // Constructor kept in sync with equivalent overload in KdlSerializerOptions

            if (defaults is KdlSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true;
                PropertyNamingPolicy = KdlKnownNamingPolicy.CamelCase;
                NumberHandling = KdlNumberHandling.AllowReadingFromString;
            }
            else if (defaults is not KdlSerializerDefaults.General)
            {
                throw new ArgumentOutOfRangeException(nameof(defaults));
            }
        }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.AllowOutOfOrderMetadataProperties"/> when set.
        /// </summary>
        public bool AllowOutOfOrderMetadataProperties { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.AllowTrailingCommas"/> when set.
        /// </summary>
        public bool AllowTrailingCommas { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.Converters"/> when set.
        /// </summary>
        public Type[]? Converters { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.DefaultBufferSize"/> when set.
        /// </summary>
        public int DefaultBufferSize { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.DefaultIgnoreCondition"/> when set.
        /// </summary>
        public KdlIgnoreCondition DefaultIgnoreCondition { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.DictionaryKeyPolicy"/> when set.
        /// </summary>
        public KdlKnownNamingPolicy DictionaryKeyPolicy { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.IgnoreReadOnlyFields"/> when set.
        /// </summary>
        public bool IgnoreReadOnlyFields { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.IgnoreReadOnlyProperties"/> when set.
        /// </summary>
        public bool IgnoreReadOnlyProperties { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.IncludeFields"/> when set.
        /// </summary>
        public bool IncludeFields { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.MaxDepth"/> when set.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.NumberHandling"/> when set.
        /// </summary>
        public KdlNumberHandling NumberHandling { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.PreferredObjectCreationHandling"/> when set.
        /// </summary>
        public KdlObjectCreationHandling PreferredObjectCreationHandling { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.PropertyNameCaseInsensitive"/> when set.
        /// </summary>
        public bool PropertyNameCaseInsensitive { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.PropertyNamingPolicy"/> when set.
        /// </summary>
        public KdlKnownNamingPolicy PropertyNamingPolicy { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.ReadCommentHandling"/> when set.
        /// </summary>
        public KdlCommentHandling ReadCommentHandling { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.ReferenceHandler"/> when set.
        /// </summary>
        public KdlKnownReferenceHandler ReferenceHandler { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.RespectNullableAnnotations"/> when set.
        /// </summary>
        public bool RespectNullableAnnotations { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.RespectRequiredConstructorParameters"/> when set.
        /// </summary>
        public bool RespectRequiredConstructorParameters { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.UnknownTypeHandling"/> when set.
        /// </summary>
        public KdlUnknownTypeHandling UnknownTypeHandling { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.UnmappedMemberHandling"/> when set.
        /// </summary>
        public KdlUnmappedMemberHandling UnmappedMemberHandling { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.WriteIndented"/> when set.
        /// </summary>
        public bool WriteIndented { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.IndentCharacter"/> when set.
        /// </summary>
        public char IndentCharacter { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.IndentCharacter"/> when set.
        /// </summary>
        public int IndentSize { get; set; }

        /// <summary>
        /// Specifies the default source generation mode for type declarations that don't set a <see cref="KdlSerializableAttribute.GenerationMode"/>.
        /// </summary>
        public KdlSourceGenerationMode GenerationMode { get; set; }

        /// <summary>
        /// Instructs the source generator to default to <see cref="KdlStringEnumConverter"/>
        /// instead of numeric serialization for all enum types encountered in its type graph.
        /// </summary>
        public bool UseStringEnumConverter { get; set; }

        /// <summary>
        /// Specifies the default value of <see cref="KdlSerializerOptions.NewLine"/> when set.
        /// </summary>
        public string? NewLine { get; set; }
    }
}
