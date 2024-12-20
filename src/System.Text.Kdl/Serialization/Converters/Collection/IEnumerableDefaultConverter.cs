using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Default base class implementation of <cref>KdlIEnumerableConverter{TCollection, TElement}</cref>.
    /// </summary>
    internal abstract class IEnumerableDefaultConverter<TCollection, TElement> : KdlCollectionConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
        internal override bool CanHaveMetadata => true;

        protected override bool OnWriteResume(KdlWriter writer, TCollection value, KdlSerializerOptions options, ref WriteStack state)
        {
            Debug.Assert(value is not null);

            IEnumerator<TElement> enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                state.Current.CollectionEnumerator = enumerator;
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    return true;
                }
            }
            else
            {
                Debug.Assert(state.Current.CollectionEnumerator is IEnumerator<TElement>);
                enumerator = (IEnumerator<TElement>)state.Current.CollectionEnumerator;
            }

            KdlConverter<TElement> converter = GetElementConverter(ref state);
            do
            {
                if (ShouldFlush(ref state, writer))
                {
                    return false;
                }

                TElement element = enumerator.Current;
                if (!converter.TryWrite(writer, element, options, ref state))
                {
                    return false;
                }

                state.Current.EndCollectionElement();
            } while (enumerator.MoveNext());

            enumerator.Dispose();
            return true;
        }
    }
}
