using System.Collections.Concurrent;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class ConcurrentQueueOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : ConcurrentQueue<TElement>
    {
        internal override bool CanPopulate => true;

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Enqueue(value);
        }
    }
}
