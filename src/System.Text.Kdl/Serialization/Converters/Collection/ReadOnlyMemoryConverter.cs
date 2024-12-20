// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class ReadOnlyMemoryConverter<T> : KdlCollectionConverter<ReadOnlyMemory<T>, T>
    {
        internal override bool CanHaveMetadata => false;
        public override bool HandleNull => true;

        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out ReadOnlyMemory<T> value)
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
            ReadOnlyMemory<T> memory = ((List<T>)state.Current.ReturnValue!).ToArray().AsMemory();
            state.Current.ReturnValue = memory;
        }

        protected override bool OnWriteResume(KdlWriter writer, ReadOnlyMemory<T> value, KdlSerializerOptions options, ref WriteStack state)
        {
            return OnWriteResume(writer, value.Span, options, ref state);
        }

        internal static bool OnWriteResume(KdlWriter writer, ReadOnlySpan<T> value, KdlSerializerOptions options, ref WriteStack state)
        {
            int index = state.Current.EnumeratorIndex;

            KdlConverter<T> elementConverter = GetElementConverter(ref state);

            if (elementConverter.CanUseDirectReadOrWrite && state.Current.NumberHandling == null)
            {
                // Fast path that avoids validation and extra indirection.
                for (; index < value.Length; index++)
                {
                    elementConverter.Write(writer, value[index], options);
                }
            }
            else
            {
                for (; index < value.Length; index++)
                {
                    if (!elementConverter.TryWrite(writer, value[index], options, ref state))
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
