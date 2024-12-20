using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Base class for converters that are able to resume after reading or writing to a buffer.
    /// This is used when the Stream-based serialization APIs are used.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class KdlResumableConverter<T> : KdlConverter<T>
    {
        public override bool HandleNull => false;

        public sealed override T? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            // Bridge from resumable to value converters.

            ReadStack state = default;
            KdlTypeInfo jsonTypeInfo = options.GetTypeInfoInternal(typeToConvert);
            state.Initialize(jsonTypeInfo);

            TryRead(ref reader, typeToConvert, options, ref state, out T? value, out _);
            return value;
        }

        public sealed override void Write(KdlWriter writer, T value, KdlSerializerOptions options)
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            // Bridge from resumable to value converters.
            WriteStack state = default;
            KdlTypeInfo typeInfo = options.GetTypeInfoInternal(typeof(T));
            state.Initialize(typeInfo);

            try
            {
                TryWrite(writer, value, options, ref state);
            }
            catch
            {
                state.DisposePendingDisposablesOnException();
                throw;
            }
        }
    }
}
