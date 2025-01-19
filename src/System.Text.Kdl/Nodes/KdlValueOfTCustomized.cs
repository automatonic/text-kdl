using System.Diagnostics;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Nodes
{
    /// <summary>
    /// A KdlValue that encapsulates arbitrary .NET type configurations.
    /// Paradoxically, instances of this type can be of any KdlValueKind
    /// (including objects and arrays) and introspecting these values is
    /// generally slower compared to the other KdlValue implementations.
    /// </summary>
    internal sealed class KdlValueCustomized<TValue> : KdlValue<TValue>
    {
        private readonly KdlTypeInfo<TValue> _jsonTypeInfo;
        private KdlValueKind? _valueKind;

        public KdlValueCustomized(TValue value, KdlTypeInfo<TValue> jsonTypeInfo, KdlNodeOptions? options = null) : base(value, options)
        {
            Debug.Assert(jsonTypeInfo.IsConfigured);
            _jsonTypeInfo = jsonTypeInfo;
        }

        private protected override KdlValueKind GetValueKindCore() => _valueKind ??= ComputeValueKind();
        internal override KdlVertex DeepCloneCore() => KdlSerializer.SerializeToNode(Value, _jsonTypeInfo)!;

        public override void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            KdlTypeInfo<TValue> jsonTypeInfo = _jsonTypeInfo;

            if (options != null && options != jsonTypeInfo.Options)
            {
                options.MakeReadOnly();
                jsonTypeInfo = (KdlTypeInfo<TValue>)options.GetTypeInfoInternal(typeof(TValue));
            }

            jsonTypeInfo.Serialize(writer, Value);
        }

        /// <summary>
        /// Computes the KdlValueKind of the value by serializing it and reading the resultant KDL.
        /// </summary>
        private KdlValueKind ComputeValueKind()
        {
            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(options: default, KdlSerializerOptions.BufferSizeDefault, out PooledByteBufferWriter output);
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
