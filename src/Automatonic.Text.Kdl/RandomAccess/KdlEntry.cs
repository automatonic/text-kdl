namespace Automatonic.Text.Kdl.RandomAccess
{
    /// <summary>
    ///   Represents a single property for a KDL object.
    /// </summary>
    public interface IKdlEntry
    {
        /// <summary>
        ///   The value of this property.
        /// </summary>
        public KdlReadOnlyElement Value { get; }

        /// <summary>
        ///   The name of this property.
        /// </summary>
        public string Name { get; }
        public bool NameIsEscaped { get; }
        public ReadOnlySpan<byte> NameSpan { get; }
        public KdlEntryKey Key { get; }

        /// <summary>
        ///   Compares <paramref name="text" /> to the name of this property.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the name of this property matches <paramref name="text"/>,
        ///   <see langword="false" /> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="Type"/> is not <see cref="KdlTokenType.PropertyName"/>.
        /// </exception>
        /// <remarks>
        ///   This method is functionally equal to doing an ordinal comparison of <paramref name="text" /> and
        ///   <see cref="Name" />, but can avoid creating the string instance.
        /// </remarks>
        public bool NameEquals(string? text);

        /// <summary>
        ///   Compares the text represented by <paramref name="utf8Text" /> to the name of this property.
        /// </summary>
        /// <param name="utf8Text">The UTF-8 encoded text to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the name of this property has the same UTF-8 encoding as
        ///   <paramref name="utf8Text" />, <see langword="false" /> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="Type"/> is not <see cref="KdlTokenType.PropertyName"/>.
        /// </exception>
        /// <remarks>
        ///   This method is functionally equal to doing an ordinal comparison of <paramref name="utf8Text" /> and
        ///   <see cref="Name" />, but can avoid creating the string instance.
        /// </remarks>
        public bool NameEquals(ReadOnlySpan<byte> utf8Text);

        /// <summary>
        ///   Compares <paramref name="text" /> to the name of this property.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the name of this property matches <paramref name="text"/>,
        ///   <see langword="false" /> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="Type"/> is not <see cref="KdlTokenType.PropertyName"/>.
        /// </exception>
        /// <remarks>
        ///   This method is functionally equal to doing an ordinal comparison of <paramref name="text" /> and
        ///   <see cref="Name" />, but can avoid creating the string instance.
        /// </remarks>
        public bool NameEquals(ReadOnlySpan<char> text);

        /// <summary>
        ///   Write the property into the provided writer as a named KDL object property.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="writer"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   This <see cref="Name"/>'s length is too large to be a KDL object property.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   This <see cref="Value"/>'s <see cref="KdlReadOnlyElement.ValueKind"/> would result in an invalid KDL.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>>
        public void WriteTo(KdlWriter writer);
    }
}
