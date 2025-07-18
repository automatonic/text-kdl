﻿using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Automatonic.Text.Kdl.Serialization
{
    [StructLayout(LayoutKind.Auto)]
    internal struct ReadBufferState : IDisposable
    {
        private byte[] _buffer;
        private byte _offset; // Read bytes offset typically used when skipping the UTF-8 BOM.
        private int _count; // Number of read bytes yet to be consumed by the serializer.
        private int _maxCount; // Number of bytes we need to clear before returning the buffer.
        private bool _isFirstBlock;
        private bool _isFinalBlock;

        // An "unsuccessful read" in this context refers to a buffer read operation that
        // wasn't sufficient to advance the reader to the next token. This occurs primarily
        // when consuming large KDL strings (which don't support streaming today) but is
        // also possible with other token types such as numbers, booleans, or nulls.
        //
        // The KdlSerializer.DeserializeAsyncEnumerable methods employ a special buffering
        // strategy where rather than attempting to fill the entire buffer, the deserializer
        // will be invoked as soon as the first chunk of data is read from the stream.
        // This is to ensure liveness: data should be surfaced on the IAE as soon as they
        // are streamed from the server. On the other hand, this can create performance
        // problems in cases where the underlying stream uses extremely fine-grained buffering.
        // For this reason, we employ a threshold that will revert to buffer filling once crossed.
        // The counter is reset to zero whenever the KDL reader has been advanced successfully.
        //
        // The threshold is set to 5 unsuccessful reads. This is a relatively conservative threshold
        // but should still make fallback unlikely in most scenaria. It should ensure that fallback
        // isn't triggered in null or boolean tokens even in the worst-case scenario where they are
        // streamed one byte at a time.
        private const int UnsuccessfulReadCountThreshold = 5;
        private int _unsuccessfulReadCount;

        public ReadBufferState(int initialBufferSize)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(
                Math.Max(initialBufferSize, KdlConstants.Utf8Bom.Length)
            );
            _maxCount = _count = _offset = 0;
            _isFirstBlock = true;
            _isFinalBlock = false;
        }

        public readonly bool IsFinalBlock => _isFinalBlock;

        public readonly ReadOnlySpan<byte> Bytes => _buffer.AsSpan(_offset, _count);

        /// <summary>
        /// Read from the stream until either our buffer is filled or we hit EOF.
        /// Calling ReadCore is relatively expensive, so we minimize the number of times
        /// we need to call it.
        /// </summary>
        public readonly async ValueTask<ReadBufferState> ReadFromStreamAsync(
            Stream utf8Kdl,
            CancellationToken cancellationToken,
            bool fillBuffer = true
        )
        {
            // Since mutable structs don't work well with async state machines,
            // make all updates on a copy which is returned once complete.
            ReadBufferState bufferState = this;

            int minBufferCount =
                fillBuffer || _unsuccessfulReadCount > UnsuccessfulReadCountThreshold
                    ? bufferState._buffer.Length
                    : 0;
            do
            {
                int bytesRead = await utf8Kdl
                    .ReadAsync(bufferState._buffer.AsMemory(bufferState._count), cancellationToken)
                    .ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    bufferState._isFinalBlock = true;
                    break;
                }

                bufferState._count += bytesRead;
            } while (bufferState._count < minBufferCount);

            bufferState.ProcessReadBytes();
            return bufferState;
        }

        /// <summary>
        /// Read from the stream until either our buffer is filled or we hit EOF.
        /// Calling ReadCore is relatively expensive, so we minimize the number of times
        /// we need to call it.
        /// </summary>
        public void ReadFromStream(Stream utf8Kdl)
        {
            do
            {
                int bytesRead = utf8Kdl.Read(_buffer.AsSpan(_count));

                if (bytesRead == 0)
                {
                    _isFinalBlock = true;
                    break;
                }

                _count += bytesRead;
            } while (_count < _buffer.Length);

            ProcessReadBytes();
        }

        /// <summary>
        /// Advances the buffer in anticipation of a subsequent read operation.
        /// </summary>
        public void AdvanceBuffer(int bytesConsumed)
        {
            Debug.Assert(bytesConsumed <= _count);

            _unsuccessfulReadCount = bytesConsumed == 0 ? _unsuccessfulReadCount + 1 : 0;
            _count -= bytesConsumed;

            if (!_isFinalBlock)
            {
                // Check if we need to shift or expand the buffer because there wasn't enough data to complete deserialization.
                if ((uint)_count > ((uint)_buffer.Length / 2))
                {
                    // We have less than half the buffer available, double the buffer size.
                    byte[] oldBuffer = _buffer;
                    int oldMaxCount = _maxCount;
                    byte[] newBuffer = ArrayPool<byte>.Shared.Rent(
                        (_buffer.Length < (int.MaxValue / 2)) ? _buffer.Length * 2 : int.MaxValue
                    );

                    // Copy the unprocessed data to the new buffer while shifting the processed bytes.
                    Buffer.BlockCopy(oldBuffer, _offset + bytesConsumed, newBuffer, 0, _count);
                    _buffer = newBuffer;
                    _maxCount = _count;

                    // Clear and return the old buffer
                    new Span<byte>(oldBuffer, 0, oldMaxCount).Clear();
                    ArrayPool<byte>.Shared.Return(oldBuffer);
                }
                else if (_count != 0)
                {
                    // Shift the processed bytes to the beginning of buffer to make more room.
                    Buffer.BlockCopy(_buffer, _offset + bytesConsumed, _buffer, 0, _count);
                }
            }

            _offset = 0;
        }

        private void ProcessReadBytes()
        {
            if (_count > _maxCount)
            {
                _maxCount = _count;
            }

            if (_isFirstBlock)
            {
                _isFirstBlock = false;

                // Handle the UTF-8 BOM if present
                Debug.Assert(_buffer.Length >= KdlConstants.Utf8Bom.Length);
                if (_buffer.AsSpan(0, _count).StartsWith(KdlConstants.Utf8Bom))
                {
                    _offset = (byte)KdlConstants.Utf8Bom.Length;
                    _count -= KdlConstants.Utf8Bom.Length;
                }
            }
        }

        public void Dispose()
        {
            // Clear only what we used and return the buffer to the pool
            new Span<byte>(_buffer, 0, _maxCount).Clear();

            byte[] toReturn = _buffer;
            _buffer = null!;

            ArrayPool<byte>.Shared.Return(toReturn);
        }
    }
}
