﻿using System.Diagnostics;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    public partial class KdlTypeInfo<T>
    {
        // This section provides helper methods guiding root-level deserialization
        // of values corresponding according to the current KdlTypeInfo configuration.

        internal T? Deserialize(ref KdlReader reader, ref ReadStack state)
        {
            Debug.Assert(IsConfigured);
            bool success = EffectiveConverter.ReadCore(
                ref reader,
                out T? result,
                Options,
                ref state
            );
            Debug.Assert(success, "Should only return false for async deserialization");
            return result;
        }

        internal async ValueTask<T?> DeserializeAsync(
            Stream utf8Kdl,
            CancellationToken cancellationToken
        )
        {
            Debug.Assert(IsConfigured);
            KdlSerializerOptions options = Options;
            var bufferState = new ReadBufferState(options.DefaultBufferSize);
            ReadStack readStack = default;
            readStack.Initialize(this, supportContinuation: true);
            var kdlReaderState = new KdlReaderState(options.GetReaderOptions());

            try
            {
                while (true)
                {
                    bufferState = await bufferState
                        .ReadFromStreamAsync(utf8Kdl, cancellationToken)
                        .ConfigureAwait(false);
                    bool success = ContinueDeserialize(
                        ref bufferState,
                        ref kdlReaderState,
                        ref readStack,
                        out T? value
                    );

                    if (success)
                    {
                        return value;
                    }
                }
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        internal T? Deserialize(Stream utf8Kdl)
        {
            Debug.Assert(IsConfigured);
            KdlSerializerOptions options = Options;
            var bufferState = new ReadBufferState(options.DefaultBufferSize);
            ReadStack readStack = default;
            readStack.Initialize(this, supportContinuation: true);
            var kdlReaderState = new KdlReaderState(options.GetReaderOptions());

            try
            {
                while (true)
                {
                    bufferState.ReadFromStream(utf8Kdl);
                    bool success = ContinueDeserialize(
                        ref bufferState,
                        ref kdlReaderState,
                        ref readStack,
                        out T? value
                    );

                    if (success)
                    {
                        return value;
                    }
                }
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        /// <summary>
        /// Caches KdlTypeInfo&lt;List&lt;T&gt;&gt; instances used by the DeserializeAsyncEnumerable method.
        /// Store as a non-generic type to avoid triggering generic recursion in the AOT compiler.
        /// cf. https://github.com/dotnet/runtime/issues/85184
        /// </summary>
        internal KdlTypeInfo? _asyncEnumerableArrayTypeInfo;
        internal KdlTypeInfo? _asyncEnumerableRootLevelValueTypeInfo;

        internal sealed override object? DeserializeAsObject(
            ref KdlReader reader,
            ref ReadStack state
        ) => Deserialize(ref reader, ref state);

        internal sealed override async ValueTask<object?> DeserializeAsObjectAsync(
            Stream utf8Kdl,
            CancellationToken cancellationToken
        )
        {
            T? result = await DeserializeAsync(utf8Kdl, cancellationToken).ConfigureAwait(false);
            return result;
        }

        internal sealed override object? DeserializeAsObject(Stream utf8Kdl) =>
            Deserialize(utf8Kdl);

        internal bool ContinueDeserialize(
            ref ReadBufferState bufferState,
            ref KdlReaderState kdlReaderState,
            ref ReadStack readStack,
            out T? value
        )
        {
            var reader = new KdlReader(bufferState.Bytes, bufferState.IsFinalBlock, kdlReaderState);
            bool success = EffectiveConverter.ReadCore(
                ref reader,
                out value,
                Options,
                ref readStack
            );

            Debug.Assert(reader.BytesConsumed <= bufferState.Bytes.Length);
            bufferState.AdvanceBuffer((int)reader.BytesConsumed);
            kdlReaderState = reader.CurrentState;
            return success;
        }
    }
}
