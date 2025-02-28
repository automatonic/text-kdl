using System.Collections.Concurrent;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class ConcurrentStackOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : ConcurrentStack<TElement>
    {
        internal override bool CanPopulate => true;

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Push(value);
        }
    }
}
