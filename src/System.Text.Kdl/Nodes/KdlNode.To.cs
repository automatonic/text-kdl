namespace System.Text.Kdl.Nodes
{
    public abstract partial class KdlElement
    {
        /// <summary>
        ///   Converts the current instance to string in KDL format.
        /// </summary>
        /// <param name="options">Options to control the serialization behavior.</param>
        /// <returns>KDL representation of current instance.</returns>
        public string ToKdlString(KdlSerializerOptions? options = null)
        {
            KdlWriterOptions writerOptions = default;
            int defaultBufferSize = KdlSerializerOptions.BufferSizeDefault;
            if (options is not null)
            {
                writerOptions = options.GetWriterOptions();
                defaultBufferSize = options.DefaultBufferSize;
            }

            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(writerOptions, defaultBufferSize, out PooledByteBufferWriter output);
            try
            {
                WriteTo(writer, options);
                writer.Flush();
                return KdlHelpers.Utf8GetString(output.WrittenMemory.Span);
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }

        /// <summary>
        ///   Gets a string representation for the current value appropriate to the node type.
        /// </summary>
        /// <returns>A string representation for the current value appropriate to the node type.</returns>
        public override string ToString()
        {
            // Special case for string; don't quote it.
            if (this is KdlValue)
            {
                if (this is KdlValuePrimitive<string> jsonString)
                {
                    return jsonString.Value;
                }

                if (this is KdlValueOfElement { Value.ValueKind: KdlValueKind.String } kdlElement)
                {
                    return kdlElement.Value.GetString()!;
                }
            }

            KdlWriter writer = KdlWriterCache.RentWriterAndBuffer(new KdlWriterOptions { Indented = true }, KdlSerializerOptions.BufferSizeDefault, out PooledByteBufferWriter output);
            try
            {
                WriteTo(writer);
                writer.Flush();
                return KdlHelpers.Utf8GetString(output.WrittenMemory.Span);
            }
            finally
            {
                KdlWriterCache.ReturnWriterAndBuffer(writer, output);
            }
        }

        /// <summary>
        ///   Write the <see cref="KdlElement"/> into the provided <see cref="KdlWriter"/> as KDL.
        /// </summary>
        /// <param name="writer">The <see cref="KdlWriter"/>.</param>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="writer"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <param name="options">Options to control the serialization behavior.</param>
        public abstract void WriteTo(KdlWriter writer, KdlSerializerOptions? options = null);
    }
}
