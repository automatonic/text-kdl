namespace System.Text.Kdl.Serialization.Converters
{
    /// Converter for <cref>System.Collections.Generic.List{TElement}</cref>.
    internal sealed class ListOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : List<TElement>
    {
        internal override bool CanPopulate => true;

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
        {
            if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
            {
                return;
            }

            if (state.Current.KdlTypeInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.KdlTypeInfo.Type);
            }

            state.Current.ReturnValue = state.Current.KdlTypeInfo.CreateObject();
        }

        protected override bool OnWriteResume(KdlWriter writer, TCollection value, KdlSerializerOptions options, ref WriteStack state)
        {
            List<TElement> list = value;

            // Using an index is 2x faster than using an enumerator.
            int index = state.Current.EnumeratorIndex;
            KdlConverter<TElement> elementConverter = GetElementConverter(ref state);

            if (elementConverter.CanUseDirectReadOrWrite && state.Current.NumberHandling == null)
            {
                // Fast path that avoids validation and extra indirection.
                for (; index < list.Count; index++)
                {
                    elementConverter.Write(writer, list[index], options);
                }
            }
            else
            {
                for (; index < list.Count; index++)
                {
                    TElement element = list[index];
                    if (!elementConverter.TryWrite(writer, element, options, ref state))
                    {
                        state.Current.EnumeratorIndex = index;
                        return false;
                    }

                    state.Current.EndCollectionElement();

                    if (ShouldFlush(ref state, writer))
                    {
                        state.Current.EnumeratorIndex = ++index;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
