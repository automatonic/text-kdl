// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides KDL serialization-related metadata about a type.
    /// </summary>
    /// <typeparam name="T">The generic definition of the type.</typeparam>
    public sealed partial class KdlTypeInfo<T> : KdlTypeInfo
    {
        private Action<KdlWriter, T>? _serialize;

        private Func<T>? _typedCreateObject;

        internal KdlTypeInfo(KdlConverter converter, KdlSerializerOptions options)
            : base(typeof(T), converter, options)
        {
            EffectiveConverter = converter.CreateCastingConverter<T>();
        }

        /// <summary>
        /// A Converter whose declared type always matches that of the current KdlTypeInfo.
        /// It might be the same instance as KdlTypeInfo.Converter or it could be wrapped
        /// in a CastingConverter in cases where a polymorphic converter is being used.
        /// </summary>
        internal KdlConverter<T> EffectiveConverter { get; }

        /// <summary>
        /// Gets or sets a parameterless factory to be used on deserialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        ///
        /// -or-
        ///
        /// A parameterless factory is not supported for the current metadata <see cref="KdlTypeInfo.Kind"/>.
        /// </exception>
        /// <remarks>
        /// If set to <see langword="null" />, any attempt to deserialize instances of the given type will fail at runtime.
        ///
        /// For contracts originating from <see cref="DefaultKdlTypeInfoResolver"/> or <see cref="KdlSerializerContext"/>,
        /// types with a single default constructor or default constructors annotated with <see cref="KdlConstructorAttribute"/>
        /// will be mapped to this delegate.
        /// </remarks>
        public new Func<T>? CreateObject
        {
            get => _typedCreateObject;
            set
            {
                SetCreateObject(value);
            }
        }

        private protected override void SetCreateObject(Delegate? createObject)
        {
            Debug.Assert(createObject is null or Func<object> or Func<T>);

            VerifyMutable();

            if (Kind == KdlTypeInfoKind.None)
            {
                Debug.Assert(_createObject == null);
                Debug.Assert(_typedCreateObject == null);
                ThrowHelper.ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(Kind);
            }

            if (!Converter.SupportsCreateObjectDelegate)
            {
                Debug.Assert(_createObject is null);
                Debug.Assert(_typedCreateObject == null);
                ThrowHelper.ThrowInvalidOperationException_CreateObjectConverterNotCompatible(Type);
            }

            Func<object>? untypedCreateObject;
            Func<T>? typedCreateObject;

            if (createObject is null)
            {
                untypedCreateObject = null;
                typedCreateObject = null;
            }
            else if (createObject is Func<T> typedDelegate)
            {
                typedCreateObject = typedDelegate;
                untypedCreateObject = createObject is Func<object> untypedDelegate ? untypedDelegate : () => typedDelegate()!;
            }
            else
            {
                Debug.Assert(createObject is Func<object>);
                untypedCreateObject = (Func<object>)createObject;
                typedCreateObject = () => (T)untypedCreateObject();
            }

            _createObject = untypedCreateObject;
            _typedCreateObject = typedCreateObject;

            // Clear any data related to the previously specified ctor
            ConstructorAttributeProviderFactory = null;
            ConstructorAttributeProvider = null;

            if (CreateObjectWithArgs is not null)
            {
                _parameterInfoValuesIndex = null;
                CreateObjectWithArgs = null;
                ParameterCount = 0;

                foreach (KdlPropertyInfo propertyInfo in PropertyList)
                {
                    propertyInfo.AssociatedParameter = null;
                }
            }
        }

        /// <summary>
        /// Serializes an instance of <typeparamref name="T"/> using
        /// <see cref="KdlSourceGenerationOptionsAttribute"/> values specified at design time.
        /// </summary>
        /// <remarks>The writer is not flushed after writing.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Action<KdlWriter, T>? SerializeHandler
        {
            get
            {
                return _serialize;
            }
            internal set
            {
                Debug.Assert(!IsReadOnly, "We should not mutate read-only KdlTypeInfo");
                _serialize = value;
                HasSerializeHandler = value != null;
            }
        }

        private protected override KdlPropertyInfo CreatePropertyInfoForTypeInfo()
        {
            return new KdlPropertyInfo<T>(
                declaringType: typeof(T),
                declaringTypeInfo: this,
                Options)
            {
                KdlTypeInfo = this,
                IsForTypeInfo = true,
            };
        }

        private protected override KdlPropertyInfo CreateKdlPropertyInfo(KdlTypeInfo declaringTypeInfo, Type? declaringType, KdlSerializerOptions options)
        {
            return new KdlPropertyInfo<T>(declaringType ?? declaringTypeInfo.Type, declaringTypeInfo, options)
            {
                KdlTypeInfo = this
            };
        }
    }
}
