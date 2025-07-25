using System.Collections;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.IEnumerable</cref>.
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    internal sealed class IEnumerableConverter<TCollection>
        : KdlCollectionConverter<TCollection, object?>
        where TCollection : IEnumerable
    {
        private readonly bool _isDeserializable = typeof(TCollection).IsAssignableFrom(
            typeof(List<object?>)
        );

        protected override void Add(in object? value, ref ReadStack state)
        {
            ((List<object?>)state.Current.ReturnValue!).Add(value);
        }

        internal override bool SupportsCreateObjectDelegate => false;

        protected override void CreateCollection(
            ref KdlReader reader,
            scoped ref ReadStack state,
            KdlSerializerOptions options
        )
        {
            if (!_isDeserializable)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(
                    Type,
                    ref reader,
                    ref state
                );
            }

            state.Current.ReturnValue = new List<object?>();
        }

        // Consider overriding ConvertCollection to convert the list to an array since a List is mutable.
        // However, converting from the temporary list to an array will be slower.

        protected override bool OnWriteResume(
            KdlWriter writer,
            TCollection value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            IEnumerator enumerator;
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
                enumerator = state.Current.CollectionEnumerator;
            }

            KdlConverter<object?> converter = GetElementConverter(ref state);
            do
            {
                if (ShouldFlush(ref state, writer))
                {
                    return false;
                }

                object? element = enumerator.Current;
                if (!converter.TryWrite(writer, element, options, ref state))
                {
                    return false;
                }

                state.Current.EndCollectionElement();
            } while (enumerator.MoveNext());

            return true;
        }
    }
}
