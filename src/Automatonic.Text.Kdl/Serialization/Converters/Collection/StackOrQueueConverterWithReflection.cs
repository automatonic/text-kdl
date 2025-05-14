using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    [method: RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
    [method: RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class StackOrQueueConverterWithReflection<TCollection>()
        : StackOrQueueConverter<TCollection>
        where TCollection : IEnumerable
    {
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo kdlTypeInfo, KdlSerializerOptions options)
        {
            kdlTypeInfo.AddMethodDelegate = DefaultKdlTypeInfoResolver.MemberAccessor.CreateAddMethodDelegate<TCollection>();
        }
    }
}
