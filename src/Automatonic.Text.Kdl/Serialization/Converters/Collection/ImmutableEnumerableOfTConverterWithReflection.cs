using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    [method: RequiresUnreferencedCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
    [method: RequiresDynamicCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
    internal sealed class ImmutableEnumerableOfTConverterWithReflection<TCollection, TElement>()
        : ImmutableEnumerableOfTConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
        [RequiresUnreferencedCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
        [RequiresDynamicCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo kdlTypeInfo, KdlSerializerOptions options)
        {
            kdlTypeInfo.CreateObjectWithArgs = DefaultKdlTypeInfoResolver.MemberAccessor.CreateImmutableEnumerableCreateRangeDelegate<TCollection, TElement>();
        }
    }
}
