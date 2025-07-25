﻿using System.Diagnostics;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class IAsyncEnumerableOfTConverter<TAsyncEnumerable, TElement>
        : KdlCollectionConverter<TAsyncEnumerable, TElement>
        where TAsyncEnumerable : IAsyncEnumerable<TElement>
    {
        internal override bool OnTryRead(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options,
            scoped ref ReadStack state,
            out TAsyncEnumerable value
        )
        {
            if (!typeToConvert.IsAssignableFrom(typeof(IAsyncEnumerable<TElement>)))
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(
                    Type,
                    ref reader,
                    ref state
                );
            }

            return base.OnTryRead(ref reader, typeToConvert, options, ref state, out value!);
        }

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((BufferedAsyncEnumerable)state.Current.ReturnValue!)._buffer.Add(value);
        }

        internal override bool SupportsCreateObjectDelegate => false;

        protected override void CreateCollection(
            ref KdlReader reader,
            scoped ref ReadStack state,
            KdlSerializerOptions options
        )
        {
            state.Current.ReturnValue = new BufferedAsyncEnumerable();
        }

        internal override bool OnTryWrite(
            KdlWriter writer,
            TAsyncEnumerable value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            if (!state.SupportAsync)
            {
                ThrowHelper.ThrowNotSupportedException_TypeRequiresAsyncSerialization(Type);
            }

            return base.OnTryWrite(writer, value, options, ref state);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Reliability",
            "CA2012:Use ValueTasks correctly",
            Justification = "Converter needs to consume ValueTask's in a non-async context"
        )]
        protected override bool OnWriteResume(
            KdlWriter writer,
            TAsyncEnumerable value,
            KdlSerializerOptions options,
            ref WriteStack state
        )
        {
            IAsyncEnumerator<TElement> enumerator;
            ValueTask<bool> moveNextTask;

            if (state.Current.AsyncDisposable is null)
            {
                enumerator = value.GetAsyncEnumerator(state.CancellationToken);
                // async enumerators can only be disposed asynchronously;
                // store in the WriteStack for future disposal
                // by the root async serialization context.
                state.Current.AsyncDisposable = enumerator;
                // enumerator.MoveNextAsync() calls can throw,
                // ensure the enumerator already is stored
                // in the WriteStack for proper disposal.
                moveNextTask = enumerator.MoveNextAsync();

                if (!moveNextTask.IsCompleted)
                {
                    // It is common for first-time MoveNextAsync() calls to return pending tasks,
                    // since typically that is when underlying network connections are being established.
                    // For this case only, suppress flushing the current buffer contents (e.g. the leading '[' token of the written array)
                    // to give the stream owner the ability to recover in case of a connection error.
                    state.SuppressFlush = true;
                    goto SuspendDueToPendingTask;
                }
            }
            else
            {
                Debug.Assert(state.Current.AsyncDisposable is IAsyncEnumerator<TElement>);
                enumerator = (IAsyncEnumerator<TElement>)state.Current.AsyncDisposable;

                if (state.Current.AsyncEnumeratorIsPendingCompletion)
                {
                    // converter was previously suspended due to a pending MoveNextAsync() task
                    Debug.Assert(state.PendingTask is Task<bool> && state.PendingTask.IsCompleted);
                    moveNextTask = new ValueTask<bool>((Task<bool>)state.PendingTask);
                    state.Current.AsyncEnumeratorIsPendingCompletion = false;
                    state.PendingTask = null;
                }
                else
                {
                    // converter was suspended for a different reason;
                    // the last MoveNextAsync() call can only have completed with 'true'.
                    moveNextTask = new ValueTask<bool>(true);
                }
            }

            Debug.Assert(moveNextTask.IsCompleted);
            KdlConverter<TElement> converter = GetElementConverter(ref state);

            // iterate through the enumerator while elements are being returned synchronously
            do
            {
                if (!moveNextTask.Result)
                {
                    // we have completed serialization for the enumerator,
                    // clear from the stack and schedule for async disposal.
                    state.Current.AsyncDisposable = null;
                    state.AddCompletedAsyncDisposable(enumerator);
                    return true;
                }

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
                moveNextTask = enumerator.MoveNextAsync();
            } while (moveNextTask.IsCompleted);

            SuspendDueToPendingTask:
            // we have a pending MoveNextAsync() call;
            // wrap inside a regular task so that it can be awaited multiple times;
            // mark the current stackframe as pending completion.
            Debug.Assert(state.PendingTask is null);
            state.PendingTask = moveNextTask.AsTask();
            state.Current.AsyncEnumeratorIsPendingCompletion = true;
            return false;
        }

        private sealed class BufferedAsyncEnumerable : IAsyncEnumerable<TElement>
        {
            public readonly List<TElement> _buffer = [];

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken _)
            {
                foreach (TElement element in _buffer)
                {
                    yield return element;
                }
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }
    }
}
