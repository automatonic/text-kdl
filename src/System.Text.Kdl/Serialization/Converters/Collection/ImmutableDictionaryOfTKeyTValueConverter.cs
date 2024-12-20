using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    internal class ImmutableDictionaryOfTKeyTValueConverter<TDictionary, TKey, TValue>
        : DictionaryDefaultConverter<TDictionary, TKey, TValue>
        where TDictionary : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        protected sealed override void Add(TKey key, in TValue value, KdlSerializerOptions options, ref ReadStack state)
        {
            ((Dictionary<TKey, TValue>)state.Current.ReturnValue!)[key] = value;
        }

        internal sealed override bool CanHaveMetadata => false;

        internal override bool SupportsCreateObjectDelegate => false;
        protected sealed override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state)
        {
            state.Current.ReturnValue = new Dictionary<TKey, TValue>();
        }

        internal sealed override bool IsConvertibleCollection => true;
        protected sealed override void ConvertCollection(ref ReadStack state, KdlSerializerOptions options)
        {
            Func<IEnumerable<KeyValuePair<TKey, TValue>>, TDictionary>? creator =
                (Func<IEnumerable<KeyValuePair<TKey, TValue>>, TDictionary>?)state.Current.KdlTypeInfo.CreateObjectWithArgs;
            Debug.Assert(creator != null);
            state.Current.ReturnValue = creator((Dictionary<TKey, TValue>)state.Current.ReturnValue!);
        }
    }
}
