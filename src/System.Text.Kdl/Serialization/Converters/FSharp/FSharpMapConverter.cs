using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    // Converter for F# maps: https://fsharp.github.io/fsharp-core-docs/reference/fsharp-collections-fsharpmap-2.html
    [method: RequiresUnreferencedCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
    [method: RequiresDynamicCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]    // Converter for F# maps: https://fsharp.github.io/fsharp-core-docs/reference/fsharp-collections-fsharpmap-2.html
    internal sealed class FSharpMapConverter<TMap, TKey, TValue>() : DictionaryDefaultConverter<TMap, TKey, TValue>
        where TMap : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        private readonly Func<IEnumerable<Tuple<TKey, TValue>>, TMap> _mapConstructor = FSharpCoreReflectionProxy.Instance.CreateFSharpMapConstructor<TMap, TKey, TValue>();

        protected override void Add(TKey key, in TValue value, KdlSerializerOptions options, ref ReadStack state)
        {
            ((List<Tuple<TKey, TValue>>)state.Current.ReturnValue!).Add(new Tuple<TKey, TValue>(key, value));
        }

        internal override bool CanHaveMetadata => false;

        internal override bool SupportsCreateObjectDelegate => false;
        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state)
        {
            state.Current.ReturnValue = new List<Tuple<TKey, TValue>>();
        }

        internal sealed override bool IsConvertibleCollection => true;
        protected override void ConvertCollection(ref ReadStack state, KdlSerializerOptions options)
        {
            state.Current.ReturnValue = _mapConstructor((List<Tuple<TKey, TValue>>)state.Current.ReturnValue!);
        }
    }
}
