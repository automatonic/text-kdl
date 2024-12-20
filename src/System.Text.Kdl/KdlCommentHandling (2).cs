namespace System.Text.Kdl
{
    /// <summary>
    /// This enum defines the various ways the <see cref="KdlReader"/> can deal with comments.
    /// </summary>
    public enum KdlCommentHandling : byte
    {
        /// <summary>
        /// By default, do no allow comments within the JSON input.
        /// Comments are treated as invalid JSON if found and a
        /// <see cref="KdlException"/> is thrown.
        /// </summary>
        Disallow = 0,
        /// <summary>
        /// Allow comments within the JSON input and ignore them.
        /// The <see cref="KdlReader"/> will behave as if no comments were present.
        /// </summary>
        Skip = 1,
        /// <summary>
        /// Allow comments within the JSON input and treat them as valid tokens.
        /// While reading, the caller will be able to access the comment values.
        /// </summary>
        Allow = 2,
    }
}
