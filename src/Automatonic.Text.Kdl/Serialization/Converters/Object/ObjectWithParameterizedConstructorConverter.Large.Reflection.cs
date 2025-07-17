using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Implementation of <cref>KdlObjectConverter{T}</cref> that supports the deserialization
    /// of KDL objects using parameterized constructors.
    /// </summary>
    [method: RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
    [method: RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    /// <summary>
    /// Implementation of <cref>KdlObjectConverter{T}</cref> that supports the deserialization
    /// of KDL objects using parameterized constructors.
    /// </summary>
    internal sealed class LargeObjectWithParameterizedConstructorConverterWithReflection<T>()
        : LargeObjectWithParameterizedConstructorConverter<T>
        where T : notnull
    {
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(
            KdlTypeInfo kdlTypeInfo,
            KdlSerializerOptions options
        )
        {
            kdlTypeInfo.CreateObjectWithArgs =
                DefaultKdlTypeInfoResolver.MemberAccessor.CreateParameterizedConstructor<T>(
                    ConstructorInfo!
                );
        }
    }
}
