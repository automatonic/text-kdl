using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.Generic.IEnumerable{TElement}</cref>.
    /// </summary>
    internal sealed class IEnumerableOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
        private readonly bool _isDeserializable = typeof(TCollection).IsAssignableFrom(typeof(List<TElement>));

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        internal override bool SupportsCreateObjectDelegate => false;
        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
        {
            if (!_isDeserializable)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(Type, ref reader, ref state);
            }

            state.Current.ReturnValue = new List<TElement>();
        }
    }
}
