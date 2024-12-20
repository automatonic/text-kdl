using System.Collections.Generic;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class IReadOnlyDictionaryOfTKeyTValueConverter<TDictionary, TKey, TValue>
        : DictionaryDefaultConverter<TDictionary, TKey, TValue>
        where TDictionary : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly bool _isDeserializable = typeof(TDictionary).IsAssignableFrom(typeof(Dictionary<TKey, TValue>));

        protected override void Add(TKey key, in TValue value, KdlSerializerOptions options, ref ReadStack state)
        {
            ((Dictionary<TKey, TValue>)state.Current.ReturnValue!)[key] = value;
        }

        internal override bool SupportsCreateObjectDelegate => false;
        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state)
        {
            if (!_isDeserializable)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(Type, ref reader, ref state);
            }

            state.Current.ReturnValue = new Dictionary<TKey, TValue>();
        }
    }
}
