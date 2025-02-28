namespace System.Text.Kdl
{
    /// <summary>
    ///   Specifies the data type of a KDL value.
    /// </summary>
    public enum KdlValueKind : byte
    {
        /// <summary>
        ///   Indicates that there is no value (as distinct from <see cref="Null"/>).
        /// </summary>
        Undefined,

        /// <summary>
        ///   Indicates that a value is a KDL node.
        /// </summary>
        Node,

        /// <summary>
        ///   Indicates that a value is a KDL string.
        /// </summary>
        String,

        /// <summary>
        ///   Indicates that a value is a KDL number.
        /// </summary>
        Number,

        /// <summary>
        ///   Indicates that a value is the KDL value <c>true</c>.
        /// </summary>
        True,

        /// <summary>
        ///   Indicates that a value is the KDL value <c>false</c>.
        /// </summary>
        False,

        /// <summary>
        ///   Indicates that a value is the KDL value <c>null</c>.
        /// </summary>
        Null,
    }
}
