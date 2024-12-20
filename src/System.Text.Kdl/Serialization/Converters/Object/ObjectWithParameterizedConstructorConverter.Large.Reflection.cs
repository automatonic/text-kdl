using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Implementation of <cref>KdlObjectConverter{T}</cref> that supports the deserialization
    /// of KDL objects using parameterized constructors.
    /// </summary>
    internal sealed class LargeObjectWithParameterizedConstructorConverterWithReflection<T>
        : LargeObjectWithParameterizedConstructorConverter<T> where T : notnull
    {
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        public LargeObjectWithParameterizedConstructorConverterWithReflection()
        {
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            jsonTypeInfo.CreateObjectWithArgs = DefaultKdlTypeInfoResolver.MemberAccessor.CreateParameterizedConstructor<T>(ConstructorInfo!);
        }
    }
}
