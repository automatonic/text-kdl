namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class QueueOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : Queue<TElement>
    {
        internal override bool CanPopulate => true;

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Enqueue(value);
        }

        protected override void CreateCollection(
            ref KdlReader reader,
            scoped ref ReadStack state,
            KdlSerializerOptions options
        )
        {
            if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
            {
                return;
            }

            if (state.Current.KdlTypeInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(
                    state.Current.KdlTypeInfo.Type
                );
            }

            state.Current.ReturnValue = state.Current.KdlTypeInfo.CreateObject();
        }
    }
}
