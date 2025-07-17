using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Default base class implementation of <cref>KdlDictionaryConverter{TCollection}</cref> .
    /// </summary>
    internal abstract class DictionaryDefaultConverter<TDictionary, TKey, TValue>
        : KdlDictionaryConverter<TDictionary, TKey, TValue>
        where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        internal override bool CanHaveMetadata => true;

        protected internal override bool OnWriteResume(
            KdlWriter writer,
            TDictionary value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            IEnumerator<KeyValuePair<TKey, TValue>> enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                state.Current.CollectionEnumerator = enumerator;
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    return true;
                }
            }
            else
            {
                enumerator =
                    (IEnumerator<KeyValuePair<TKey, TValue>>)state.Current.CollectionEnumerator;
            }

            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;
            _keyConverter ??= GetConverter<TKey>(typeInfo.KeyTypeInfo!);
            _valueConverter ??= GetConverter<TValue>(typeInfo.ElementTypeInfo!);

            do
            {
                if (ShouldFlush(ref state, writer))
                {
                    return false;
                }

                if (state.Current.PropertyState < StackFramePropertyState.Name)
                {
                    state.Current.PropertyState = StackFramePropertyState.Name;
                    TKey key = enumerator.Current.Key;
                    _keyConverter.WriteAsPropertyNameCore(
                        writer,
                        key,
                        options,
                        state.Current.IsWritingExtensionDataProperty
                    );
                }

                TValue element = enumerator.Current.Value;
                if (!_valueConverter.TryWrite(writer, element, options, ref state))
                {
                    return false;
                }

                state.Current.EndDictionaryEntry();
            } while (enumerator.MoveNext());

            enumerator.Dispose();
            return true;
        }
    }
}
