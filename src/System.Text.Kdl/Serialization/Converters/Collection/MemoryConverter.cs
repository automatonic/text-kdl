using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class MemoryConverter<T> : KdlCollectionConverter<Memory<T>, T>
    {
        internal override bool CanHaveMetadata => false;
        public override bool HandleNull => true;

        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out Memory<T> value)
        {
            if (reader.TokenType is KdlTokenType.Null)
            {
                value = default;
                return true;
            }

            return base.OnTryRead(ref reader, typeToConvert, options, ref state, out value);
        }

        protected override void Add(in T value, ref ReadStack state)
        {
            ((List<T>)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref KdlReader reader, scoped ref ReadStack state, KdlSerializerOptions options)
        {
            state.Current.ReturnValue = new List<T>();
        }

        internal sealed override bool IsConvertibleCollection => true;
        protected override void ConvertCollection(ref ReadStack state, KdlSerializerOptions options)
        {
            Memory<T> memory = ((List<T>)state.Current.ReturnValue!).ToArray().AsMemory();
            state.Current.ReturnValue = memory;
        }

        protected override bool OnWriteResume(KdlWriter writer, Memory<T> value, KdlSerializerOptions options, ref WriteStack state)
        {
            return ReadOnlyMemoryConverter<T>.OnWriteResume(writer, value.Span, options, ref state);
        }
    }
}
