using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Converters;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    /// <summary>
    /// Provides options to be used with <see cref="KdlSerializer"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed partial class KdlSerializerOptions
    {
        internal const int BufferSizeDefault = 16 * 1024;

        // For backward compatibility the default max depth for KdlSerializer is 64,
        // the minimum of KdlReaderOptions.DefaultMaxDepth and KdlWriterOptions.DefaultMaxDepth.
        internal const int DefaultMaxDepth = KdlReaderOptions.DefaultMaxDepth;

        /// <summary>
        /// Gets a read-only, singleton instance of <see cref="KdlSerializerOptions" /> that uses the default configuration.
        /// </summary>
        /// <remarks>
        /// Each <see cref="KdlSerializerOptions" /> instance encapsulates its own serialization metadata caches,
        /// so using fresh default instances every time one is needed can result in redundant recomputation of converters.
        /// This property provides a shared instance that can be consumed by any number of components without necessitating any converter recomputation.
        /// </remarks>
        public static KdlSerializerOptions Default
        {
            [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
            [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
            get => s_defaultOptions ?? GetOrCreateSingleton(ref s_defaultOptions, KdlSerializerDefaults.General);
        }

        private static KdlSerializerOptions? s_defaultOptions;

        /// <summary>
        /// Gets a read-only, singleton instance of <see cref="KdlSerializerOptions" /> that uses the web configuration.
        /// </summary>
        /// <remarks>
        /// Each <see cref="KdlSerializerOptions" /> instance encapsulates its own serialization metadata caches,
        /// so using fresh default instances every time one is needed can result in redundant recomputation of converters.
        /// This property provides a shared instance that can be consumed by any number of components without necessitating any converter recomputation.
        /// </remarks>
        public static KdlSerializerOptions Web
        {
            [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
            [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
            get => s_webOptions ?? GetOrCreateSingleton(ref s_webOptions, KdlSerializerDefaults.Web);
        }

        private static KdlSerializerOptions? s_webOptions;

        // For any new option added, consider adding it to the options copied in the copy constructor below
        // and consider updating the EqualtyComparer used for comparing CachingContexts.
        private IKdlTypeInfoResolver? _typeInfoResolver;
        private KdlNamingPolicy? _dictionaryKeyPolicy;
        private KdlNamingPolicy? _jsonPropertyNamingPolicy;
        private KdlCommentHandling _readCommentHandling;
        private ReferenceHandler? _referenceHandler;
        private JavaScriptEncoder? _encoder;
        private ConverterList? _converters;
        private KdlIgnoreCondition _defaultIgnoreCondition;
        private KdlNumberHandling _numberHandling;
        private KdlObjectCreationHandling _preferredObjectCreationHandling;
        private KdlUnknownTypeHandling _unknownTypeHandling;
        private KdlUnmappedMemberHandling _unmappedMemberHandling;

        private int _defaultBufferSize = BufferSizeDefault;
        private int _maxDepth;
        private bool _allowOutOfOrderMetadataProperties;
        private bool _allowTrailingCommas;
        private bool _respectNullableAnnotations = AppContextSwitchHelper.RespectNullableAnnotationsDefault;
        private bool _respectRequiredConstructorParameters = AppContextSwitchHelper.RespectRequiredConstructorParametersDefault;
        private readonly bool _ignoreNullValues;
        private bool _ignoreReadOnlyProperties;
        private bool _ignoreReadonlyFields;
        private bool _includeFields;
        private string? _newLine;
        private bool _propertyNameCaseInsensitive;
        private bool _writeIndented;
        private char _indentCharacter = KdlConstants.DefaultIndentCharacter;
        private int _indentSize = KdlConstants.DefaultIndentSize;

        /// <summary>
        /// Constructs a new <see cref="KdlSerializerOptions"/> instance.
        /// </summary>
        public KdlSerializerOptions() => TrackOptionsInstance(this);

        /// <summary>
        /// Copies the options from a <see cref="KdlSerializerOptions"/> instance to a new instance.
        /// </summary>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> instance to copy options from.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        public KdlSerializerOptions(KdlSerializerOptions options)
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            // The following fields are not copied intentionally:
            // 1. _cachingContext can only be set in immutable options instances.
            // 2. _typeInfoResolverChain can be created lazily as it relies on
            //    _typeInfoResolver as its source of truth.

            _dictionaryKeyPolicy = options._dictionaryKeyPolicy;
            _jsonPropertyNamingPolicy = options._jsonPropertyNamingPolicy;
            _readCommentHandling = options._readCommentHandling;
            _referenceHandler = options._referenceHandler;
            _converters = options._converters is { } converters ? new(this, converters) : null;
            _encoder = options._encoder;
            _defaultIgnoreCondition = options._defaultIgnoreCondition;
            _numberHandling = options._numberHandling;
            _preferredObjectCreationHandling = options._preferredObjectCreationHandling;
            _unknownTypeHandling = options._unknownTypeHandling;
            _unmappedMemberHandling = options._unmappedMemberHandling;

            _defaultBufferSize = options._defaultBufferSize;
            _maxDepth = options._maxDepth;
            _allowOutOfOrderMetadataProperties = options._allowOutOfOrderMetadataProperties;
            _allowTrailingCommas = options._allowTrailingCommas;
            _respectNullableAnnotations = options._respectNullableAnnotations;
            _respectRequiredConstructorParameters = options._respectRequiredConstructorParameters;
            _ignoreNullValues = options._ignoreNullValues;
            _ignoreReadOnlyProperties = options._ignoreReadOnlyProperties;
            _ignoreReadonlyFields = options._ignoreReadonlyFields;
            _includeFields = options._includeFields;
            _newLine = options._newLine;
            _propertyNameCaseInsensitive = options._propertyNameCaseInsensitive;
            _writeIndented = options._writeIndented;
            _indentCharacter = options._indentCharacter;
            _indentSize = options._indentSize;
            _typeInfoResolver = options._typeInfoResolver;
            EffectiveMaxDepth = options.EffectiveMaxDepth;
            ReferenceHandlingStrategy = options.ReferenceHandlingStrategy;

            TrackOptionsInstance(this);
        }

        /// <summary>
        /// Constructs a new <see cref="KdlSerializerOptions"/> instance with a predefined set of options determined by the specified <see cref="KdlSerializerDefaults"/>.
        /// </summary>
        /// <param name="defaults"> The <see cref="KdlSerializerDefaults"/> to reason about.</param>
        public KdlSerializerOptions(KdlSerializerDefaults defaults) : this()
        {
            // Should be kept in sync with equivalent overload in KdlSourceGenerationOptionsAttribute

            if (defaults == KdlSerializerDefaults.Web)
            {
                _propertyNameCaseInsensitive = true;
                _jsonPropertyNamingPolicy = KdlNamingPolicy.CamelCase;
                _numberHandling = KdlNumberHandling.AllowReadingFromString;
            }
            else if (defaults != KdlSerializerDefaults.General)
            {
                throw new ArgumentOutOfRangeException(nameof(defaults));
            }
        }

        /// <summary>Tracks the options instance to enable all instances to be enumerated.</summary>
        private static void TrackOptionsInstance(KdlSerializerOptions options) => TrackedOptionsInstances.All.Add(options, null);

        internal static class TrackedOptionsInstances
        {
            /// <summary>Tracks all live KdlSerializerOptions instances.</summary>
            /// <remarks>Instances are added to the table in their constructor.</remarks>
            public static ConditionalWeakTable<KdlSerializerOptions, object?> All { get; } =
                // TODO https://github.com/dotnet/runtime/issues/51159:
                // Look into linking this away / disabling it when hot reload isn't in use.
                [];
        }

        /// <summary>
        /// Gets or sets the <see cref="KdlTypeInfo"/> contract resolver used by this instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <remarks>
        /// A <see langword="null"/> setting is equivalent to using the reflection-based <see cref="DefaultKdlTypeInfoResolver" />.
        /// The property will be populated automatically once used with one of the <see cref="KdlSerializer"/> methods.
        ///
        /// This property is kept in sync with the <see cref="TypeInfoResolverChain"/> property.
        /// Any change made to this property will be reflected by <see cref="TypeInfoResolverChain"/> and vice versa.
        /// </remarks>
        public IKdlTypeInfoResolver? TypeInfoResolver
        {
            get => _typeInfoResolver;
            set
            {
                VerifyMutable();

                if (_typeInfoResolverChain is { } resolverChain && !ReferenceEquals(resolverChain, value))
                {
                    // User is setting a new resolver; invalidate the resolver chain if already created.
                    resolverChain.Clear();
                    resolverChain.AddFlattened(value);
                }

                _typeInfoResolver = value;
            }
        }

        /// <summary>
        /// Gets the list of chained <see cref="KdlTypeInfo"/> contract resolvers used by this instance.
        /// </summary>
        /// <remarks>
        /// The ordering of the chain is significant: <see cref="KdlSerializerOptions "/> will query each
        /// of the resolvers in their specified order, returning the first result that is non-null.
        /// If all resolvers in the chain return null, then <see cref="KdlSerializerOptions"/> will also return null.
        ///
        /// This property is auxiliary to and is kept in sync with the <see cref="TypeInfoResolver"/> property.
        /// Any change made to this property will be reflected by <see cref="TypeInfoResolver"/> and vice versa.
        /// </remarks>
        public IList<IKdlTypeInfoResolver> TypeInfoResolverChain => _typeInfoResolverChain ??= new(this);
        private OptionsBoundKdlTypeInfoResolverChain? _typeInfoResolverChain;

        /// <summary>
        /// Allows KDL metadata properties to be specified after regular properties in a deserialized KDL object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <remarks>
        /// When set to <see langword="true" />, removes the requirement that KDL metadata properties
        /// such as $id and $type should be specified at the very start of the deserialized KDL object.
        ///
        /// It should be noted that enabling this setting can result in over-buffering
        /// when deserializing large KDL payloads in the context of streaming deserialization.
        /// </remarks>
        public bool AllowOutOfOrderMetadataProperties
        {
            get => _allowOutOfOrderMetadataProperties;
            set
            {
                VerifyMutable();
                _allowOutOfOrderMetadataProperties = value;
            }
        }

        /// <summary>
        /// Defines whether an extra comma at the end of a list of KDL values in an object or array
        /// is allowed (and ignored) within the KDL payload being deserialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <remarks>
        /// By default, it's set to false, and <exception cref="KdlException"/> is thrown if a trailing comma is encountered.
        /// </remarks>
        public bool AllowTrailingCommas
        {
            get => _allowTrailingCommas;
            set
            {
                VerifyMutable();
                _allowTrailingCommas = value;
            }
        }

        /// <summary>
        /// The default buffer size in bytes used when creating temporary buffers.
        /// </summary>
        /// <remarks>The default size is 16K.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when the buffer size is less than 1.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public int DefaultBufferSize
        {
            get => _defaultBufferSize;
            set
            {
                VerifyMutable();

                if (value < 1)
                {
                    throw new ArgumentException(SR.SerializationInvalidBufferSize);
                }

                _defaultBufferSize = value;
            }
        }

        /// <summary>
        /// The encoder to use when escaping strings, or <see langword="null" /> to use the default encoder.
        /// </summary>
        public JavaScriptEncoder? Encoder
        {
            get => _encoder;
            set
            {
                VerifyMutable();

                _encoder = value;
            }
        }

        /// <summary>
        /// Specifies the policy used to convert a <see cref="System.Collections.IDictionary"/> key's name to another format, such as camel-casing.
        /// </summary>
        /// <remarks>
        /// This property can be set to <see cref="KdlNamingPolicy.CamelCase"/> to specify a camel-casing policy.
        /// It is not used when deserializing.
        /// </remarks>
        public KdlNamingPolicy? DictionaryKeyPolicy
        {
            get => _dictionaryKeyPolicy;
            set
            {
                VerifyMutable();
                _dictionaryKeyPolicy = value;
            }
        }

        /// <summary>
        /// Specifies a condition to determine when properties with default values are ignored during serialization or deserialization.
        /// The default value is <see cref="KdlIgnoreCondition.Never" />.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if this property is set to <see cref="KdlIgnoreCondition.Always"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred,
        /// or <see cref="IgnoreNullValues"/> has been set to <see langword="true"/>. These properties cannot be used together.
        /// </exception>
        public KdlIgnoreCondition DefaultIgnoreCondition
        {
            get => _defaultIgnoreCondition;
            set
            {
                VerifyMutable();

                if (value == KdlIgnoreCondition.Always)
                {
                    throw new ArgumentException(SR.DefaultIgnoreConditionInvalid);
                }

                if (value != KdlIgnoreCondition.Never && _ignoreNullValues)
                {
                    throw new InvalidOperationException(SR.DefaultIgnoreConditionAlreadySpecified);
                }

                _defaultIgnoreCondition = value;
            }
        }

        /// <summary>
        /// Specifies how number types should be handled when serializing or deserializing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public KdlNumberHandling NumberHandling
        {
            get => _numberHandling;
            set
            {
                VerifyMutable();

                if (!KdlSerializer.IsValidNumberHandlingValue(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _numberHandling = value;
            }
        }

        /// <summary>
        /// Specifies preferred object creation handling for properties when deserializing KDL.
        /// When set to <see cref="KdlObjectCreationHandling.Populate"/> all properties which
        /// are capable of reusing the existing instance will be populated.
        /// </summary>
        /// <remarks>
        /// Only property type is taken into consideration. For example if property is of type
        /// <see cref="IEnumerable{T}"/> but it is assigned <see cref="List{T}"/> it will not be populated
        /// because <see cref="IEnumerable{T}"/> is not capable of populating.
        /// Additionally value types require a setter to be populated.
        /// </remarks>
        public KdlObjectCreationHandling PreferredObjectCreationHandling
        {
            get => _preferredObjectCreationHandling;
            set
            {
                VerifyMutable();

                if (!KdlSerializer.IsValidCreationHandlingValue(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _preferredObjectCreationHandling = value;
            }
        }

        /// <summary>
        /// Determines whether read-only properties are ignored during serialization.
        /// A property is read-only if it contains a public getter but not a public setter.
        /// The default value is false.
        /// </summary>
        /// <remarks>
        /// Read-only properties are not deserialized regardless of this setting.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool IgnoreReadOnlyProperties
        {
            get => _ignoreReadOnlyProperties;
            set
            {
                VerifyMutable();
                _ignoreReadOnlyProperties = value;
            }
        }

        /// <summary>
        /// Determines whether read-only fields are ignored during serialization.
        /// A field is read-only if it is marked with the <c>readonly</c> keyword.
        /// The default value is false.
        /// </summary>
        /// <remarks>
        /// Read-only fields are not deserialized regardless of this setting.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool IgnoreReadOnlyFields
        {
            get => _ignoreReadonlyFields;
            set
            {
                VerifyMutable();
                _ignoreReadonlyFields = value;
            }
        }

        /// <summary>
        /// Determines whether fields are handled on serialization and deserialization.
        /// The default value is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool IncludeFields
        {
            get => _includeFields;
            set
            {
                VerifyMutable();
                _includeFields = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when serializing or deserializing KDL, with the default (i.e. 0) indicating a max depth of 64.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the max depth is set to a negative value.
        /// </exception>
        /// <remarks>
        /// Going past this depth will throw a <exception cref="KdlException"/>.
        /// </remarks>
        public int MaxDepth
        {
            get => _maxDepth;
            set
            {
                VerifyMutable();

                if (value < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_MaxDepthMustBePositive(nameof(value));
                }

                _maxDepth = value;
                EffectiveMaxDepth = value == 0 ? DefaultMaxDepth : value;
            }
        }

        internal int EffectiveMaxDepth { get; private set; } = DefaultMaxDepth;

        /// <summary>
        /// Specifies the policy used to convert a property's name on an object to another format, such as camel-casing.
        /// The resulting property name is expected to match the KDL payload during deserialization, and
        /// will be used when writing the property name during serialization.
        /// </summary>
        /// <remarks>
        /// The policy is not used for properties that have a <see cref="KdlPropertyNameAttribute"/> applied.
        /// This property can be set to <see cref="KdlNamingPolicy.CamelCase"/> to specify a camel-casing policy.
        /// </remarks>
        public KdlNamingPolicy? PropertyNamingPolicy
        {
            get => _jsonPropertyNamingPolicy;
            set
            {
                VerifyMutable();
                _jsonPropertyNamingPolicy = value;
            }
        }

        /// <summary>
        /// Determines whether a property's name uses a case-insensitive comparison during deserialization.
        /// The default value is false.
        /// </summary>
        /// <remarks>There is a performance cost associated when the value is true.</remarks>
        public bool PropertyNameCaseInsensitive
        {
            get => _propertyNameCaseInsensitive;
            set
            {
                VerifyMutable();
                _propertyNameCaseInsensitive = value;
            }
        }

        /// <summary>
        /// Defines how the comments are handled during deserialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the comment handling enum is set to a value that is not supported (or not within the <see cref="KdlCommentHandling"/> enum range).
        /// </exception>
        /// <remarks>
        /// By default <exception cref="KdlException"/> is thrown if a comment is encountered.
        /// </remarks>
        public KdlCommentHandling ReadCommentHandling
        {
            get => _readCommentHandling;
            set
            {
                VerifyMutable();

                Debug.Assert(value >= 0);
                if (value > KdlCommentHandling.Skip)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.KdlSerializerDoesNotSupportComments);
                }

                _readCommentHandling = value;
            }
        }

        /// <summary>
        /// Defines how deserializing a type declared as an <see cref="object"/> is handled during deserialization.
        /// </summary>
        public KdlUnknownTypeHandling UnknownTypeHandling
        {
            get => _unknownTypeHandling;
            set
            {
                VerifyMutable();
                _unknownTypeHandling = value;
            }
        }

        /// <summary>
        /// Determines how <see cref="KdlSerializer"/> handles KDL properties that
        /// cannot be mapped to a specific .NET member when deserializing object types.
        /// </summary>
        public KdlUnmappedMemberHandling UnmappedMemberHandling
        {
            get => _unmappedMemberHandling;
            set
            {
                VerifyMutable();
                _unmappedMemberHandling = value;
            }
        }

        /// <summary>
        /// Defines whether KDL should pretty print which includes:
        /// indenting nested KDL tokens, adding new lines, and adding white space between property names and values.
        /// By default, the KDL is serialized without any extra white space.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool WriteIndented
        {
            get => _writeIndented;
            set
            {
                VerifyMutable();
                _writeIndented = value;
            }
        }

        /// <summary>
        /// Defines the indentation character being used when <see cref="WriteIndented" /> is enabled. Defaults to the space character.
        /// </summary>
        /// <remarks>Allowed characters are space and horizontal tab.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> contains an invalid character.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public char IndentCharacter
        {
            get => _indentCharacter;
            set
            {
                KdlWriterHelper.ValidateIndentCharacter(value);
                VerifyMutable();
                _indentCharacter = value;
            }
        }

        /// <summary>
        /// Defines the indentation size being used when <see cref="WriteIndented" /> is enabled. Defaults to two.
        /// </summary>
        /// <remarks>Allowed values are all integers between 0 and 127, included.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of the allowed range.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public int IndentSize
        {
            get => _indentSize;
            set
            {
                KdlWriterHelper.ValidateIndentSize(value);
                VerifyMutable();
                _indentSize = value;
            }
        }

        /// <summary>
        /// Configures how object references are handled when reading and writing KDL.
        /// </summary>
        public ReferenceHandler? ReferenceHandler
        {
            get => _referenceHandler;
            set
            {
                VerifyMutable();
                _referenceHandler = value;
                ReferenceHandlingStrategy = value?.HandlingStrategy ?? KdlKnownReferenceHandler.Unspecified;
            }
        }

        /// <summary>
        /// Gets or sets the new line string to use when <see cref="WriteIndented"/> is <see langword="true"/>.
        /// The default is the value of <see cref="Environment.NewLine"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the new line string is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the new line string is not <c>\n</c> or <c>\r\n</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public string NewLine
        {
            get => _newLine ??= Environment.NewLine;
            set
            {
                KdlWriterHelper.ValidateNewLine(value);
                VerifyMutable();
                _newLine = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether nullability annotations should be respected during serialization and deserialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <remarks>
        /// Nullability annotations are resolved from the properties, fields and constructor parameters
        /// that are used by the serializer. This includes annotations stemming from attributes such as
        /// <see cref="NotNullAttribute"/>, <see cref="MaybeNullAttribute"/>,
        /// <see cref="AllowNullAttribute"/> and <see cref="DisallowNullAttribute"/>.
        ///
        /// Due to restrictions in how nullable reference types are represented at run time,
        /// this setting only governs nullability annotations of non-generic properties and fields.
        /// It cannot be used to enforce nullability annotations of root-level types or generic parameters.
        ///
        /// The default setting for this property can be toggled application-wide using the
        /// "System.Text.Kdl.Serialization.RespectNullableAnnotationsDefault" feature switch.
        /// </remarks>
        public bool RespectNullableAnnotations
        {
            get => _respectNullableAnnotations;
            set
            {
                VerifyMutable();
                _respectNullableAnnotations = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether non-optional constructor parameters should be specified during deserialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        /// <remarks>
        /// For historical reasons constructor-based deserialization treats all constructor parameters as optional by default.
        /// This flag allows users to toggle that behavior as necessary for each <see cref="KdlSerializerOptions"/> instance.
        ///
        /// The default setting for this property can be toggled application-wide using the
        /// "System.Text.Kdl.Serialization.RespectRequiredConstructorParametersDefault" feature switch.
        /// </remarks>
        public bool RespectRequiredConstructorParameters
        {
            get => _respectRequiredConstructorParameters;
            set
            {
                VerifyMutable();
                _respectRequiredConstructorParameters = value;
            }
        }

        /// <summary>
        /// Returns true if options uses compatible built-in resolvers or a combination of compatible built-in resolvers.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool CanUseFastPathSerializationLogic
        {
            get
            {
                Debug.Assert(IsReadOnly);
                Debug.Assert(TypeInfoResolver != null);
                return _canUseFastPathSerializationLogic ??= TypeInfoResolver.IsCompatibleWithOptions(this);
            }
        }

        private bool? _canUseFastPathSerializationLogic;

        // The cached value used to determine if ReferenceHandler should use Preserve or IgnoreCycles semantics or None of them.
        internal KdlKnownReferenceHandler ReferenceHandlingStrategy = KdlKnownReferenceHandler.Unspecified;

        /// <summary>
        /// Specifies whether the current instance has been locked for user modification.
        /// </summary>
        /// <remarks>
        /// A <see cref="KdlSerializerOptions"/> instance can be locked either if
        /// it has been passed to one of the <see cref="KdlSerializer"/> methods,
        /// has been associated with a <see cref="KdlSerializerContext"/> instance,
        /// or a user explicitly called the <see cref="MakeReadOnly()"/> methods on the instance.
        ///
        /// Read-only instances use caching when querying <see cref="KdlConverter"/> and <see cref="KdlTypeInfo"/> metadata.
        /// </remarks>
        public bool IsReadOnly => _isReadOnly;
        private volatile bool _isReadOnly;

        /// <summary>
        /// Marks the current instance as read-only preventing any further user modification.
        /// </summary>
        /// <exception cref="InvalidOperationException">The instance does not specify a <see cref="TypeInfoResolver"/> setting.</exception>
        /// <remarks>This method is idempotent.</remarks>
        public void MakeReadOnly()
        {
            if (_typeInfoResolver is null)
            {
                ThrowHelper.ThrowInvalidOperationException_KdlSerializerOptionsNoTypeInfoResolverSpecified();
            }

            _isReadOnly = true;
        }

        /// <summary>
        /// Marks the current instance as read-only preventing any further user modification.
        /// </summary>
        /// <param name="populateMissingResolver">Populates unconfigured <see cref="TypeInfoResolver"/> properties with the reflection-based default.</param>
        /// <exception cref="InvalidOperationException">
        /// The instance does not specify a <see cref="TypeInfoResolver"/> setting. Thrown if <paramref name="populateMissingResolver"/> is <see langword="false"/>.
        /// -OR-
        /// The <see cref="KdlSerializer.IsReflectionEnabledByDefault"/> feature switch has been turned off.
        /// </exception>
        /// <remarks>
        /// When <paramref name="populateMissingResolver"/> is set to <see langword="true" />, configures the instance following
        /// the semantics of the <see cref="KdlSerializer"/> methods accepting <see cref="KdlSerializerOptions"/> parameters.
        ///
        /// This method is idempotent.
        /// </remarks>
        [RequiresUnreferencedCode("Populating unconfigured TypeInfoResolver properties with the reflection resolver requires unreferenced code.")]
        [RequiresDynamicCode("Populating unconfigured TypeInfoResolver properties with the reflection resolver requires runtime code generation.")]
        public void MakeReadOnly(bool populateMissingResolver)
        {
            if (populateMissingResolver)
            {
                if (!_isConfiguredForKdlSerializer)
                {
                    ConfigureForKdlSerializer();
                }
            }
            else
            {
                MakeReadOnly();
            }

            Debug.Assert(IsReadOnly);
        }

        /// <summary>
        /// Configures the instance for use by the KdlSerializer APIs, applying reflection-based fallback where applicable.
        /// </summary>
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private void ConfigureForKdlSerializer()
        {
            if (KdlSerializer.IsReflectionEnabledByDefault)
            {
                // Even if a resolver has already been specified, we need to root
                // the default resolver to gain access to the default converters.
                DefaultKdlTypeInfoResolver defaultResolver = DefaultKdlTypeInfoResolver.DefaultInstance;

                switch (_typeInfoResolver)
                {
                    case null:
                        // Use the default reflection-based resolver if no resolver has been specified.
                        _typeInfoResolver = defaultResolver;
                        break;

                    case KdlSerializerContext ctx when AppContextSwitchHelper.IsSourceGenReflectionFallbackEnabled:
                        // .NET 6 compatibility mode: enable fallback to reflection metadata for KdlSerializerContext
                        _effectiveKdlTypeInfoResolver = KdlTypeInfoResolver.Combine(ctx, defaultResolver);

                        if (_cachingContext is { } cachingContext)
                        {
                            // A cache has already been created by the source generator.
                            // Repeat the same configuration routine for that options instance, if different.
                            // Invalidate any cache entries that have already been stored.
                            if (cachingContext.Options != this && !cachingContext.Options._isConfiguredForKdlSerializer)
                            {
                                cachingContext.Options.ConfigureForKdlSerializer();
                            }
                            else
                            {
                                cachingContext.Clear();
                            }
                        }
                        break;
                }
            }
            else if (_typeInfoResolver is null or EmptyKdlTypeInfoResolver)
            {
                ThrowHelper.ThrowInvalidOperationException_KdlSerializerIsReflectionDisabled();
            }

            Debug.Assert(_typeInfoResolver != null);
            // NB preserve write order.
            _isReadOnly = true;
            _isConfiguredForKdlSerializer = true;
        }

        /// <summary>
        /// This flag is supplementary to <see cref="_isReadOnly"/> and is only used to keep track
        /// of source-gen reflection fallback (assuming the IsSourceGenReflectionFallbackEnabled feature switch is on).
        /// This mode necessitates running the <see cref="ConfigureForKdlSerializer"/> method even
        /// for options instances that have been marked as read-only.
        /// </summary>
        private volatile bool _isConfiguredForKdlSerializer;

        // Only populated in .NET 6 compatibility mode encoding reflection fallback in source gen
        private IKdlTypeInfoResolver? _effectiveKdlTypeInfoResolver;

        private KdlTypeInfo? GetTypeInfoNoCaching(Type type)
        {
            IKdlTypeInfoResolver? resolver = _effectiveKdlTypeInfoResolver ?? _typeInfoResolver;
            if (resolver is null)
            {
                return null;
            }

            KdlTypeInfo? info = resolver.GetTypeInfo(type, this);

            if (info != null)
            {
                if (info.Type != type)
                {
                    ThrowHelper.ThrowInvalidOperationException_ResolverTypeNotCompatible(type, info.Type);
                }

                if (info.Options != this)
                {
                    ThrowHelper.ThrowInvalidOperationException_ResolverTypeInfoOptionsNotCompatible();
                }
            }
            else
            {
                Debug.Assert(_effectiveKdlTypeInfoResolver is null, "an effective resolver always returns metadata");

                if (type == KdlTypeInfo.ObjectType)
                {
                    // If the resolver does not provide a KdlTypeInfo<object> instance, fill
                    // with the serialization-only converter to enable polymorphic serialization.
                    var converter = new SlimObjectConverter(resolver);
                    info = new KdlTypeInfo<object>(converter, this);
                }
            }

            return info;
        }

        internal KdlReadOnlyDocumentOptions GetDocumentOptions()
        {
            return new KdlReadOnlyDocumentOptions
            {
                AllowTrailingCommas = AllowTrailingCommas,
                CommentHandling = ReadCommentHandling,
                MaxDepth = MaxDepth
            };
        }

        internal KdlElementOptions GetNodeOptions()
        {
            return new KdlElementOptions
            {
                PropertyNameCaseInsensitive = PropertyNameCaseInsensitive
            };
        }

        internal KdlReaderOptions GetReaderOptions()
        {
            return new KdlReaderOptions
            {
                AllowTrailingCommas = AllowTrailingCommas,
                CommentHandling = ReadCommentHandling,
                MaxDepth = EffectiveMaxDepth
            };
        }

        internal KdlWriterOptions GetWriterOptions()
        {
            return new KdlWriterOptions
            {
                Encoder = Encoder,
                Indented = WriteIndented,
                IndentCharacter = IndentCharacter,
                IndentSize = IndentSize,
                MaxDepth = EffectiveMaxDepth,
                NewLine = NewLine,
#if !DEBUG
                SkipValidation = true
#endif
            };
        }

        internal void VerifyMutable()
        {
            if (_isReadOnly)
            {
                ThrowHelper.ThrowInvalidOperationException_SerializerOptionsReadOnly(_typeInfoResolver as KdlSerializerContext);
            }
        }

        private sealed class ConverterList(KdlSerializerOptions options, IList<KdlConverter>? source = null) : ConfigurationList<KdlConverter>(source)
        {
            private readonly KdlSerializerOptions _options = options;

            public override bool IsReadOnly => _options.IsReadOnly;
            protected override void OnCollectionModifying() => _options.VerifyMutable();
        }

        private sealed class OptionsBoundKdlTypeInfoResolverChain : KdlTypeInfoResolverChain
        {
            private readonly KdlSerializerOptions _options;

            public OptionsBoundKdlTypeInfoResolverChain(KdlSerializerOptions options)
            {
                _options = options;
                AddFlattened(options._typeInfoResolver);
            }

            public override bool IsReadOnly => _options.IsReadOnly;

            protected override void ValidateAddedValue(IKdlTypeInfoResolver item)
            {
                if (ReferenceEquals(item, this) || ReferenceEquals(item, _options._typeInfoResolver))
                {
                    // Cannot add the instances in TypeInfoResolver or TypeInfoResolverChain to the chain itself.
                    ThrowHelper.ThrowInvalidOperationException_InvalidChainedResolver();
                }
            }

            protected override void OnCollectionModifying()
            {
                _options.VerifyMutable();
            }

            protected override void OnCollectionModified()
            {
                // Collection modified by the user: replace the main
                // resolver with the resolver chain as our source of truth.
                _options._typeInfoResolver = this;
            }
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlSerializerOptions GetOrCreateSingleton(
            ref KdlSerializerOptions? location,
            KdlSerializerDefaults defaults)
        {
            var options = new KdlSerializerOptions(defaults)
            {
                // Because we're marking the default instance as read-only,
                // we need to specify a resolver instance for the case where
                // reflection is disabled by default: use one that returns null for all types.

                TypeInfoResolver = KdlSerializer.IsReflectionEnabledByDefault
                    ? DefaultKdlTypeInfoResolver.DefaultInstance
                    : KdlTypeInfoResolver.Empty,

                _isReadOnly = true,
            };

            return Interlocked.CompareExchange(ref location, options, null) ?? options;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"TypeInfoResolver = {TypeInfoResolver?.ToString() ?? "<null>"}, IsReadOnly = {IsReadOnly}";
    }
}
