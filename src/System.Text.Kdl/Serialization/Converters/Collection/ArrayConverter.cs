namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Array</cref>.
    /// </summary>
    internal sealed class ArrayConverter<TCollection, TElement> : IEnumerableDefaultConverter<TElement[], TElement>
    {
        internal override bool CanHaveMetadata => false;

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        internal override bool SupportsCreateObjectDelegate => false;
        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        internal sealed override bool IsConvertibleCollection => true;
        protected override void ConvertCollection(ref ReadStack state, KdlSerializerOptions options)
        {
            List<TElement> list = (List<TElement>)state.Current.ReturnValue!;
            state.Current.ReturnValue = list.ToArray();
        }

        protected override bool OnWriteResume(KdlWriter writer, TElement[] array, KdlSerializerOptions options, ref WriteStack state)
        {
            int index = state.Current.EnumeratorIndex;

            KdlConverter<TElement> elementConverter = GetElementConverter(ref state);
            if (elementConverter.CanUseDirectReadOrWrite && state.Current.NumberHandling == null)
            {
                // Fast path that avoids validation and extra indirection.
                for (; index < array.Length; index++)
                {
                    elementConverter.Write(writer, array[index], options);
                }
            }
            else
            {
                for (; index < array.Length; index++)
                {
                    TElement element = array[index];
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
