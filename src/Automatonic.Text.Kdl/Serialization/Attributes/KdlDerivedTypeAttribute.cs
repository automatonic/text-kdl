namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a type declaration, indicates that the specified subtype should be opted into polymorphic serialization.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface,
        AllowMultiple = true,
        Inherited = false
    )]
    public class KdlDerivedTypeAttribute : KdlAttribute
    {
        /// <summary>
        /// Initializes a new attribute with specified parameters.
        /// </summary>
        /// <param name="derivedType">A derived type that should be supported in polymorphic serialization of the declared based type.</param>
        public KdlDerivedTypeAttribute(Type derivedType) => DerivedType = derivedType;

        /// <summary>
        /// Initializes a new attribute with specified parameters.
        /// </summary>
        /// <param name="derivedType">A derived type that should be supported in polymorphic serialization of the declared base type.</param>
        /// <param name="typeDiscriminator">The type discriminator identifier to be used for the serialization of the subtype.</param>
        public KdlDerivedTypeAttribute(Type derivedType, string typeDiscriminator)
        {
            DerivedType = derivedType;
            TypeDiscriminator = typeDiscriminator;
        }

        /// <summary>
        /// Initializes a new attribute with specified parameters.
        /// </summary>
        /// <param name="derivedType">A derived type that should be supported in polymorphic serialization of the declared base type.</param>
        /// <param name="typeDiscriminator">The type discriminator identifier to be used for the serialization of the subtype.</param>
        public KdlDerivedTypeAttribute(Type derivedType, int typeDiscriminator)
        {
            DerivedType = derivedType;
            TypeDiscriminator = typeDiscriminator;
        }

        /// <summary>
        /// A derived type that should be supported in polymorphic serialization of the declared base type.
        /// </summary>
        public Type DerivedType { get; }

        /// <summary>
        /// The type discriminator identifier to be used for the serialization of the subtype.
        /// </summary>
        public object? TypeDiscriminator { get; }
    }
}
