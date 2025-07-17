using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.Generic.IDictionary{TKey, TValue}</cref> that
    /// (de)serializes as a KDL object with properties representing the dictionary element key and value.
    /// </summary>
    internal sealed class IDictionaryOfTKeyTValueConverter<TDictionary, TKey, TValue>
        : DictionaryDefaultConverter<TDictionary, TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>
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
            TDictionary collection = (TDictionary)state.Current.ReturnValue!;
            collection[key] = value;
            if (IsValueType)
            {
                state.Current.ReturnValue = collection;
            }
            ;
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

        internal override void ConfigureKdlTypeInfo(
            KdlTypeInfo kdlTypeInfo,
            KdlSerializerOptions options
        )
        {
            // Deserialize as Dictionary<TKey,TValue> for interface types that support it.
            if (
                kdlTypeInfo.CreateObject is null
                && Type.IsAssignableFrom(typeof(Dictionary<TKey, TValue>))
            )
            {
                Debug.Assert(Type.IsInterface);
                kdlTypeInfo.CreateObject = () => new Dictionary<TKey, TValue>();
            }
        }
    }
}
