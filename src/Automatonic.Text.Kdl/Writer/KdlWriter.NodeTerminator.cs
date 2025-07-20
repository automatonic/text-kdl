namespace Automatonic.Text.Kdl
{
    using System.Text;
    using System.Text.Unicode;

    public sealed partial class KdlWriter
    {
        /// <summary>
        /// Delegate for writing a KDL Node Terminator.
        /// </summary>
        /// <param name="writer"></param>
        public delegate void NodeTerminatorWriter(KdlWriter writer, string? comment = null);

        /// <summary>
        /// Writes the KDL Node Terminator as a newline.
        /// </summary>
        /// <remarks>
        /// node-terminator := single-line-comment | newline | ';' | eof
        /// </remarks>
        public static void WriteNodeTerminatorNewline(KdlWriter writer, string? comment = null)
        {
            int bytesToWrite = 1;
            if (writer._memory.Length - writer.BytesPending < bytesToWrite)
            {
                writer.Grow(bytesToWrite);
            }
            var output = writer._memory.Span;
            //TECHDEBT: this should probably default to Environment.NewLine and be
            // configurable in the options.
            //c.f. writer.Options.NewLine
            output[writer.BytesPending++] = 0x0A; // '\n'
        }

        public static void WriteNodeTerminatorSemicolon(KdlWriter writer, string? comment = null)
        {
            int bytesToWrite = 1;
            if (writer._memory.Length - writer.BytesPending < bytesToWrite)
            {
                writer.Grow(bytesToWrite);
            }
            var output = writer._memory.Span;
            output[writer.BytesPending++] = 0x3B; // ';'
        }

        public static void WriteNodeTerminatorEof(KdlWriter writer, string? comment = null)
        {
            // No action needed for EOF, as it is the end of the document.
            // This is just a placeholder to indicate EOF as a terminator.
        }

        public static void WriteNodeTerminatorComment(KdlWriter writer, string? comment = null)
        {
            throw new NotImplementedException("Comment terminators are not yet implemented.");
            // var commentByteSpan = writer._memory.Span.Slice(writer.BytesPending);
            // var utf8PropertyName = System.Text.Encoding.UTF8.Get("kdl-comment");
            // int commentIdx = KdlWriterHelper.NeedsEscaping(comment, writer._options.CommentEncoder);

            // Debug.Assert(
            //     commentIdx >= -1
            //         && commentIdx < utf8PropertyName.Length
            //         && commentIdx < int.MaxValue / 2
            // );

            // if (commentIdx != -1)
            // {
            //     WriteStringEscapeProperty(utf8PropertyName, commentIdx);
            // }
            // else
            // {
            //     WriteStringByOptionsPropertyName(utf8PropertyName);
            // }

            // comment ??= string.Empty;
            // int bytesToWrite = 2 + System.Text.Encoding.UTF8.GetByteCount(comment);
            // Utf8.GetBytes(
            //     comment,
            //     writer._memory.Span.Slice(writer.BytesPending),
            //     out int bytesWritten
            // );
            // if (writer._memory.Length - writer.BytesPending < bytesToWrite)
            // {
            //     writer.Grow(bytesToWrite);
            // }
            // var output = writer._memory.Span;
            // output[writer.BytesPending++] = 0x2F; // '/'
            // output[writer.BytesPending++] = 0x2F; // '/'
        }
    }
}
