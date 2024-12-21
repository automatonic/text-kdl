using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    [method: RequiresUnreferencedCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
    [method: RequiresDynamicCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
    internal sealed class ImmutableDictionaryOfTKeyTValueConverterWithReflection<TCollection, TKey, TValue>()
        : ImmutableDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>
        where TCollection : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        [RequiresUnreferencedCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
        [RequiresDynamicCode(IEnumerableConverterFactoryHelpers.ImmutableConvertersUnreferencedCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            jsonTypeInfo.CreateObjectWithArgs = DefaultKdlTypeInfoResolver.MemberAccessor.CreateImmutableDictionaryCreateRangeDelegate<TCollection, TKey, TValue>();
        }
    }
}
