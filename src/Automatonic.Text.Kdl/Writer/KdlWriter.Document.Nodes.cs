namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        public void WriteDocumentNode()
        {
            //TECHDEBT: this should use a "default" node terminator writer
            // that can be configured in the options.
            WriteDocumentNode(WriteNodeTerminatorNewline);
        }

        public void WriteDocumentNodeAndSemicolon()
        {
            WriteDocumentNode(WriteNodeTerminatorSemicolon);
        }

        

        /// <summary>
        /// Writes a document-level byte order mark to the KDL output.
        /// </summary>
        public void WriteDocumentNode(NodeTerminatorWriter writeNodeTerminator)
        {
            WriteBaseNode();

            // Write the node terminator
            writeNodeTerminator(this);
        }
    }
}
