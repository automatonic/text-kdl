using System.Collections;
using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.IDictionary</cref> that (de)serializes as a KDL object with properties
    /// representing the dictionary element key and value.
    /// </summary>
    internal sealed class IDictionaryConverter<TDictionary>
        : KdlDictionaryConverter<TDictionary, string, object?>
        where TDictionary : IDictionary
    {
        internal override bool CanPopulate => true;

        protected override void Add(
            string key,
            in object? value,
            KdlSerializerOptions options,
            ref ReadStack state
        )
        {
            TDictionary collection = (TDictionary)state.Current.ReturnValue!;
            collection[key] = value;
            if (IsValueType)
            {
                state.Current.ReturnValue = collection;
            }
        }

        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state)
        {
            base.CreateCollection(ref reader, ref state);
            TDictionary returnValue = (TDictionary)state.Current.ReturnValue!;
            if (returnValue.IsReadOnly)
            {
                state.Current.ReturnValue = null; // clear out for more accurate KdlPath reporting.
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(
                    Type,
                    ref reader,
                    ref state
                );
            }
        }

        protected internal override bool OnWriteResume(
            KdlWriter writer,
            TDictionary value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            IDictionaryEnumerator enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                state.Current.CollectionEnumerator = enumerator;
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = (IDictionaryEnumerator)state.Current.CollectionEnumerator;
            }

            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;
            _valueConverter ??= GetConverter<object?>(typeInfo.ElementTypeInfo!);

            do
            {
                if (ShouldFlush(ref state, writer))
                {
                    return false;
                }

                if (state.Current.PropertyState < StackFramePropertyState.Name)
                {
                    state.Current.PropertyState = StackFramePropertyState.Name;
                    object key = enumerator.Key;
                    // Optimize for string since that's the hot path.
                    if (key is string keyString)
                    {
                        _keyConverter ??= GetConverter<string>(typeInfo.KeyTypeInfo!);
                        _keyConverter.WriteAsPropertyNameCore(
                            writer,
                            keyString,
                            options,
                            state.Current.IsWritingExtensionDataProperty
                        );
                    }
                    else
                    {
                        // IDictionary is a special case since it has polymorphic object semantics on serialization
                        // but needs to use KdlConverter<string> on deserialization.
                        _valueConverter.WriteAsPropertyNameCore(
                            writer,
                            key,
                            options,
                            state.Current.IsWritingExtensionDataProperty
                        );
                    }
                }

                object? element = enumerator.Value;
                if (!_valueConverter.TryWrite(writer, element, options, ref state))
                {
                    return false;
                }

                state.Current.EndDictionaryEntry();
            } while (enumerator.MoveNext());

            return true;
        }

        internal override void ConfigureKdlTypeInfo(
            KdlTypeInfo kdlTypeInfo,
            KdlSerializerOptions options
        )
        {
            // Deserialize as Dictionary<TKey,TValue> for interface types that support it.
            if (
                kdlTypeInfo.CreateObject is null
                && Type.IsAssignableFrom(typeof(Dictionary<string, object?>))
            )
            {
                Debug.Assert(Type.IsInterface);
                kdlTypeInfo.CreateObject = () => new Dictionary<string, object?>();
            }
        }
    }
}
