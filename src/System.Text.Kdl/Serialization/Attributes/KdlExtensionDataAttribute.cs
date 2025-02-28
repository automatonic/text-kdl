namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// When placed on a property or field of type <see cref="System.Text.Kdl.Nodes.KdlNode"/> or
    /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>, any properties that do not have a
    /// matching property or field are added during deserialization and written during serialization.
    /// </summary>
    /// <remarks>
    /// When using <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>, the TKey value must be <see cref="string"/>
    /// and TValue must be <see cref="KdlReadOnlyElement"/> or <see cref="object"/>.
    ///
    /// During deserializing with a <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> extension property with TValue as
    /// <see cref="object"/>, the type of object created will either be a <see cref="System.Text.Kdl.Nodes.KdlElement"/> or a
    /// <see cref="KdlReadOnlyElement"/> depending on the value of <see cref="System.Text.Kdl.KdlSerializerOptions.UnknownTypeHandling"/>.
    ///
    /// If a <see cref="KdlReadOnlyElement"/> is created, a "null" KDL value is treated as a KdlElement with <see cref="KdlReadOnlyElement.ValueKind"/>
    /// set to <see cref="KdlValueKind.Null"/>, otherwise a "null" KDL value is treated as a <c>null</c> object reference.
    ///
    /// During serializing, the name of the extension data member is not included in the KDL;
    /// the data contained within the extension data is serialized as properties of the KDL object.
    ///
    /// If there is more than one extension member on a type, or the member is not of the correct type,
    /// an <see cref="InvalidOperationException"/> is thrown during the first serialization or deserialization of that type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class KdlExtensionDataAttribute : KdlAttribute
    {
    }
}
