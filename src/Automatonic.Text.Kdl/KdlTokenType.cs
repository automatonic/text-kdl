namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// This enum defines the various KDL tokens that make up a KDL text and is used by
    /// the <see cref="KdlReader"/> when moving from one token to the next.
    /// The <see cref="KdlReader"/> starts at 'None' by default. The 'Comment' enum value
    /// is only ever reached in a specific <see cref="KdlReader"/> mode and is not
    /// reachable by default.
    /// </summary>
    public enum KdlTokenType : byte
    {
        // Do not re-order.
        // We rely on the ordering to quickly check things like IsTokenTypePrimitive

        /// <summary>
        ///   Indicates that there is no value (as distinct from <see cref="Null"/>).
        /// </summary>
        /// <remarks>
        ///   This is the default token type if no data has been read by the <see cref="KdlReader"/>.
        /// </remarks>
        None,

        /// <summary>
        ///   Indicates the beginning of a KDL type annotation.
        /// </summary>
        StartTypeAnnotation,

        /// <summary>
        ///   Indicates the end of a KDL type annotation.
        /// </summary>
        EndTypeAnnotation,

        /// <summary>
        ///   Indicates that the token type is the start of a KDL node's children block.
        /// </summary>
        StartChildrenBlock,

        /// <summary>
        ///   Indicates that the token type is the end of a KDL node's children block.
        /// </summary>
        EndChildrenBlock,

        /// <summary>
        ///   Indicates that the token type is the start of a KDL array.
        /// </summary>
        StartArray,

        /// <summary>
        ///   Indicates that the token type is the end of a KDL array.
        /// </summary>
        EndArray,

        /// <summary>
        ///   Indicates that the token type is a KDL property name.
        /// </summary>
        PropertyName,

        /// <summary>
        ///   Indicates that the token type is the comment string.
        /// </summary>
        Comment,

        /// <summary>
        ///   Indicates that the token type is a KDL string.
        /// </summary>
        String,

        /// <summary>
        ///   Indicates that the token type is a KDL number.
        /// </summary>
        Number,

        /// <summary>
        ///   Indicates that the token type is the KDL literal <c>true</c>.
        /// </summary>
        True,

        /// <summary>
        ///   Indicates that the token type is the KDL literal <c>false</c>.
        /// </summary>
        False,

        /// <summary>
        ///   Indicates that the token type is the KDL literal <c>null</c>.
        /// </summary>
        Null,
    }
}
