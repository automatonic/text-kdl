namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Writes the KDL document version (2) declaration.
        /// </summary>
        /// <remarks>
        /// <code>
        /// document := bom? version? nodes
        /// version :=
        ///     '/-' unicode-space* 'kdl-version' unicode-space+ ('1' | '2')
        ///     unicode-space* newline
        /// </code>
        /// </remarks>
        public void WriteDocumentVersion()
        {
            byte versionByte = 0x32; // '2'
            WriteDocumentVersion(versionByte);
        }

        /// <summary>
        /// Writes the KDL document version declaration.
        /// </summary>
        /// <remarks>
        /// <code>
        /// document := bom? version? nodes
        /// version :=
        ///     '/-' unicode-space* 'kdl-version' unicode-space+ ('1' | '2')
        ///     unicode-space* newline
        /// </code>
        /// </remarks>
        internal void WriteDocumentVersion(byte version)
        {
            if (_anyNodeHasBeenWritten)
            {
                ThrowHelper.ThrowInvalidOperationException_KdlWriter_VersionMustPrecedeAnyNode();
            }
            // Write the version declaration
            // "/- kdl-version 2\n";
            int bytesToWrite = 12;

            if (_memory.Length - BytesPending < bytesToWrite)
            {
                Grow(bytesToWrite);
            }
            var output = _memory.Span;
            output[BytesPending++] = 0x2F; // '/'
            output[BytesPending++] = 0x2D; // '-'
            output[BytesPending++] = 0x20; // ' '
            output[BytesPending++] = 0x6B; // 'k'
            output[BytesPending++] = 0x64; // 'd'
            output[BytesPending++] = 0x6C; // 'l'
            output[BytesPending++] = 0x2D; // '-'
            output[BytesPending++] = 0x76; // 'v'
            output[BytesPending++] = 0x65; // 'e'
            output[BytesPending++] = 0x72; // 'r'
            output[BytesPending++] = 0x73; // 's'
            output[BytesPending++] = 0x69; // 'i'
            output[BytesPending++] = 0x6F; // 'o'
            output[BytesPending++] = 0x6E; // 'n'
            output[BytesPending++] = 0x20; // ' '
            output[BytesPending++] = version; // e.g. '1' or '2'
            output[BytesPending++] = 0x0A; // '\n'
            BytesPending += bytesToWrite;
        }
    }
}
