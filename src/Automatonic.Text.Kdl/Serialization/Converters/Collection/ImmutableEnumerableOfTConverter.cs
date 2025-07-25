using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal class ImmutableEnumerableOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
        protected sealed override void Add(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        internal sealed override bool CanHaveMetadata => false;

        internal override bool SupportsCreateObjectDelegate => false;

        protected sealed override void CreateCollection(
            ref KdlReader reader,
            scoped ref ReadStack state,
            KdlSerializerOptions options
        )
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        internal sealed override bool IsConvertibleCollection => true;

        protected sealed override void ConvertCollection(
            ref ReadStack state,
            KdlSerializerOptions options
        )
        {
            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;

            Func<IEnumerable<TElement>, TCollection>? creator = (Func<
                IEnumerable<TElement>,
                TCollection
            >?)
                typeInfo.CreateObjectWithArgs;
            Debug.Assert(creator != null);
            state.Current.ReturnValue = creator((List<TElement>)state.Current.ReturnValue!);
        }
    }
}
