using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.RandomAccess
{
    public readonly partial struct KdlReadOnlyElement
    {
        /// <summary>
        ///   Parses one KDL value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <returns>
        ///   A KdlElement representing the value (and nested values) read from the reader.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="KdlReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="KdlTokenType.PropertyName"/> or <see cref="KdlTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="KdlReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        ///
        ///   <para>
        ///     Upon completion of this method, <paramref name="reader"/> will be positioned at the
        ///     final token in the KDL value. If an exception is thrown, the reader is reset to
        ///     the state it was in when the method was called.
        ///   </para>
        ///
        ///   <para>
        ///     This method makes a copy of the data the reader acted on, so there is no caller
        ///     requirement to maintain data integrity beyond the return of this method.
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The current <paramref name="reader"/> token does not start or represent a value.
        /// </exception>
        /// <exception cref="KdlException">
        ///   A value could not be read from the reader.
        /// </exception>
        public static KdlReadOnlyElement ParseValue(ref KdlReader reader)
        {
            bool ret = KdlReadOnlyDocument.TryParseValue(ref reader, out KdlReadOnlyDocument? document, shouldThrow: true, useArrayPools: false);

            Debug.Assert(ret, "TryParseValue returned false with shouldThrow: true.");
            Debug.Assert(document != null, "null document returned with shouldThrow: true.");
            return document.RootElement;
        }

        internal static KdlReadOnlyElement ParseValue(Stream utf8Kdl, KdlReadOnlyDocumentOptions options)
        {
            KdlReadOnlyDocument document = KdlReadOnlyDocument.ParseValue(utf8Kdl, options);
            return document.RootElement;
        }

        internal static KdlReadOnlyElement ParseValue(ReadOnlySpan<byte> utf8Kdl, KdlReadOnlyDocumentOptions options)
        {
            KdlReadOnlyDocument document = KdlReadOnlyDocument.ParseValue(utf8Kdl, options);
            return document.RootElement;
        }

        internal static KdlReadOnlyElement ParseValue(string kdl, KdlReadOnlyDocumentOptions options)
        {
            KdlReadOnlyDocument document = KdlReadOnlyDocument.ParseValue(kdl, options);
            return document.RootElement;
        }

        /// <summary>
        ///   Attempts to parse one KDL value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <param name="element">Receives the parsed element.</param>
        /// <returns>
        ///   <see langword="true"/> if a value was read and parsed into a KdlElement;
        ///   <see langword="false"/> if the reader ran out of data while parsing.
        ///   All other situations result in an exception being thrown.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="KdlReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="KdlTokenType.PropertyName"/> or <see cref="KdlTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="KdlReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        ///
        ///   <para>
        ///     Upon completion of this method, <paramref name="reader"/> will be positioned at the
        ///     final token in the KDL value.  If an exception is thrown, or <see langword="false"/>
        ///     is returned, the reader is reset to the state it was in when the method was called.
        ///   </para>
        ///
        ///   <para>
        ///     This method makes a copy of the data the reader acted on, so there is no caller
        ///     requirement to maintain data integrity beyond the return of this method.
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The current <paramref name="reader"/> token does not start or represent a value.
        /// </exception>
        /// <exception cref="KdlException">
        ///   A value could not be read from the reader.
        /// </exception>
        public static bool TryParseValue(ref KdlReader reader, [NotNullWhen(true)] out KdlReadOnlyElement? element)
        {
            bool ret = KdlReadOnlyDocument.TryParseValue(ref reader, out KdlReadOnlyDocument? document, shouldThrow: false, useArrayPools: false);
            element = document?.RootElement;
            return ret;
        }
    }
}
