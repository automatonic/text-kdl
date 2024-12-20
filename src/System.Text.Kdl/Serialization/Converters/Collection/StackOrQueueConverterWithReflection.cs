using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class StackOrQueueConverterWithReflection<TCollection>
        : StackOrQueueConverter<TCollection>
        where TCollection : IEnumerable
    {
        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        public StackOrQueueConverterWithReflection() { }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            jsonTypeInfo.AddMethodDelegate = DefaultKdlTypeInfoResolver.MemberAccessor.CreateAddMethodDelegate<TCollection>();
        }
    }
}
