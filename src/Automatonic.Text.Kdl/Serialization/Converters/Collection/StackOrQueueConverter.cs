using System.Collections;
using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal class StackOrQueueConverter<TCollection>
        : KdlCollectionConverter<TCollection, object?>
        where TCollection : IEnumerable
    {
        internal override bool CanPopulate => true;

        protected sealed override void Add(in object? value, ref ReadStack state)
        {
            var addMethodDelegate = (Action<TCollection, object?>?)state.Current.KdlTypeInfo.AddMethodDelegate;
            Debug.Assert(addMethodDelegate != null);
            addMethodDelegate((TCollection)state.Current.ReturnValue!, value);
        }

        protected sealed override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
        {
            if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
            {
                return;
            }

            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;
            Func<object>? constructorDelegate = typeInfo.CreateObject;

            if (constructorDelegate == null)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(Type, ref reader, ref state);
            }

            state.Current.ReturnValue = constructorDelegate();

            Debug.Assert(typeInfo.AddMethodDelegate != null);
        }

        protected sealed override bool OnWriteResume(KdlWriter writer, TCollection value, KdlSerializerOptions options, ref WriteStack state)
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
