using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Represents a key for a <see cref="KdlNode"/>.
    /// </summary>
    [DebuggerDisplay("{Display,nq}")]
    public record KdlEntryKey
    {
        /// <summary>
        /// Gets the index of the entry, or null if it is not known.
        /// </summary>
        public int? EntryIndex { get; init; }

        /// <summary>
        /// Gets the index of the property, or null if it is not known.
        /// </summary>
        public int? PropertyIndex { get; init; }

        /// <summary>
        /// Gets the name of the property, or null if it is not known.
        /// </summary>
        public string? PropertyName { get; init; }

        /// <summary>
        /// Gets the index of the argument, or null if it is not known.
        /// </summary>
        public int? ArgumentIndex { get; init; }

        /// <summary>
        /// Creates a new instance of the <see cref="KdlEntryKey"/> record with only the specified entry index.
        /// </summary>
        /// <param name="entryIndex">The entry index</param>
        /// <remarks>
        /// Use this form to get the n-th entry of a node.
        /// </remarks>
        /// <returns>A new instance.</returns>
        public static KdlEntryKey ForEntry(int entryIndex) =>
            new(
                entryIndex: entryIndex,
                propertyIndex: null,
                propertyName: null,
                argumentIndex: null
            );

        /// <summary>
        /// Creates a new instance of the <see cref="KdlEntryKey"/> record with the specified property index.
        /// </summary>
        /// <param name="propertyIndex"></param>
        /// <param name="propertyName"></param>
        /// <remarks>
        /// Used by the system to enumerate the sorted properties of a node.
        /// </remarks>
        public static KdlEntryKey ForProperty(int propertyIndex) =>
            new(
                entryIndex: null,
                propertyIndex: propertyIndex,
                propertyName: null,
                argumentIndex: null
            );

        /// <summary>
        /// Creates a new instance of the <see cref="KdlEntryKey"/> record with the specified property name.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <remarks>
        /// Use this key when only the property name is known.
        /// </remarks>
        public static KdlEntryKey ForProperty(string propertyName) =>
            new(
                entryIndex: null,
                propertyIndex: null,
                propertyName: propertyName,
                argumentIndex: null
            );

        /// <summary>
        /// Creates a new instance of the <see cref="KdlEntryKey"/> record with the specified argument index.
        /// </summary>
        /// <param name="argumentIndex"></param>
        /// <remarks>
        /// Use this key when only the argument index is known.
        /// </remarks>
        public static KdlEntryKey ForArgument(int argumentIndex) =>
            new(
                entryIndex: null,
                propertyIndex: null,
                propertyName: null,
                argumentIndex: argumentIndex
            );

        /// <summary>
        /// Creates a new instance of the <see cref="KdlEntryKey"/> record with the specified entry, property index, and property name.
        /// </summary>
        /// <param name="entryIndex">The entry index</param>
        /// <param name="propertyIndex">The property index</param>
        /// <param name="propertyName">The property name</param>
        /// <remarks>
        /// Used by the system to enumerate the properties of a node.
        /// </remarks>
        internal static KdlEntryKey ForExistingProperty(
            int entryIndex,
            int propertyIndex,
            string propertyName
        ) =>
            new(
                entryIndex: entryIndex,
                propertyIndex: propertyIndex,
                propertyName: propertyName,
                argumentIndex: null
            );

        /// <summary>
        /// Creates a new instance of the <see cref="KdlEntryKey"/> record with the specified entry and argument index.
        /// </summary>
        /// <param name="entryIndex"></param>
        /// <param name="argumentIndex"></param>
        /// <remarks>
        /// Used by the system to enumerate the arguments of a node.
        /// </remarks>
        internal static KdlEntryKey ForExistingArgument(int entryIndex, int argumentIndex) =>
            new(
                entryIndex: entryIndex,
                propertyIndex: null,
                propertyName: null,
                argumentIndex: argumentIndex
            );

        /// <summary>
        /// Initializes a new instance of the <see cref="KdlEntryKey"/> class.
        /// </summary>
        /// <param name="entryIndex">The index of the entry, or null if it is not known.</param>
        /// <remarks>
        /// This constructor is intentially internal to limit the formulations of keys to the known valid ones.
        /// </remarks>
        internal KdlEntryKey(
            int? entryIndex,
            int? propertyIndex,
            string? propertyName,
            int? argumentIndex
        )
        {
            EntryIndex = entryIndex;
            PropertyIndex = propertyIndex;
            PropertyName = propertyName;
            ArgumentIndex = argumentIndex;
        }

        /// <summary>
        /// Implcit conversion from string to <see cref="KdlEntryKey"/>.
        /// </summary>
        public static implicit operator KdlEntryKey(string propertyName) =>
            ForProperty(propertyName);

        public bool IsForProperty => PropertyName is not null || PropertyIndex is not null;
        public bool IsForArgument => ArgumentIndex is not null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Display =>
            this switch
            {
                { EntryIndex: { } ei } => this switch
                {
                    { PropertyIndex: { } pi, PropertyName: { } pn } => $"#{ei} {pn}@{pi}=",
                    { PropertyIndex: { } pi } => $"3{ei} @{pi}=",
                    { PropertyName: { } pn } => $"#{ei} {pn}=",
                    { ArgumentIndex: { } ai } => $"#{ei} [{ai}]",
                    _ => $"#{ei}",
                },
                { PropertyIndex: { } pi, PropertyName: { } pn } => $"{pn}@{pi}=",
                { PropertyIndex: { } pi } => $"@{pi}=",
                { PropertyName: { } pn } => $"{pn}=",
                { ArgumentIndex: { } ai } => $"[{ai}]",
                _ => "Unknown",
            };
    }
}
