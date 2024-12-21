using System.Buffers;
using System.Diagnostics;

namespace System.Text.Kdl
{
    /// <summary>
    /// Defines a thread-local cache for KdlSerializer to store reusable KdlWriter/IBufferWriter instances.
    /// </summary>
    internal static class KdlWriterCache
    {
        [ThreadStatic]
        private static ThreadLocalState? t_threadLocalState;

        public static KdlWriter RentWriterAndBuffer(KdlSerializerOptions options, out PooledByteBufferWriter bufferWriter) =>
            RentWriterAndBuffer(options.GetWriterOptions(), options.DefaultBufferSize, out bufferWriter);

        public static KdlWriter RentWriterAndBuffer(KdlWriterOptions options, int defaultBufferSize, out PooledByteBufferWriter bufferWriter)
        {
            ThreadLocalState state = t_threadLocalState ??= new();
            KdlWriter writer;

            if (state.RentedWriters++ == 0)
            {
                // First KdlSerializer call in the stack -- initialize & return the cached instances.
                bufferWriter = state.BufferWriter;
                writer = state.Writer;

                bufferWriter.InitializeEmptyInstance(defaultBufferSize);
                writer.Reset(bufferWriter, options);
            }
            else
            {
                // We're in a recursive KdlSerializer call -- return fresh instances.
                bufferWriter = new PooledByteBufferWriter(defaultBufferSize);
                writer = new KdlWriter(bufferWriter, options);
            }

            return writer;
        }

        public static KdlWriter RentWriter(KdlSerializerOptions options, IBufferWriter<byte> bufferWriter)
        {
            ThreadLocalState state = t_threadLocalState ??= new();
            KdlWriter writer;

            if (state.RentedWriters++ == 0)
            {
                // First KdlSerializer call in the stack -- initialize & return the cached instance.
                writer = state.Writer;
                writer.Reset(bufferWriter, options.GetWriterOptions());
            }
            else
            {
                // We're in a recursive KdlSerializer call -- return a fresh instance.
                writer = new KdlWriter(bufferWriter, options.GetWriterOptions());
            }

            return writer;
        }

        public static void ReturnWriterAndBuffer(KdlWriter writer, PooledByteBufferWriter bufferWriter)
        {
            Debug.Assert(t_threadLocalState != null);
            ThreadLocalState state = t_threadLocalState;

            writer.ResetAllStateForCacheReuse();
            bufferWriter.ClearAndReturnBuffers();

            int rentedWriters = --state.RentedWriters;
            Debug.Assert(rentedWriters == 0 == (ReferenceEquals(state.BufferWriter, bufferWriter) && ReferenceEquals(state.Writer, writer)));
        }

        public static void ReturnWriter(KdlWriter writer)
        {
            Debug.Assert(t_threadLocalState != null);
            ThreadLocalState state = t_threadLocalState;

            writer.ResetAllStateForCacheReuse();

            int rentedWriters = --state.RentedWriters;
            Debug.Assert(rentedWriters == 0 == ReferenceEquals(state.Writer, writer));
        }

        private sealed class ThreadLocalState
        {
            public readonly PooledByteBufferWriter BufferWriter;
            public readonly KdlWriter Writer;
            public int RentedWriters;

            public ThreadLocalState()
            {
                BufferWriter = PooledByteBufferWriter.CreateEmptyInstanceForCaching();
                Writer = KdlWriter.CreateEmptyInstanceForCaching();
            }
        }
    }
}
