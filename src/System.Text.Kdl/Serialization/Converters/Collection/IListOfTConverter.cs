using System.Diagnostics;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.Generic.IList{TElement}</cref>.
    /// </summary>
    internal sealed class IListOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IList<TElement>
    {
        internal override bool CanPopulate => true;

        protected override void Add(in TElement value, ref ReadStack state)
        {
            TCollection collection = (TCollection)state.Current.ReturnValue!;
            collection.Add(value);
            if (IsValueType)
            {
                state.Current.ReturnValue = collection;
            };
        }

        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
        {
            base.CreateCollection(ref reader, ref state, options);
            TCollection returnValue = (TCollection)state.Current.ReturnValue!;
            if (returnValue.IsReadOnly)
            {
                state.Current.ReturnValue = null; // clear out for more accurate KdlPath reporting.
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(Type, ref reader, ref state);
            }
        }

        internal override void ConfigureKdlTypeInfo(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            // Deserialize as List<T> for interface types that support it.
            if (jsonTypeInfo.CreateObject is null && Type.IsAssignableFrom(typeof(List<TElement>)))
            {
                Debug.Assert(Type.IsInterface);
                jsonTypeInfo.CreateObject = () => new List<TElement>();
            }
        }
    }
}
