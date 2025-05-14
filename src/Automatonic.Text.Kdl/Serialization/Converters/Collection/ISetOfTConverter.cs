using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class ISetOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : ISet<TElement>
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

        internal override void ConfigureKdlTypeInfo(KdlTypeInfo kdlTypeInfo, KdlSerializerOptions options)
        {
            // Deserialize as HashSet<TElement> for interface types that support it.
            if (kdlTypeInfo.CreateObject is null && Type.IsAssignableFrom(typeof(HashSet<TElement>)))
            {
                Debug.Assert(Type.IsInterface);
                kdlTypeInfo.CreateObject = () => new HashSet<TElement>();
            }
        }
    }
}
