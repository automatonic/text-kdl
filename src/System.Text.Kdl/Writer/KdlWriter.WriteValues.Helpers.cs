using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        private void ValidateWritingValue()
        {
            Debug.Assert(!_options.SkipValidation);

            if (_inObject)
            {
                if (_tokenType != KdlTokenType.PropertyName)
                {
                    Debug.Assert(_tokenType is not KdlTokenType.None and not KdlTokenType.StartArray);
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotWriteValueWithinObject, currentDepth: default, maxDepth: _options.MaxDepth, token: default, _tokenType);
                }
            }
            else
            {
                Debug.Assert(_tokenType != KdlTokenType.PropertyName);

                // It is more likely for CurrentDepth to not equal 0 when writing valid KDL, so check that first to rely on short-circuiting and return quickly.
                if (CurrentDepth == 0 && _tokenType != KdlTokenType.None)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotWriteValueAfterPrimitiveOrClose, currentDepth: default, maxDepth: _options.MaxDepth, token: default, _tokenType);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Base64EncodeAndWrite(ReadOnlySpan<byte> bytes, Span<byte> output)
        {
            Span<byte> destination = output[BytesPending..];
            OperationStatus status = Base64.EncodeToUtf8(bytes, destination, out int consumed, out int written);
            Debug.Assert(status == OperationStatus.Done);
            Debug.Assert(consumed == bytes.Length);
            BytesPending += written;
        }
    }
}
