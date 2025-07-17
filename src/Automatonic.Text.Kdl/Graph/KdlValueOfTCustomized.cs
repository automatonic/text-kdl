using System.Diagnostics;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Graph
{
    /// <summary>
    /// A KdlValue that encapsulates arbitrary .NET type configurations.
    /// Paradoxically, instances of this type can be of any KdlValueKind
    /// (including objects and arrays) and introspecting these values is
    /// generally slower compared to the other KdlValue implementations.
    /// </summary>
    internal sealed class KdlValueCustomized<TValue> : KdlValue<TValue>
    {
        private readonly KdlTypeInfo<TValue> _kdlTypeInfo;
        private KdlValueKind? _valueKind;

        public KdlValueCustomized(
            TValue value,
            KdlTypeInfo<TValue> kdlTypeInfo,
            KdlElementOptions? options = null
        )
            : base(value, options)
        {
            Debug.Assert(kdlTypeInfo.IsConfigured);
            _kdlTypeInfo = kdlTypeInfo;
        }

        private protected override KdlValueKind GetValueKindCore() =>
            _valueKind ??= ComputeValueKind();

        internal override KdlElement DeepCloneCore() =>
            KdlSerializer.SerializeToNode(Value, _kdlTypeInfo)!;

        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            KdlTypeInfo<TValue> kdlTypeInfo = _kdlTypeInfo;

            if (options != null && options != kdlTypeInfo.Options)
            {
                options.MakeReadOnly();
                kdlTypeInfo = (KdlTypeInfo<TValue>)options.GetTypeInfoInternal(typeof(TValue));
            }

            kdlTypeInfo.Serialize(writer, Value);
        }

        /// <summary>
        /// Computes the KdlValueKind of the value by serializing it and reading the resultant KDL.
        /// </summary>
        private KdlValueKind ComputeValueKind()
        {
            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(
                options: default,
                KdlSerializerOptions.BufferSizeDefault,
                out PooledByteBufferWriter output
            );
            try
            {
                WriteTo(writer);
                writer.Flush();
                KdlReader reader = new(output.WrittenMemory.Span);
                bool success = reader.Read();
                Debug.Assert(success);
                return KdlReaderHelper.ToValueKind(reader.TokenType);
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }
    }
}
