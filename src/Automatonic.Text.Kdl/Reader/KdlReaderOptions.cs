using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Provides the ability for the user to define custom behavior when reading KDL.
    /// </summary>
    public struct KdlReaderOptions
    {
        internal const int DefaultMaxDepth = 64;

        private int _maxDepth;
        private KdlCommentHandling _commentHandling;

        /// <summary>
        /// Defines how the <see cref="KdlReader"/> should handle comments when reading through the KDL.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the comment handling enum is set to a value that is not supported (i.e. not within the <see cref="KdlCommentHandling"/> enum range).
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
                if (value > KdlCommentHandling.Allow)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_CommentEnumMustBeInRange(nameof(value));
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

        /// <summary>
        /// Defines whether the <see cref="KdlReader"/> should tolerate
        /// zero or more top-level KDL values that are whitespace separated.
        /// </summary>
        /// <remarks>
        /// By default, it's set to false, and <exception cref="KdlException"/> is thrown if trailing content is encountered after the first top-level KDL value.
        /// </remarks>
        public bool AllowMultipleValues { get; set; }
    }
}