using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.Kdl.Reflection;
using System.Text.Kdl.Serialization.Converters;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides KDL serialization-related metadata about a type.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract partial class KdlTypeInfo
    {
        internal const string MetadataFactoryRequiresUnreferencedCode = "KDL serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Kdl source generation for native AOT applications.";

        internal const string KdlObjectTypeName = "System.Text.Kdl.Nodes.KdlNode";

        internal delegate T ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>(TArg0? arg0, TArg1? arg1, TArg2? arg2, TArg3? arg3);

        /// <summary>
        /// Indices of required properties.
        /// </summary>
        internal int NumberOfRequiredProperties { get; private set; }

        private Action<object>? _onSerializing;
        private Action<object>? _onSerialized;
        private Action<object>? _onDeserializing;
        private Action<object>? _onDeserialized;

        internal KdlTypeInfo(Type type, KdlConverter converter, KdlSerializerOptions options)
        {
            Type = type;
            Options = options;
            Converter = converter;
            Kind = GetTypeInfoKind(type, converter);
            PropertyInfoForTypeInfo = CreatePropertyInfoForTypeInfo();
            ElementType = converter.ElementType;
            KeyType = converter.KeyType;
        }

        /// <summary>
        /// Gets the element type corresponding to an enumerable, dictionary or optional type.
        /// </summary>
        /// <remarks>
        /// Returns the element type for enumerable types, the value type for dictionary types,
        /// and the underlying type for <see cref="Nullable{T}"/> or F# optional types.
        ///
        /// Returns <see langword="null"/> for all other types or types using custom converters.
        /// </remarks>
        public Type? ElementType { get; }

        /// <summary>
        /// Gets the key type corresponding to a dictionary type.
        /// </summary>
        /// <remarks>
        /// Returns the key type for dictionary types.
        ///
        /// Returns <see langword="null"/> for all other types or types using custom converters.
        /// </remarks>
        public Type? KeyType { get; }

        /// <summary>
        /// Gets or sets a parameterless factory to be used on deserialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// A parameterless factory is not supported for the current metadata <see cref="Kind"/>.
        /// </exception>
        /// <remarks>
        /// If set to <see langword="null" />, any attempt to deserialize instances of the given type will result in an exception.
        ///
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// types with a single default constructor or default constructors annotated with <see cref="KdlConstructorAttribute"/>
        /// will be mapped to this delegate.
        /// </remarks>
        public Func<object>? CreateObject
        {
            get => _createObject;
            set => SetCreateObject(value);
        }

        private protected abstract void SetCreateObject(Delegate? createObject);
        private protected Func<object>? _createObject;

        internal Func<object>? CreateObjectForExtensionDataProperty { get; set; }

        /// <summary>
        /// Gets or sets a callback to be invoked before serialization occurs.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Serialization callbacks are only supported for <see cref="KdlTypeInfoKind.Object"/> metadata.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from any <see cref="IKdlOnSerializing"/> implementation on the type.
        /// </remarks>
        public Action<object>? OnSerializing
        {
            get => _onSerializing;
            set
            {
                VerifyMutable();

                if (Kind is not (KdlTypeInfoKind.Object or KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary))
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                }

                _onSerializing = value;
            }
        }

        /// <summary>
        /// Gets or sets a callback to be invoked after serialization occurs.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Serialization callbacks are only supported for <see cref="KdlTypeInfoKind.Object"/> metadata.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from any <see cref="IKdlOnSerialized"/> implementation on the type.
        /// </remarks>
        public Action<object>? OnSerialized
        {
            get => _onSerialized;
            set
            {
                VerifyMutable();

                if (Kind is not (KdlTypeInfoKind.Object or KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary))
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                }

                _onSerialized = value;
            }
        }

        /// <summary>
        /// Gets or sets a callback to be invoked before deserialization occurs.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Serialization callbacks are only supported for <see cref="KdlTypeInfoKind.Object"/> metadata.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from any <see cref="IKdlOnDeserializing"/> implementation on the type.
        /// </remarks>
        public Action<object>? OnDeserializing
        {
            get => _onDeserializing;
            set
            {
                VerifyMutable();

                if (Kind is not (KdlTypeInfoKind.Object or KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary))
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                }

                if (Converter.IsConvertibleCollection)
                {
                    // The values for convertible collections aren't available at the start of deserialization.
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOnDeserializingCallbacksNotSupported(Type);
                }

                _onDeserializing = value;
            }
        }

        /// <summary>
        /// Gets or sets a callback to be invoked after deserialization occurs.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Serialization callbacks are only supported for <see cref="KdlTypeInfoKind.Object"/> metadata.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from any <see cref="IKdlOnDeserialized"/> implementation on the type.
        /// </remarks>
        public Action<object>? OnDeserialized
        {
            get => _onDeserialized;
            set
            {
                VerifyMutable();

                if (Kind is not (KdlTypeInfoKind.Object or KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary))
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                }

                _onDeserialized = value;
            }
        }

        /// <summary>
        /// Gets the list of <see cref="KdlPropertyInfo"/> metadata corresponding to the current type.
        /// </summary>
        /// <remarks>
        /// Property is only applicable to metadata of kind <see cref="KdlTypeInfoKind.Object"/>.
        /// For other kinds an empty, read-only list will be returned.
        ///
        /// The order of <see cref="KdlPropertyInfo"/> entries in the list determines the serialization order,
        /// unless either of the entries specifies a non-zero <see cref="KdlPropertyInfo.Order"/> value,
        /// in which case the properties will be stable sorted by <see cref="KdlPropertyInfo.Order"/>.
        ///
        /// It is required that added <see cref="KdlPropertyInfo"/> entries are unique up to <see cref="KdlPropertyInfo.Name"/>,
        /// however this will only be validated on serialization, once the metadata instance gets locked for further modification.
        /// </remarks>
        public IList<KdlPropertyInfo> Properties => PropertyList;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal KdlPropertyInfoList PropertyList
        {
            get
            {
                return _properties ?? CreatePropertyList();
                KdlPropertyInfoList CreatePropertyList()
                {
                    var list = new KdlPropertyInfoList(this);
                    if (_sourceGenDelayedPropertyInitializer is { } propInit)
                    {
                        // .NET 6 source gen backward compatibility -- ensure that the
                        // property initializer delegate is invoked lazily.
                        KdlMetadataServices.PopulateProperties(this, list, propInit);
                    }

                    KdlPropertyInfoList? result = Interlocked.CompareExchange(ref _properties, list, null);
                    _sourceGenDelayedPropertyInitializer = null;
                    return result ?? list;
                }
            }
        }

        /// <summary>
        /// Stores the .NET 6-style property initialization delegate for delayed evaluation.
        /// </summary>
        internal Func<KdlSerializerContext, KdlPropertyInfo[]>? SourceGenDelayedPropertyInitializer
        {
            get => _sourceGenDelayedPropertyInitializer;
            set
            {
                Debug.Assert(!IsReadOnly);
                Debug.Assert(_properties is null, "must not be set if a property list has been initialized.");
                _sourceGenDelayedPropertyInitializer = value;
            }
        }

        private Func<KdlSerializerContext, KdlPropertyInfo[]>? _sourceGenDelayedPropertyInitializer;
        private KdlPropertyInfoList? _properties;

        /// <summary>
        /// Gets or sets a configuration object specifying polymorphism metadata.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="value" /> has been associated with a different <see cref="KdlTypeInfo"/> instance.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Polymorphic serialization is not supported for the current metadata <see cref="Kind"/>.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the configuration of this setting will be mapped from any <see cref="KdlDerivedTypeAttribute"/> or <see cref="KdlPolymorphicAttribute"/> annotations.
        /// </remarks>
        public KdlPolymorphismOptions? PolymorphismOptions
        {
            get => _polymorphismOptions;
            set
            {
                VerifyMutable();

                if (value != null)
                {
                    if (Kind == KdlTypeInfoKind.None)
                    {
                        ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                    }

                    if (value.DeclaringTypeInfo != null && value.DeclaringTypeInfo != this)
                    {
                        ThrowHelper.ThrowArgumentException_KdlPolymorphismOptionsAssociatedWithDifferentKdlTypeInfo(nameof(value));
                    }

                    value.DeclaringTypeInfo = this;
                }

                _polymorphismOptions = value;
            }
        }

        /// <summary>
        /// Specifies whether the current instance has been locked for modification.
        /// </summary>
        /// <remarks>
        /// A <see cref="KdlTypeInfo"/> instance can be locked either if
        /// it has been passed to one of the <see cref="KdlSerializer"/> methods,
        /// has been associated with a <see cref="KdlSerializerContext"/> instance,
        /// or a user explicitly called the <see cref="MakeReadOnly"/> method on the instance.
        /// </remarks>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Locks the current instance for further modification.
        /// </summary>
        /// <remarks>This method is idempotent.</remarks>
        public void MakeReadOnly() => IsReadOnly = true;

        private protected KdlPolymorphismOptions? _polymorphismOptions;

        internal object? CreateObjectWithArgs { get; set; }

        // Add method delegate for non-generic Stack and Queue; and types that derive from them.
        internal object? AddMethodDelegate { get; set; }

        internal KdlPropertyInfo? ExtensionDataProperty { get; private set; }

        internal PolymorphicTypeResolver? PolymorphicTypeResolver { get; private set; }

        // Indicates that SerializeHandler is populated.
        internal bool HasSerializeHandler { get; private protected set; }

        // Indicates that SerializeHandler is populated and is compatible with the associated contract metadata.
        internal bool CanUseSerializeHandler { get; private set; }

        // Configure would normally have thrown why initializing properties for source gen but type had SerializeHandler
        // so it is allowed to be used for fast-path serialization but it will throw if used for metadata-based serialization
        internal bool PropertyMetadataSerializationNotSupported { get; set; }

        internal bool IsNullable => Converter.NullableElementConverter is not null;
        internal bool CanBeNull => PropertyInfoForTypeInfo.PropertyTypeCanBeNull;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ValidateCanBeUsedForPropertyMetadataSerialization()
        {
            if (PropertyMetadataSerializationNotSupported)
            {
                ThrowHelper.ThrowInvalidOperationException_NoMetadataForTypeProperties(Options.TypeInfoResolver, Type);
            }
        }

        /// <summary>
        /// Return the KdlTypeInfo for the element type, or null if the type is not an enumerable or dictionary.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal KdlTypeInfo? ElementTypeInfo
        {
            get
            {
                Debug.Assert(IsConfigured);
                Debug.Assert(_elementTypeInfo is null or { IsConfigurationStarted: true });
                // Even though this instance has already been configured,
                // it is possible for contending threads to call the property
                // while the wider KdlTypeInfo graph is still being configured.
                // Call EnsureConfigured() to force synchronization if necessary.
                KdlTypeInfo? elementTypeInfo = _elementTypeInfo;
                elementTypeInfo?.EnsureConfigured();
                return elementTypeInfo;
            }
            set
            {
                Debug.Assert(!IsReadOnly);
                Debug.Assert(value is null || value.Type == ElementType);
                _elementTypeInfo = value;
            }
        }

        /// <summary>
        /// Return the KdlTypeInfo for the key type, or null if the type is not a dictionary.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal KdlTypeInfo? KeyTypeInfo
        {
            get
            {
                Debug.Assert(IsConfigured);
                Debug.Assert(_keyTypeInfo is null or { IsConfigurationStarted: true });
                // Even though this instance has already been configured,
                // it is possible for contending threads to call the property
                // while the wider KdlTypeInfo graph is still being configured.
                // Call EnsureConfigured() to force synchronization if necessary.
                KdlTypeInfo? keyTypeInfo = _keyTypeInfo;
                keyTypeInfo?.EnsureConfigured();
                return keyTypeInfo;
            }
            set
            {
                Debug.Assert(!IsReadOnly);
                Debug.Assert(value is null || value.Type == KeyType);
                _keyTypeInfo = value;
            }
        }

        private KdlTypeInfo? _elementTypeInfo;
        private KdlTypeInfo? _keyTypeInfo;

        /// <summary>
        /// Gets the <see cref="KdlSerializerOptions"/> value associated with the current <see cref="KdlTypeInfo" /> instance.
        /// </summary>
        public KdlSerializerOptions Options { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> for which the KDL serialization contract is being defined.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the <see cref="KdlConverter"/> associated with the current type.
        /// </summary>
        /// <remarks>
        /// The <see cref="KdlConverter"/> associated with the type determines the value of <see cref="Kind"/>,
        /// and by extension the types of metadata that are configurable in the current KDL contract.
        /// As such, the value of the converter cannot be changed once a <see cref="KdlTypeInfo"/> instance has been created.
        /// </remarks>
        public KdlConverter Converter { get; }

        /// <summary>
        /// Determines the kind of contract metadata that the current instance is specifying.
        /// </summary>
        /// <remarks>
        /// The value of <see cref="Kind"/> determines what aspects of the KDL contract are configurable.
        /// For example, it is only possible to configure the <see cref="Properties"/> list for metadata
        /// of kind <see cref="KdlTypeInfoKind.Object"/>.
        ///
        /// The value of <see cref="Kind"/> is determined exclusively by the <see cref="KdlConverter"/>
        /// resolved for the current type, and cannot be changed once resolution has happened.
        /// User-defined custom converters (specified either via <see cref="KdlConverterAttribute"/> or <see cref="KdlSerializerOptions.Converters"/>)
        /// are metadata-agnostic and thus always resolve to <see cref="KdlTypeInfoKind.None"/>.
        /// </remarks>
        public KdlTypeInfoKind Kind { get; }

        /// <summary>
        /// Dummy <see cref="KdlPropertyInfo"/> instance corresponding to the declaring type of this <see cref="KdlTypeInfo"/>.
        /// </summary>
        /// <remarks>
        /// Used as convenience in cases where we want to serialize property-like values that do not define property metadata, such as:
        /// 1. a collection element type,
        /// 2. a dictionary key or value type or,
        /// 3. the property metadata for the root-level value.
        /// For example, for a property returning <see cref="List{T}"/> where T is a string,
        /// a KdlTypeInfo will be created with .Type=typeof(string) and .PropertyInfoForTypeInfo=KdlPropertyInfo{string}.
        /// </remarks>
        internal KdlPropertyInfo PropertyInfoForTypeInfo { get; }

        private protected abstract KdlPropertyInfo CreatePropertyInfoForTypeInfo();

        /// <summary>
        /// Gets or sets the type-level <see cref="KdlSerializerOptions.NumberHandling"/> override.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Specified an invalid <see cref="KdlNumberHandling"/> value.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from any <see cref="KdlNumberHandlingAttribute"/> annotations.
        /// </remarks>
        public KdlNumberHandling? NumberHandling
        {
            get => _numberHandling;
            set
            {
                VerifyMutable();

                if (value is not null && !KdlSerializer.IsValidNumberHandlingValue(value.Value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _numberHandling = value;
            }
        }

        internal KdlNumberHandling EffectiveNumberHandling => _numberHandling ?? Options.NumberHandling;
        private KdlNumberHandling? _numberHandling;

        /// <summary>
        /// Gets or sets the type-level <see cref="KdlUnmappedMemberHandling"/> override.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Unmapped member handling only supported for <see cref="KdlTypeInfoKind.Object"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Specified an invalid <see cref="KdlUnmappedMemberHandling"/> value.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from any <see cref="KdlUnmappedMemberHandlingAttribute"/> annotations.
        /// </remarks>
        public KdlUnmappedMemberHandling? UnmappedMemberHandling
        {
            get => _unmappedMemberHandling;
            set
            {
                VerifyMutable();

                if (Kind != KdlTypeInfoKind.Object)
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                }

                if (value is not null && !KdlSerializer.IsValidUnmappedMemberHandlingValue(value.Value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _unmappedMemberHandling = value;
            }
        }

        private KdlUnmappedMemberHandling? _unmappedMemberHandling;

        internal KdlUnmappedMemberHandling EffectiveUnmappedMemberHandling { get; private set; }

        private KdlObjectCreationHandling? _preferredPropertyObjectCreationHandling;

        /// <summary>
        /// Gets or sets the preferred <see cref="KdlObjectCreationHandling"/> value for properties contained in the type.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// Unmapped member handling only supported for <see cref="KdlTypeInfoKind.Object"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Specified an invalid <see cref="KdlObjectCreationHandling"/> value.
        /// </exception>
        /// <remarks>
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// the value of this callback will be mapped from <see cref="KdlObjectCreationHandlingAttribute"/> annotations on types.
        /// </remarks>
        public KdlObjectCreationHandling? PreferredPropertyObjectCreationHandling
        {
            get => _preferredPropertyObjectCreationHandling;
            set
            {
                VerifyMutable();

                if (Kind != KdlTypeInfoKind.Object)
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
                }

                if (value is not null && !KdlSerializer.IsValidCreationHandlingValue(value.Value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _preferredPropertyObjectCreationHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IKdlTypeInfoResolver"/> from which this metadata instance originated.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        /// </exception>
        /// <remarks>
        /// Metadata used to determine the <see cref="KdlSerializerContext.GeneratedSerializerOptions"/>
        /// configuration for the current metadata instance.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IKdlTypeInfoResolver? OriginatingResolver
        {
            get => _originatingResolver;
            set
            {
                VerifyMutable();

                if (value is KdlSerializerContext)
                {
                    // The source generator uses this property setter to brand the metadata instance as user-unmodified.
                    // Even though users could call the same property setter to unset this flag, this is generally speaking fine.
                    // This flag is only used to determine fast-path invalidation, worst case scenario this would lead to a false negative.
                    IsCustomized = false;
                }

                _originatingResolver = value;
            }
        }

        private IKdlTypeInfoResolver? _originatingResolver;

        /// <summary>
        /// Gets or sets an attribute provider corresponding to the deserialization constructor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlPropertyInfo"/> instance has been locked for further modification.
        /// </exception>
        /// <remarks>
        /// When resolving metadata via the built-in resolvers this will be populated with
        /// the underlying <see cref="ConstructorInfo" /> of the serialized property or field.
        /// </remarks>
        public ICustomAttributeProvider? ConstructorAttributeProvider
        {
            get
            {
                Func<ICustomAttributeProvider>? ctorAttrProviderFactory = Volatile.Read(ref ConstructorAttributeProviderFactory);
                ICustomAttributeProvider? ctorAttrProvider = _constructorAttributeProvider;

                if (ctorAttrProvider is null && ctorAttrProviderFactory is not null)
                {
                    _constructorAttributeProvider = ctorAttrProvider = ctorAttrProviderFactory();
                    Volatile.Write(ref ConstructorAttributeProviderFactory, null);
                }

                return ctorAttrProvider;
            }
            internal set
            {
                Debug.Assert(!IsReadOnly);

                _constructorAttributeProvider = value;
                Volatile.Write(ref ConstructorAttributeProviderFactory, null);
            }
        }

        // Metadata emanating from the source generator use delayed attribute provider initialization
        // ensuring that reflection metadata resolution remains pay-for-play and is trimmable.
        internal Func<ICustomAttributeProvider>? ConstructorAttributeProviderFactory;
        private ICustomAttributeProvider? _constructorAttributeProvider;

        internal void VerifyMutable()
        {
            if (IsReadOnly)
            {
                ThrowHelper.ThrowInvalidOperationException_TypeInfoImmutable();
            }

            IsCustomized = true;
        }

        /// <summary>
        /// Indicates that the current KdlTypeInfo might contain user modifications.
        /// Defaults to true, and is only unset by the built-in contract resolvers.
        /// </summary>
        internal bool IsCustomized { get; set; } = true;

        internal bool IsConfigured => _configurationState == ConfigurationState.Configured;
        internal bool IsConfigurationStarted => _configurationState is not ConfigurationState.NotConfigured;
        private volatile ConfigurationState _configurationState;
        private enum ConfigurationState : byte
        {
            NotConfigured = 0,
            Configuring = 1,
            Configured = 2
        };

        private ExceptionDispatchInfo? _cachedConfigureError;

        internal void EnsureConfigured()
        {
            if (!IsConfigured)
            {
                ConfigureSynchronized();
            }

            void ConfigureSynchronized()
            {
                Options.MakeReadOnly();
                MakeReadOnly();

                _cachedConfigureError?.Throw();

                lock (Options.CacheContext)
                {
                    if (_configurationState != ConfigurationState.NotConfigured)
                    {
                        // The value of _configurationState is either
                        //    'Configuring': recursive instance configured by this thread or
                        //    'Configured' : instance already configured by another thread.
                        // We can safely yield the configuration operation in both cases.
                        return;
                    }

                    _cachedConfigureError?.Throw();

                    try
                    {
                        _configurationState = ConfigurationState.Configuring;
                        Configure();
                        _configurationState = ConfigurationState.Configured;
                    }
                    catch (Exception e)
                    {
                        _cachedConfigureError = ExceptionDispatchInfo.Capture(e);
                        _configurationState = ConfigurationState.NotConfigured;
                        throw;
                    }
                }
            }
        }

        private void Configure()
        {
            Debug.Assert(Monitor.IsEntered(Options.CacheContext), "Configure called directly, use EnsureConfigured which synchronizes access to this method");
            Debug.Assert(Options.IsReadOnly);
            Debug.Assert(IsReadOnly);

            PropertyInfoForTypeInfo.Configure();

            if (PolymorphismOptions != null)
            {
                // This needs to be done before ConfigureProperties() is called
                // KdlPropertyInfo.Configure() must have this value available in order to detect Polymoprhic + cyclic class case
                PolymorphicTypeResolver = new PolymorphicTypeResolver(Options, PolymorphismOptions, Type, Converter.CanHaveMetadata);
            }

            if (Kind == KdlTypeInfoKind.Object)
            {
                ConfigureProperties();

                if (DetermineUsesParameterizedConstructor())
                {
                    ConfigureConstructorParameters();
                }
            }

            if (ElementType != null)
            {
                _elementTypeInfo ??= Options.GetTypeInfoInternal(ElementType);
                _elementTypeInfo.EnsureConfigured();
            }

            if (KeyType != null)
            {
                _keyTypeInfo ??= Options.GetTypeInfoInternal(KeyType);
                _keyTypeInfo.EnsureConfigured();
            }

            DetermineIsCompatibleWithCurrentOptions();
            CanUseSerializeHandler = HasSerializeHandler && IsCompatibleWithCurrentOptions;
        }

        /// <summary>
        /// Gets any ancestor polymorphic types that declare
        /// a type discriminator for the current type. Consulted
        /// when serializing polymorphic values as objects.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal KdlTypeInfo? AncestorPolymorphicType
        {
            get
            {
                Debug.Assert(IsConfigured);
                Debug.Assert(Type != typeof(object));

                if (!_isAncestorPolymorphicTypeResolved)
                {
                    _ancestorPolymorhicType = PolymorphicTypeResolver.FindNearestPolymorphicBaseType(this);
                    _isAncestorPolymorphicTypeResolved = true;
                }

                return _ancestorPolymorhicType;
            }
        }

        private KdlTypeInfo? _ancestorPolymorhicType;
        private volatile bool _isAncestorPolymorphicTypeResolved;

        /// <summary>
        /// Determines if the transitive closure of all KdlTypeInfo metadata referenced
        /// by the current type (property types, key types, element types, ...) are
        /// compatible with the settings as specified in KdlSerializerOptions.
        /// </summary>
        private void DetermineIsCompatibleWithCurrentOptions()
        {
            // Defines a recursive algorithm validating that the `IsCurrentNodeCompatible`
            // predicate is valid for every node in the type graph. This method only checks
            // the immediate children, with recursion being driven by the Configure() method.
            // Therefore, this method must be called _after_ the child nodes have been configured.

            Debug.Assert(IsReadOnly);
            Debug.Assert(!IsConfigured);

            if (!IsCurrentNodeCompatible())
            {
                IsCompatibleWithCurrentOptions = false;
                return;
            }

            if (_properties != null)
            {
                foreach (KdlPropertyInfo property in _properties)
                {
                    Debug.Assert(property.IsConfigured);

                    if (!property.IsPropertyTypeInfoConfigured)
                    {
                        // Either an ignored property or property is part of a cycle.
                        // In both cases we can ignore these instances.
                        continue;
                    }

                    if (!property.KdlTypeInfo.IsCompatibleWithCurrentOptions)
                    {
                        IsCompatibleWithCurrentOptions = false;
                        return;
                    }
                }
            }

            if (_elementTypeInfo?.IsCompatibleWithCurrentOptions == false ||
                _keyTypeInfo?.IsCompatibleWithCurrentOptions == false)
            {
                IsCompatibleWithCurrentOptions = false;
                return;
            }

            Debug.Assert(IsCompatibleWithCurrentOptions);

            // Defines the core predicate that must be checked for every node in the type graph.
            bool IsCurrentNodeCompatible()
            {
                if (IsCustomized)
                {
                    // Return false if we have detected contract customization by the user.
                    return false;
                }

                if (Options.CanUseFastPathSerializationLogic)
                {
                    // Simple case/backward compatibility: options uses a combination of compatible built-in converters.
                    return true;
                }

                return OriginatingResolver.IsCompatibleWithOptions(Options);
            }
        }

        /// <summary>
        /// Holds the result of the above algorithm -- NB must default to true
        /// to establish a base case for recursive types and any KdlIgnored property types.
        /// </summary>
        private bool IsCompatibleWithCurrentOptions { get; set; } = true;

        /// <summary>
        /// Determine if the current configuration is compatible with using a parameterized constructor.
        /// </summary>
        internal bool DetermineUsesParameterizedConstructor()
            => Converter.ConstructorIsParameterized && CreateObject is null;

        /// <summary>
        /// Creates a blank <see cref="KdlTypeInfo{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type for which contract metadata is specified.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> instance the metadata is associated with.</param>
        /// <returns>A blank <see cref="KdlTypeInfo{T}"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <remarks>
        /// The returned <see cref="KdlTypeInfo{T}"/> will be blank, with the exception of the
        /// <see cref="Converter"/> property which will be resolved either from
        /// <see cref="KdlSerializerOptions.Converters"/> or the built-in converters for the type.
        /// Any converters specified via <see cref="KdlConverterAttribute"/> on the type declaration
        /// will not be resolved by this method.
        ///
        /// What converter does get resolved influences the value of <see cref="Kind"/>,
        /// which constrains the type of metadata that can be modified in the <see cref="KdlTypeInfo"/> instance.
        /// </remarks>
        [RequiresUnreferencedCode(MetadataFactoryRequiresUnreferencedCode)]
        [RequiresDynamicCode(MetadataFactoryRequiresUnreferencedCode)]
        public static KdlTypeInfo<T> CreateKdlTypeInfo<T>(KdlSerializerOptions options)
        {
            if (options == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            KdlConverter converter = DefaultKdlTypeInfoResolver.GetConverterForType(typeof(T), options, resolveKdlConverterAttribute: false);
            return new KdlTypeInfo<T>(converter, options);
        }

        /// <summary>
        /// Creates a blank <see cref="KdlTypeInfo"/> instance.
        /// </summary>
        /// <param name="type">The type for which contract metadata is specified.</param>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> instance the metadata is associated with.</param>
        /// <returns>A blank <see cref="KdlTypeInfo"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> cannot be used for serialization.</exception>
        /// <remarks>
        /// The returned <see cref="KdlTypeInfo"/> will be blank, with the exception of the
        /// <see cref="Converter"/> property which will be resolved either from
        /// <see cref="KdlSerializerOptions.Converters"/> or the built-in converters for the type.
        /// Any converters specified via <see cref="KdlConverterAttribute"/> on the type declaration
        /// will not be resolved by this method.
        ///
        /// What converter does get resolved influences the value of <see cref="Kind"/>,
        /// which constrains the type of metadata that can be modified in the <see cref="KdlTypeInfo"/> instance.
        /// </remarks>
        [RequiresUnreferencedCode(MetadataFactoryRequiresUnreferencedCode)]
        [RequiresDynamicCode(MetadataFactoryRequiresUnreferencedCode)]
        public static KdlTypeInfo CreateKdlTypeInfo(Type type, KdlSerializerOptions options)
        {
            if (type == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(type));
            }

            if (options == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            if (IsInvalidForSerialization(type))
            {
                ThrowHelper.ThrowArgumentException_CannotSerializeInvalidType(nameof(type), type, null, null);
            }

            KdlConverter converter = DefaultKdlTypeInfoResolver.GetConverterForType(type, options, resolveKdlConverterAttribute: false);
            return CreateKdlTypeInfo(type, converter, options);
        }

        [RequiresUnreferencedCode(MetadataFactoryRequiresUnreferencedCode)]
        [RequiresDynamicCode(MetadataFactoryRequiresUnreferencedCode)]
        internal static KdlTypeInfo CreateKdlTypeInfo(Type type, KdlConverter converter, KdlSerializerOptions options)
        {
            KdlTypeInfo jsonTypeInfo;

            if (converter.Type == type)
            {
                // For performance, avoid doing a reflection-based instantiation
                // if the converter type matches that of the declared type.
                jsonTypeInfo = converter.CreateKdlTypeInfo(options);
            }
            else
            {
                Type jsonTypeInfoType = typeof(KdlTypeInfo<>).MakeGenericType(type);
                jsonTypeInfo = (KdlTypeInfo)jsonTypeInfoType.CreateInstanceNoWrapExceptions(
                    parameterTypes: [typeof(KdlConverter), typeof(KdlSerializerOptions)],
                    parameters: [converter, options])!;
            }

            Debug.Assert(jsonTypeInfo.Type == type);
            return jsonTypeInfo;
        }

        /// <summary>
        /// Creates a blank <see cref="KdlPropertyInfo"/> instance for the current <see cref="KdlTypeInfo"/>.
        /// </summary>
        /// <param name="propertyType">The declared type for the property.</param>
        /// <param name="name">The property name used in KDL serialization and deserialization.</param>
        /// <returns>A blank <see cref="KdlPropertyInfo"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyType"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyType"/> cannot be used for serialization.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="KdlTypeInfo"/> instance has been locked for further modification.</exception>
        [RequiresUnreferencedCode(MetadataFactoryRequiresUnreferencedCode)]
        [RequiresDynamicCode(MetadataFactoryRequiresUnreferencedCode)]
        public KdlPropertyInfo CreateKdlPropertyInfo(Type propertyType, string name)
        {
            if (propertyType == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyType));
            }

            if (name == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(name));
            }

            if (IsInvalidForSerialization(propertyType))
            {
                ThrowHelper.ThrowArgumentException_CannotSerializeInvalidType(nameof(propertyType), propertyType, Type, name);
            }

            VerifyMutable();
            KdlPropertyInfo propertyInfo = CreatePropertyUsingReflection(propertyType, declaringType: null);
            propertyInfo.Name = name;

            return propertyInfo;
        }

        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        internal KdlPropertyInfo CreatePropertyUsingReflection(Type propertyType, Type? declaringType)
        {
            KdlPropertyInfo jsonPropertyInfo;

            if (Options.TryGetTypeInfoCached(propertyType, out KdlTypeInfo? jsonTypeInfo))
            {
                // If a KdlTypeInfo has already been cached for the property type,
                // avoid reflection-based initialization by delegating construction
                // of KdlPropertyInfo<T> construction to the property type metadata.
                jsonPropertyInfo = jsonTypeInfo.CreateKdlPropertyInfo(declaringTypeInfo: this, declaringType, Options);
            }
            else
            {
                // Metadata for `propertyType` has not been registered yet.
                // Use reflection to instantiate the correct KdlPropertyInfo<T>
                Type propertyInfoType = typeof(KdlPropertyInfo<>).MakeGenericType(propertyType);
                jsonPropertyInfo = (KdlPropertyInfo)propertyInfoType.CreateInstanceNoWrapExceptions(
                    parameterTypes: [typeof(Type), typeof(KdlTypeInfo), typeof(KdlSerializerOptions)],
                    parameters: [declaringType ?? Type, this, Options])!;
            }

            Debug.Assert(jsonPropertyInfo.PropertyType == propertyType);
            return jsonPropertyInfo;
        }

        /// <summary>
        /// Creates a KdlPropertyInfo whose property type matches the type of this KdlTypeInfo instance.
        /// </summary>
        private protected abstract KdlPropertyInfo CreateKdlPropertyInfo(KdlTypeInfo declaringTypeInfo, Type? declaringType, KdlSerializerOptions options);

        private protected Dictionary<ParameterLookupKey, KdlParameterInfoValues>? _parameterInfoValuesIndex;

        // Untyped, root-level serialization methods
        internal abstract void SerializeAsObject(KdlWriter writer, object? rootValue);
        internal abstract Task SerializeAsObjectAsync(PipeWriter pipeWriter, object? rootValue, int flushThreshold, CancellationToken cancellationToken);
        internal abstract Task SerializeAsObjectAsync(Stream utf8Kdl, object? rootValue, CancellationToken cancellationToken);
        internal abstract Task SerializeAsObjectAsync(PipeWriter utf8Kdl, object? rootValue, CancellationToken cancellationToken);
        internal abstract void SerializeAsObject(Stream utf8Kdl, object? rootValue);

        // Untyped, root-level deserialization methods
        internal abstract object? DeserializeAsObject(ref KdlReader reader, ref ReadStack state);
        internal abstract ValueTask<object?> DeserializeAsObjectAsync(Stream utf8Kdl, CancellationToken cancellationToken);
        internal abstract object? DeserializeAsObject(Stream utf8Kdl);

        internal ref struct PropertyHierarchyResolutionState(KdlSerializerOptions options)
        {
            public Dictionary<string, (KdlPropertyInfo, int index)> AddedProperties = new(options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            public Dictionary<string, KdlPropertyInfo>? IgnoredProperties;
            public bool IsPropertyOrderSpecified;
        }

        private protected readonly struct ParameterLookupKey(Type type, string name) : IEquatable<ParameterLookupKey>
        {
            public Type Type { get; } = type;
            public string Name { get; } = name;
            public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            public bool Equals(ParameterLookupKey other) => Type == other.Type && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            public override bool Equals([NotNullWhen(true)] object? obj) => obj is ParameterLookupKey key && Equals(key);
        }

        internal void ConfigureProperties()
        {
            Debug.Assert(Kind == KdlTypeInfoKind.Object);
            Debug.Assert(_propertyCache is null);
            Debug.Assert(_propertyIndex is null);
            Debug.Assert(ExtensionDataProperty is null);

            KdlPropertyInfoList properties = PropertyList;
            StringComparer comparer = Options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            Dictionary<string, KdlPropertyInfo> propertyIndex = new(properties.Count, comparer);
            List<KdlPropertyInfo> propertyCache = new(properties.Count);

            int numberOfRequiredProperties = 0;
            bool arePropertiesSorted = true;
            int previousPropertyOrder = int.MinValue;

            foreach (KdlPropertyInfo property in properties)
            {
                Debug.Assert(property.DeclaringTypeInfo == this);

                if (property.IsExtensionData)
                {
                    if (UnmappedMemberHandling is KdlUnmappedMemberHandling.Disallow)
                    {
                        ThrowHelper.ThrowInvalidOperationException_ExtensionDataConflictsWithUnmappedMemberHandling(Type, property);
                    }

                    if (ExtensionDataProperty != null)
                    {
                        ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(Type, typeof(KdlExtensionDataAttribute));
                    }

                    ExtensionDataProperty = property;
                }
                else
                {
                    if (property.IsRequired)
                    {
                        property.RequiredPropertyIndex = numberOfRequiredProperties++;
                    }

                    if (arePropertiesSorted)
                    {
                        arePropertiesSorted = previousPropertyOrder <= property.Order;
                        previousPropertyOrder = property.Order;
                    }

                    if (!propertyIndex.TryAdd(property.Name, property))
                    {
                        ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameConflict(Type, property.Name);
                    }

                    propertyCache.Add(property);
                }

                property.Configure();
            }

            if (!arePropertiesSorted)
            {
                // Properties have been configured by the user and require sorting.
                properties.SortProperties();
                propertyCache.StableSortByKey(static propInfo => propInfo.Order);
            }

            NumberOfRequiredProperties = numberOfRequiredProperties;
            _propertyCache = [.. propertyCache];
            _propertyIndex = propertyIndex;

            // Override global UnmappedMemberHandling configuration
            // if type specifies an extension data property.
            EffectiveUnmappedMemberHandling = UnmappedMemberHandling ??
                (ExtensionDataProperty is null
                    ? Options.UnmappedMemberHandling
                    : KdlUnmappedMemberHandling.Skip);
        }

        internal void PopulateParameterInfoValues(KdlParameterInfoValues[] parameterInfoValues)
        {
            if (parameterInfoValues.Length == 0)
            {
                return;
            }

            Dictionary<ParameterLookupKey, KdlParameterInfoValues> parameterIndex = new(parameterInfoValues.Length);
            foreach (KdlParameterInfoValues parameterInfoValue in parameterInfoValues)
            {
                ParameterLookupKey paramKey = new(parameterInfoValue.ParameterType, parameterInfoValue.Name);
                parameterIndex.TryAdd(paramKey, parameterInfoValue); // Ignore conflicts since they are reported at serialization time.
            }

            ParameterCount = parameterInfoValues.Length;
            _parameterInfoValuesIndex = parameterIndex;
        }

        internal void ResolveMatchingParameterInfo(KdlPropertyInfo propertyInfo)
        {
            Debug.Assert(
                CreateObjectWithArgs is null || _parameterInfoValuesIndex is not null,
                "Metadata with parameterized constructors must have populated parameter info metadata.");

            if (_parameterInfoValuesIndex is not { } index)
            {
                return;
            }

            string propertyName = propertyInfo.MemberName ?? propertyInfo.Name;
            ParameterLookupKey propKey = new(propertyInfo.PropertyType, propertyName);
            if (index.TryGetValue(propKey, out KdlParameterInfoValues? matchingParameterInfoValues))
            {
                propertyInfo.AddKdlParameterInfo(matchingParameterInfoValues);
            }
        }

        internal void ConfigureConstructorParameters()
        {
            Debug.Assert(Kind == KdlTypeInfoKind.Object);
            Debug.Assert(DetermineUsesParameterizedConstructor());
            Debug.Assert(_propertyCache is not null);
            Debug.Assert(_parameterCache is null);

            List<KdlParameterInfo> parameterCache = new(ParameterCount);
            Dictionary<ParameterLookupKey, KdlParameterInfo> parameterIndex = new(ParameterCount);

            foreach (KdlPropertyInfo propertyInfo in _propertyCache)
            {
                KdlParameterInfo? parameterInfo = propertyInfo.AssociatedParameter;
                if (parameterInfo is null)
                {
                    continue;
                }

                string propertyName = propertyInfo.MemberName ?? propertyInfo.Name;
                ParameterLookupKey paramKey = new(propertyInfo.PropertyType, propertyName);
                if (!parameterIndex.TryAdd(paramKey, parameterInfo))
                {
                    // Multiple object properties cannot bind to the same constructor parameter.
                    ThrowHelper.ThrowInvalidOperationException_MultiplePropertiesBindToConstructorParameters(
                        Type,
                        parameterInfo.Name,
                        propertyInfo.Name,
                        parameterIndex[paramKey].MatchingProperty.Name);
                }

                parameterCache.Add(parameterInfo);
            }

            if (ExtensionDataProperty is { AssociatedParameter: not null })
            {
                Debug.Assert(ExtensionDataProperty.MemberName != null, "Custom property info cannot be data extension property");
                ThrowHelper.ThrowInvalidOperationException_ExtensionDataCannotBindToCtorParam(ExtensionDataProperty.MemberName, ExtensionDataProperty);
            }

            _parameterCache = [.. parameterCache];
            _parameterInfoValuesIndex = null;
        }

        internal static void ValidateType(Type type)
        {
            if (IsInvalidForSerialization(type))
            {
                ThrowHelper.ThrowInvalidOperationException_CannotSerializeInvalidType(type, declaringType: null, memberInfo: null);
            }
        }

        internal static bool IsInvalidForSerialization(Type type)
        {
            return type == typeof(void) || type.IsPointer || type.IsByRef || IsByRefLike(type) || type.ContainsGenericParameters;
        }

        internal void PopulatePolymorphismMetadata()
        {
            Debug.Assert(!IsReadOnly);

            KdlPolymorphismOptions? options = KdlPolymorphismOptions.CreateFromAttributeDeclarations(Type);
            if (options != null)
            {
                options.DeclaringTypeInfo = this;
                _polymorphismOptions = options;
            }
        }

        internal void MapInterfaceTypesToCallbacks()
        {
            Debug.Assert(!IsReadOnly);

            if (Kind is KdlTypeInfoKind.Object or KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary)
            {
                if (typeof(IKdlOnSerializing).IsAssignableFrom(Type))
                {
                    OnSerializing = static obj => ((IKdlOnSerializing)obj).OnSerializing();
                }

                if (typeof(IKdlOnSerialized).IsAssignableFrom(Type))
                {
                    OnSerialized = static obj => ((IKdlOnSerialized)obj).OnSerialized();
                }

                if (typeof(IKdlOnDeserializing).IsAssignableFrom(Type))
                {
                    OnDeserializing = static obj => ((IKdlOnDeserializing)obj).OnDeserializing();
                }

                if (typeof(IKdlOnDeserialized).IsAssignableFrom(Type))
                {
                    OnDeserialized = static obj => ((IKdlOnDeserialized)obj).OnDeserialized();
                }
            }
        }

        internal void SetCreateObjectIfCompatible(Delegate? createObject)
        {
            Debug.Assert(!IsReadOnly);

            // Guard against the reflection resolver/source generator attempting to pass
            // a CreateObject delegate to converters/metadata that do not support it.
            if (Converter.SupportsCreateObjectDelegate && !Converter.ConstructorIsParameterized)
            {
                SetCreateObject(createObject);
            }
        }

        private static bool IsByRefLike(Type type)
        {
#if NET
            return type.IsByRefLike;
#else
            if (!type.IsValueType)
            {
                return false;
            }

            object[] attributes = type.GetCustomAttributes(inherit: false);

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType().FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute")
                {
                    return true;
                }
            }

            return false;
#endif
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool SupportsPolymorphicDeserialization
        {
            get
            {
                Debug.Assert(IsConfigurationStarted);
                return PolymorphicTypeResolver?.UsesTypeDiscriminators == true;
            }
        }

        internal static bool IsValidExtensionDataProperty(Type propertyType)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(propertyType) ||
                typeof(IDictionary<string, KdlReadOnlyElement>).IsAssignableFrom(propertyType) ||
                // Avoid a reference to typeof(KdlVertex) to support trimming.
                (propertyType.FullName == KdlObjectTypeName && ReferenceEquals(propertyType.Assembly, typeof(KdlTypeInfo).Assembly));
        }

        private static KdlTypeInfoKind GetTypeInfoKind(Type type, KdlConverter converter)
        {
            if (type == typeof(object) && converter.CanBePolymorphic)
            {
                // System.Object is polymorphic and will not respect Properties
                Debug.Assert(converter is ObjectConverter);
                return KdlTypeInfoKind.None;
            }

            switch (converter.ConverterStrategy)
            {
                case ConverterStrategy.Value:
                    return KdlTypeInfoKind.None;
                case ConverterStrategy.Object:
                    return KdlTypeInfoKind.Object;
                case ConverterStrategy.Enumerable:
                    return KdlTypeInfoKind.Enumerable;
                case ConverterStrategy.Dictionary:
                    return KdlTypeInfoKind.Dictionary;
                case ConverterStrategy.None:
                    Debug.Assert(converter is KdlConverterFactory);
                    ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
                    return default;
                default:
                    Debug.Fail($"Unexpected class type: {converter.ConverterStrategy}");
                    throw new InvalidOperationException();
            }
        }

        internal sealed class KdlPropertyInfoList(KdlTypeInfo jsonTypeInfo) : ConfigurationList<KdlPropertyInfo>
        {
            private readonly KdlTypeInfo _jsonTypeInfo = jsonTypeInfo;

            public override bool IsReadOnly => (_jsonTypeInfo._properties == this && _jsonTypeInfo.IsReadOnly) || _jsonTypeInfo.Kind != KdlTypeInfoKind.Object;
            protected override void OnCollectionModifying()
            {
                if (_jsonTypeInfo._properties == this)
                {
                    _jsonTypeInfo.VerifyMutable();
                }

                if (_jsonTypeInfo.Kind != KdlTypeInfoKind.Object)
                {
                    ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(_jsonTypeInfo.Kind);
                }
            }

            protected override void ValidateAddedValue(KdlPropertyInfo item)
            {
                item.EnsureChildOf(_jsonTypeInfo);
            }

            public void SortProperties()
            {
                _list.StableSortByKey(static propInfo => propInfo.Order);
            }

            /// <summary>
            /// Used by the built-in resolvers to add property metadata applying conflict resolution.
            /// </summary>
            public void AddPropertyWithConflictResolution(KdlPropertyInfo jsonPropertyInfo, ref PropertyHierarchyResolutionState state)
            {
                Debug.Assert(!_jsonTypeInfo.IsConfigured);
                Debug.Assert(jsonPropertyInfo.MemberName != null, "MemberName can be null in custom KdlPropertyInfo instances and should never be passed in this method");

                // Algorithm should be kept in sync with the Roslyn equivalent in KdlSourceGenerator.Parser.cs
                string memberName = jsonPropertyInfo.MemberName;

                if (state.AddedProperties.TryAdd(jsonPropertyInfo.Name, (jsonPropertyInfo, Count)))
                {
                    Add(jsonPropertyInfo);
                    state.IsPropertyOrderSpecified |= jsonPropertyInfo.Order != 0;
                }
                else
                {
                    // The KdlPropertyNameAttribute or naming policy resulted in a collision.
                    (KdlPropertyInfo other, int index) = state.AddedProperties[jsonPropertyInfo.Name];

                    if (other.IsIgnored)
                    {
                        // Overwrite previously cached property since it has [KdlIgnore].
                        state.AddedProperties[jsonPropertyInfo.Name] = (jsonPropertyInfo, index);
                        this[index] = jsonPropertyInfo;
                        state.IsPropertyOrderSpecified |= jsonPropertyInfo.Order != 0;
                    }
                    else
                    {
                        bool ignoreCurrentProperty =
                            // Does the current property have `KdlIgnoreAttribute`?
                            jsonPropertyInfo.IsIgnored ||
                            // Is the current property hidden by the previously cached property
                            // (with `new` keyword, or by overriding)?
                            jsonPropertyInfo.IsOverriddenOrShadowedBy(other) ||
                            // Was a property with the same CLR name ignored? That property hid the current property,
                            // thus, if it was ignored, the current property should be ignored too.
                            (state.IgnoredProperties?.TryGetValue(memberName, out KdlPropertyInfo? ignored) == true && jsonPropertyInfo.IsOverriddenOrShadowedBy(ignored));

                        if (!ignoreCurrentProperty)
                        {
                            ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameConflict(_jsonTypeInfo.Type, jsonPropertyInfo.Name);
                        }
                    }
                }

                if (jsonPropertyInfo.IsIgnored)
                {
                    (state.IgnoredProperties ??= [])[memberName] = jsonPropertyInfo;
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Type = {Type.Name}, Kind = {Kind}";
    }
}
