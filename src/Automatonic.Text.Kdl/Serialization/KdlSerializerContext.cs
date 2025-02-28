using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Provides metadata about a set of types that is relevant to KDL serialization.
    /// </summary>
    public abstract partial class KdlSerializerContext : IKdlTypeInfoResolver, IBuiltInKdlTypeInfoResolver
    {
        private KdlSerializerOptions? _options;

        /// <summary>
        /// Gets the run time specified options of the context. If no options were passed
        /// when instantiating the context, then a new instance is bound and returned.
        /// </summary>
        /// <remarks>
        /// The options instance cannot be mutated once it is bound to the context instance.
        /// </remarks>
        public KdlSerializerOptions Options
        {
            get
            {
                KdlSerializerOptions? options = _options;

                if (options is null)
                {
                    options = new KdlSerializerOptions { TypeInfoResolver = this };
                    options.MakeReadOnly();
                    _options = options;
                }

                return options;
            }
        }

        internal void AssociateWithOptions(KdlSerializerOptions options)
        {
            Debug.Assert(!options.IsReadOnly);
            options.TypeInfoResolver = this;
            options.MakeReadOnly();
            _options = options;
        }

        /// <summary>
        /// Indicates whether pre-generated serialization logic for types in the context
        /// is compatible with the run time specified <see cref="KdlSerializerOptions"/>.
        /// </summary>
        bool IBuiltInKdlTypeInfoResolver.IsCompatibleWithOptions(KdlSerializerOptions options)
        {
            Debug.Assert(options != null);

            KdlSerializerOptions? generatedSerializerOptions = GeneratedSerializerOptions;

            return
                generatedSerializerOptions is not null &&
                // Guard against unsupported features
                options.Converters.Count == 0 &&
                options.Encoder is null &&
                // Disallow custom number handling we'd need to honor when writing.
                // AllowReadingFromString and Strict are fine since there's no action to take when writing.
                !KdlHelpers.RequiresSpecialNumberHandlingOnWrite(options.NumberHandling) &&
                options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Unspecified &&

                // Ensure options values are consistent with expected defaults.
                options.DefaultIgnoreCondition == generatedSerializerOptions.DefaultIgnoreCondition &&
                options.RespectNullableAnnotations == generatedSerializerOptions.RespectNullableAnnotations &&
                options.IgnoreReadOnlyFields == generatedSerializerOptions.IgnoreReadOnlyFields &&
                options.IgnoreReadOnlyProperties == generatedSerializerOptions.IgnoreReadOnlyProperties &&
                options.IncludeFields == generatedSerializerOptions.IncludeFields &&
                options.PropertyNamingPolicy == generatedSerializerOptions.PropertyNamingPolicy &&
                options.DictionaryKeyPolicy is null;
        }

        /// <summary>
        /// The default run time options for the context. Its values are defined at design-time via <see cref="KdlSourceGenerationOptionsAttribute"/>.
        /// </summary>
        protected abstract KdlSerializerOptions? GeneratedSerializerOptions { get; }

        /// <summary>
        /// Creates an instance of <see cref="KdlSerializerContext"/> and binds it with the indicated <see cref="KdlSerializerOptions"/>.
        /// </summary>
        /// <param name="options">The run time provided options for the context instance.</param>
        /// <remarks>
        /// If no instance options are passed, then no options are set until the context is bound using <see cref="KdlSerializerOptions.AddContext{TContext}"/>,
        /// or until <see cref="Options"/> is called, where a new options instance is created and bound.
        /// </remarks>
        protected KdlSerializerContext(KdlSerializerOptions? options)
        {
            if (options != null)
            {
                options.VerifyMutable();
                AssociateWithOptions(options);
            }
        }

        /// <summary>
        /// Returns a <see cref="KdlTypeInfo"/> instance representing the given type.
        /// </summary>
        /// <param name="type">The type to fetch metadata about.</param>
        /// <returns>The metadata for the specified type, or <see langword="null" /> if the context has no metadata for the type.</returns>
        public abstract KdlTypeInfo? GetTypeInfo(Type type);

        KdlTypeInfo? IKdlTypeInfoResolver.GetTypeInfo(Type type, KdlSerializerOptions options)
        {
            if (options != null && options != _options)
            {
                ThrowHelper.ThrowInvalidOperationException_ResolverTypeInfoOptionsNotCompatible();
            }

            return GetTypeInfo(type);
        }
    }
}
