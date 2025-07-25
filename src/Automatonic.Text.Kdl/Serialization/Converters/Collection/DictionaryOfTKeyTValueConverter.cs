using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for Dictionary{string, TValue} that (de)serializes as a KDL object with properties
    /// representing the dictionary element key and value.
    /// </summary>
    internal sealed class DictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>
        : DictionaryDefaultConverter<TCollection, TKey, TValue>
        where TCollection : Dictionary<TKey, TValue>
        where TKey : notnull
    {
        internal override bool CanPopulate => true;

        protected override void Add(
            TKey key,
            in TValue value,
            KdlSerializerOptions options,
            ref ReadStack state
        )
        {
            ((TCollection)state.Current.ReturnValue!)[key] = value;
        }

        protected internal override bool OnWriteResume(
            KdlWriter writer,
            TCollection value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            Dictionary<TKey, TValue>.Enumerator enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    return true;
                }
            }
            else
            {
                enumerator = (Dictionary<TKey, TValue>.Enumerator)
                    state.Current.CollectionEnumerator;
            }

            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;
            _keyConverter ??= GetConverter<TKey>(typeInfo.KeyTypeInfo!);
            _valueConverter ??= GetConverter<TValue>(typeInfo.ElementTypeInfo!);

            if (
                !state.SupportContinuation
                && _valueConverter.CanUseDirectReadOrWrite
                && state.Current.NumberHandling == null
            )
            {
                // Fast path that avoids validation and extra indirection.
                do
                {
                    TKey key = enumerator.Current.Key;
                    _keyConverter.WriteAsPropertyNameCore(
                        writer,
                        key,
                        options,
                        state.Current.IsWritingExtensionDataProperty
                    );
                    _valueConverter.Write(writer, enumerator.Current.Value, options);
                } while (enumerator.MoveNext());
            }
            else
            {
                do
                {
                    if (ShouldFlush(ref state, writer))
                    {
                        state.Current.CollectionEnumerator = enumerator;
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
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    state.Current.EndDictionaryEntry();
                } while (enumerator.MoveNext());
            }

            enumerator.Dispose();
            return true;
        }
    }
}
