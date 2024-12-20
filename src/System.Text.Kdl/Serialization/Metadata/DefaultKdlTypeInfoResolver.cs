using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Defines the default, reflection-based KDL contract resolver used by System.Text.Kdl.
    /// </summary>
    /// <remarks>
    /// The contract resolver used by <see cref="KdlSerializerOptions.Default"/>.
    /// </remarks>
    public partial class DefaultKdlTypeInfoResolver : IKdlTypeInfoResolver, IBuiltInKdlTypeInfoResolver
    {
        private bool _mutable;

        /// <summary>
        /// Creates a mutable <see cref="DefaultKdlTypeInfoResolver"/> instance.
        /// </summary>
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        public DefaultKdlTypeInfoResolver() : this(mutable: true)
        {
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private DefaultKdlTypeInfoResolver(bool mutable)
        {
            _mutable = mutable;
        }

        /// <summary>
        /// Resolves a KDL contract for a given <paramref name="type"/> and <paramref name="options"/> configuration.
        /// </summary>
        /// <param name="type">The type for which to resolve a KDL contract.</param>
        /// <param name="options">A <see cref="KdlSerializerOptions"/> instance used to determine contract configuration.</param>
        /// <returns>A <see cref="KdlTypeInfo"/> defining a reflection-derived KDL contract for <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The base implementation of this method will produce a reflection-derived contract
        /// and apply any callbacks from the <see cref="Modifiers"/> list.
        /// </remarks>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "The ctor is marked RequiresUnreferencedCode.")]
        [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
            Justification = "The ctor is marked RequiresDynamicCode.")]
        public virtual KdlTypeInfo GetTypeInfo(Type type, KdlSerializerOptions options)
        {
            if (type == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(type));
            }

            if (options == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            _mutable = false;

            KdlTypeInfo.ValidateType(type);
            KdlTypeInfo typeInfo = CreateKdlTypeInfo(type, options);
            typeInfo.OriginatingResolver = this;

            // We've finished configuring the metadata, brand the instance as user-unmodified.
            // This should be the last update operation in the resolver to avoid resetting the flag.
            typeInfo.IsCustomized = false;

            if (_modifiers != null)
            {
                foreach (Action<KdlTypeInfo> modifier in _modifiers)
                {
                    modifier(typeInfo);
                }
            }

            return typeInfo;
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        private static KdlTypeInfo CreateKdlTypeInfo(Type type, KdlSerializerOptions options)
        {
            KdlConverter converter = GetConverterForType(type, options);
            return CreateTypeInfoCore(type, converter, options);
        }

        /// <summary>
        /// Gets a list of user-defined callbacks that can be used to modify the initial contract.
        /// </summary>
        /// <remarks>
        /// The modifier list will be rendered immutable after the first <see cref="GetTypeInfo(Type, KdlSerializerOptions)"/> invocation.
        ///
        /// Modifier callbacks are called consecutively in the order in which they are specified in the list.
        /// </remarks>
        public IList<Action<KdlTypeInfo>> Modifiers => _modifiers ??= new ModifierCollection(this);
        private ModifierCollection? _modifiers;

        private sealed class ModifierCollection : ConfigurationList<Action<KdlTypeInfo>>
        {
            private readonly DefaultKdlTypeInfoResolver _resolver;

            public ModifierCollection(DefaultKdlTypeInfoResolver resolver)
            {
                _resolver = resolver;
            }

            public override bool IsReadOnly => !_resolver._mutable;
            protected override void OnCollectionModifying()
            {
                if (!_resolver._mutable)
                {
                    ThrowHelper.ThrowInvalidOperationException_DefaultTypeInfoResolverImmutable();
                }
            }
        }

        bool IBuiltInKdlTypeInfoResolver.IsCompatibleWithOptions(KdlSerializerOptions _)
            // Metadata generated by the default resolver is compatible by definition,
            // provided that no user extensions have been made on the class.
            => _modifiers is null or { Count: 0 } && GetType() == typeof(DefaultKdlTypeInfoResolver);

        internal static DefaultKdlTypeInfoResolver DefaultInstance
        {
            [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
            [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
            get
            {
                if (s_defaultInstance is DefaultKdlTypeInfoResolver result)
                {
                    return result;
                }

                var newInstance = new DefaultKdlTypeInfoResolver(mutable: false);
                return Interlocked.CompareExchange(ref s_defaultInstance, newInstance, comparand: null) ?? newInstance;
            }
        }

        private static DefaultKdlTypeInfoResolver? s_defaultInstance;
    }
}
