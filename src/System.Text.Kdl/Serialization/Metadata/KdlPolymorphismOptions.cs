using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Defines polymorphic configuration for a specified base type.
    /// </summary>
    public class KdlPolymorphismOptions
    {
        private DerivedTypeList? _derivedTypes;
        private bool _ignoreUnrecognizedTypeDiscriminators;
        private KdlUnknownDerivedTypeHandling _unknownDerivedTypeHandling;
        private string? _typeDiscriminatorPropertyName;

        /// <summary>
        /// Creates an empty <see cref="KdlPolymorphismOptions"/> instance.
        /// </summary>
        public KdlPolymorphismOptions()
        {
        }

        /// <summary>
        /// Gets the list of derived types supported in the current polymorphic type configuration.
        /// </summary>
        public IList<KdlDerivedType> DerivedTypes => _derivedTypes ??= new(this);

        /// <summary>
        /// When set to <see langword="true"/>, instructs the serializer to ignore any
        /// unrecognized type discriminator id's and reverts to the contract of the base type.
        /// Otherwise, it will fail the deserialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The parent <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        /// </exception>
        public bool IgnoreUnrecognizedTypeDiscriminators
        {
            get => _ignoreUnrecognizedTypeDiscriminators;
            set
            {
                VerifyMutable();
                _ignoreUnrecognizedTypeDiscriminators = value;
            }
        }

        /// <summary>
        /// Gets or sets the behavior when serializing an undeclared derived runtime type.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The parent <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        /// </exception>
        public KdlUnknownDerivedTypeHandling UnknownDerivedTypeHandling
        {
            get => _unknownDerivedTypeHandling;
            set
            {
                VerifyMutable();
                _unknownDerivedTypeHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets a custom type discriminator property name for the polymorhic type.
        /// Uses the default '$type' property name if left unset.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The parent <see cref="KdlTypeInfo"/> instance has been locked for further modification.
        /// </exception>
        [AllowNull]
        public string TypeDiscriminatorPropertyName
        {
            get => _typeDiscriminatorPropertyName ?? KdlSerializer.TypePropertyName;
            set
            {
                VerifyMutable();
                _typeDiscriminatorPropertyName = value;
            }
        }

        private void VerifyMutable() => DeclaringTypeInfo?.VerifyMutable();

        internal KdlTypeInfo? DeclaringTypeInfo { get; set; }

        private sealed class DerivedTypeList : ConfigurationList<KdlDerivedType>
        {
            private readonly KdlPolymorphismOptions _parent;

            public DerivedTypeList(KdlPolymorphismOptions parent)
            {
                _parent = parent;
            }

            public override bool IsReadOnly => _parent.DeclaringTypeInfo?.IsReadOnly == true;
            protected override void OnCollectionModifying() => _parent.DeclaringTypeInfo?.VerifyMutable();
        }

        internal static KdlPolymorphismOptions? CreateFromAttributeDeclarations(Type baseType)
        {
            KdlPolymorphismOptions? options = null;

            if (baseType.GetCustomAttribute<KdlPolymorphicAttribute>(inherit: false) is KdlPolymorphicAttribute polymorphicAttribute)
            {
                options = new()
                {
                    IgnoreUnrecognizedTypeDiscriminators = polymorphicAttribute.IgnoreUnrecognizedTypeDiscriminators,
                    UnknownDerivedTypeHandling = polymorphicAttribute.UnknownDerivedTypeHandling,
                    TypeDiscriminatorPropertyName = polymorphicAttribute.TypeDiscriminatorPropertyName,
                };
            }

            foreach (KdlDerivedTypeAttribute attr in baseType.GetCustomAttributes<KdlDerivedTypeAttribute>(inherit: false))
            {
                (options ??= new()).DerivedTypes.Add(new KdlDerivedType(attr.DerivedType, attr.TypeDiscriminator));
            }

            return options;
        }
    }
}
