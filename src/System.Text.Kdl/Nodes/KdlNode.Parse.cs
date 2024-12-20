// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Kdl.Serialization.Converters;
using System.Threading;
using System.Threading.Tasks;

namespace System.Text.Kdl.Nodes
{
    public abstract partial class KdlNode
    {
        /// <summary>
        ///   Parses one KDL value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <param name="nodeOptions">Options to control the behavior.</param>
        /// <returns>
        ///   The <see cref="KdlNode"/> from the reader.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="KdlReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="KdlTokenType.PropertyName"/> or <see cref="KdlTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="KdlReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        ///   <para>
        ///     Upon completion of this method, <paramref name="reader"/> will be positioned at the
        ///     final token in the KDL value.  If an exception is thrown, the reader is reset to the state it was in when the method was called.
        ///   </para>
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
        public static KdlNode? Parse(
            ref KdlReader reader,
            KdlNodeOptions? nodeOptions = null)
        {
            KdlElement element = KdlElement.ParseValue(ref reader);
            return KdlNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        ///   Parses text representing a single KDL value.
        /// </summary>
        /// <param name="kdl">KDL text to parse.</param>
        /// <param name="nodeOptions">Options to control the node behavior after parsing.</param>
        /// <param name="documentOptions">Options to control the document behavior during parsing.</param>
        /// <returns>
        ///   A <see cref="KdlNode"/> representation of the KDL value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="kdl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KdlException">
        ///   <paramref name="kdl"/> does not represent a valid single KDL value.
        /// </exception>
        public static KdlNode? Parse(
            [StringSyntax(StringSyntaxAttribute.Kdl)] string kdl,
            KdlNodeOptions? nodeOptions = null,
            KdlDocumentOptions documentOptions = default(KdlDocumentOptions))
        {
            if (kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(kdl));
            }

            KdlElement element = KdlElement.ParseValue(kdl, documentOptions);
            return KdlNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        ///   Parses text representing a single KDL value.
        /// </summary>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="nodeOptions">Options to control the node behavior after parsing.</param>
        /// <param name="documentOptions">Options to control the document behavior during parsing.</param>
        /// <returns>
        ///   A <see cref="KdlNode"/> representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        public static KdlNode? Parse(
            ReadOnlySpan<byte> utf8Kdl,
            KdlNodeOptions? nodeOptions = null,
            KdlDocumentOptions documentOptions = default(KdlDocumentOptions))
        {
            KdlElement element = KdlElement.ParseValue(utf8Kdl, documentOptions);
            return KdlNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        ///   Parse a <see cref="Stream"/> as UTF-8 encoded data representing a single KDL value into a
        ///   <see cref="KdlNode"/>.  The Stream will be read to completion.
        /// </summary>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="nodeOptions">Options to control the node behavior after parsing.</param>
        /// <param name="documentOptions">Options to control the document behavior during parsing.</param>
        /// <returns>
        ///   A <see cref="KdlNode"/> representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        public static KdlNode? Parse(
            Stream utf8Kdl,
            KdlNodeOptions? nodeOptions = null,
            KdlDocumentOptions documentOptions = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlElement element = KdlElement.ParseValue(utf8Kdl, documentOptions);
            return KdlNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        ///   Parse a <see cref="Stream"/> as UTF-8 encoded data representing a single KDL value into a
        ///   <see cref="KdlNode"/>.  The Stream will be read to completion.
        /// </summary>
        /// <param name="utf8Kdl">KDL text to parse.</param>
        /// <param name="nodeOptions">Options to control the node behavior after parsing.</param>
        /// <param name="documentOptions">Options to control the document behavior during parsing.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        ///   A <see cref="Task"/> to produce a <see cref="KdlNode"/> representation of the KDL value.
        /// </returns>
        /// <exception cref="KdlException">
        ///   <paramref name="utf8Kdl"/> does not represent a valid single KDL value.
        /// </exception>
        public static async Task<KdlNode?> ParseAsync(
            Stream utf8Kdl,
            KdlNodeOptions? nodeOptions = null,
            KdlDocumentOptions documentOptions = default,
            CancellationToken cancellationToken = default)
        {
            if (utf8Kdl is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(utf8Kdl));
            }

            KdlDocument document = await KdlDocument.ParseAsyncCoreUnrented(utf8Kdl, documentOptions, cancellationToken).ConfigureAwait(false);
            return KdlNodeConverter.Create(document.RootElement, nodeOptions);
        }
    }
}
