using System.Diagnostics;

namespace System.Text.Kdl.RandomAccess
{
    /// <summary>
    /// Provides the ability for the user to define custom behavior when parsing KDL to create a <see cref="KdlReadOnlyDocument"/>.
    /// </summary>
    public struct KdlReadOnlyDocumentOptions
    {
        internal const int DefaultMaxDepth = 64;

        private int _maxDepth;
        private KdlCommentHandling _commentHandling;

        /// <summary>
        /// Defines how the <see cref="KdlReader"/> should handle comments when reading through the KDL.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the comment handling enum is set to a value that is not supported (or not within the <see cref="KdlCommentHandling"/> enum range).
        /// </exception>
        /// <remarks>
        /// By default <exception cref="KdlException"/> is thrown if a comment is encountered.
        /// </remarks>
        public KdlCommentHandling CommentHandling
        {
            readonly get => _commentHandling;
            set
            {
                Debug.Assert(value >= 0);
                if (value > KdlCommentHandling.Skip)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.KdlDocumentDoesNotSupportComments);
                }

                _commentHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading KDL, with the default (i.e. 0) indicating a max depth of 64.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the max depth is set to a negative value.
        /// </exception>
        /// <remarks>
        /// Reading past this depth will throw a <exception cref="KdlException"/>.
        /// </remarks>
        public int MaxDepth
        {
            readonly get => _maxDepth;
            set
            {
                if (value < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_MaxDepthMustBePositive(nameof(value));
                }

                _maxDepth = value;
            }
        }

        /// <summary>
        /// Defines whether an extra comma at the end of a list of KDL values in an object or array
        /// is allowed (and ignored) within the KDL payload being read.
        /// </summary>
        /// <remarks>
        /// By default, it's set to false, and <exception cref="KdlException"/> is thrown if a trailing comma is encountered.
        /// </remarks>
        public bool AllowTrailingCommas { get; set; }

        internal KdlReaderOptions GetReaderOptions()
        {
            return new KdlReaderOptions
            {
                AllowTrailingCommas = AllowTrailingCommas,
                CommentHandling = CommentHandling,
                MaxDepth = MaxDepth
            };
        }
    }
}
