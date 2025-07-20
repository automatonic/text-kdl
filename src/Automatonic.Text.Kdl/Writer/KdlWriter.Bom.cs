namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes a document-level byte order mark to the KDL output.
        /// </summary>
        /// <remarks>
        /// This method can only be called at the start of the document.
        /// <code>
        /// document := bom? version? nodes
        /// bom := '\u{FEFF}'
        /// </code>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the BOM is attempted to be written after any nodes have been written.
        /// </exception>
        public void WriteDocumentBom()
        {
            if (BytesCommitted > 0)
            {
                ThrowHelper.ThrowInvalidOperationException_KdlWriter_DocumentBomOnlyAtStart();
            }
            int bytesToWrite = 2;
            if (_memory.Length - BytesPending < bytesToWrite)
            {
                Grow(bytesToWrite);
            }
            var output = _memory.Span;
            output[BytesPending++] = 0xFE; // BOM start
            output[BytesPending++] = 0xFF; // BOM end
        }
    }
}
