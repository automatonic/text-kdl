using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Automatonic.Text.Kdl.RandomAccess
{
    /// <summary>
    ///   Represents a specific KDL value within a <see cref="KdlReadOnlyDocument"/>.
    /// </summary>
    /// <remarks>
    /// This is apparently borrowed from HTML/XML where an "Element" is everything within the start/end tag. This is distinct from
    /// "Node" in that is the top level structure of KDL and "node" is the graph theory API that we have aliased as "DocumentNode" to differentiate the semantics. 
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct KdlReadOnlyElement
    {
        private readonly KdlReadOnlyDocument _parent;
        private readonly int _idx;

        internal KdlReadOnlyElement(KdlReadOnlyDocument parent, int idx)
        {
            // parent is usually not null, but the Current property
            // on the enumerators (when initialized as `default`) can
            // get here with a null.
            Debug.Assert(idx >= 0);

            _parent = parent;
            _idx = idx;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private KdlTokenType TokenType => _parent?.GetKdlTokenType(_idx) ?? KdlTokenType.None;
        /// <summary>
        ///   The <see cref="KdlValueKind"/> that the value is.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public KdlValueKind ValueKind => TokenType.ToValueKind();

        /// <summary>
        ///   Get the value at a specified index when the current value is a
        ///   <see cref="KdlValueKind.Node"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        ///   <paramref name="index"/> is not in the range [0, <see cref="GetEntryCount"/>()).
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public KdlReadOnlyElement this[int index]
        {
            get
            {
                CheckValidInstance();

                return _parent.GetArrayIndexElement(_idx, index);
            }
        }

        /// <summary>
        ///   Get the number of entries contained within the current node value.
        /// </summary>
        /// <returns>The number of entries contained within the current node value.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public int GetEntryCount()
        {
            CheckValidInstance();

            //return _parent.GetPropertyCount(_idx);
            return _parent.GetArrayLength(_idx);
        }

        /// <summary>
        ///   Gets a <see cref="KdlReadOnlyElement"/> representing the value of a required property identified
        ///   by <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        ///   Property name matching is performed as an ordinal, case-sensitive, comparison.
        ///
        ///   If a property is defined multiple times for the same object, the last such definition is
        ///   what is matched.
        /// </remarks>
        /// <param name="propertyName">Name of the property whose value to return.</param>
        /// <returns>
        ///   A <see cref="KdlReadOnlyElement"/> representing the value of the requested property.
        /// </returns>
        /// <seealso cref="EnumerateNode"/>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///   No property was found with the requested name.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public KdlReadOnlyElement GetProperty(string propertyName)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            if (TryGetProperty(propertyName, out KdlReadOnlyElement property))
            {
                return property;
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///   Gets a <see cref="KdlReadOnlyElement"/> representing the value of a required property identified
        ///   by <paramref name="propertyName"/>.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Property name matching is performed as an ordinal, case-sensitive, comparison.
        ///   </para>
        ///
        ///   <para>
        ///     If a property is defined multiple times for the same object, the last such definition is
        ///     what is matched.
        ///   </para>
        /// </remarks>
        /// <param name="propertyName">Name of the property whose value to return.</param>
        /// <returns>
        ///   A <see cref="KdlReadOnlyElement"/> representing the value of the requested property.
        /// </returns>
        /// <seealso cref="EnumerateNode"/>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///   No property was found with the requested name.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public KdlReadOnlyElement GetProperty(ReadOnlySpan<char> propertyName)
        {
            if (TryGetProperty(propertyName, out KdlReadOnlyElement property))
            {
                return property;
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///   Gets a <see cref="KdlReadOnlyElement"/> representing the value of a required property identified
        ///   by <paramref name="utf8PropertyName"/>.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Property name matching is performed as an ordinal, case-sensitive, comparison.
        ///   </para>
        ///
        ///   <para>
        ///     If a property is defined multiple times for the same object, the last such definition is
        ///     what is matched.
        ///   </para>
        /// </remarks>
        /// <param name="utf8PropertyName">
        ///   The UTF-8 (with no Byte-Order-Mark (BOM)) representation of the name of the property to return.
        /// </param>
        /// <returns>
        ///   A <see cref="KdlReadOnlyElement"/> representing the value of the requested property.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///   No property was found with the requested name.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="EnumerateNode"/>
        public KdlReadOnlyElement GetProperty(ReadOnlySpan<byte> utf8PropertyName)
        {
            if (TryGetProperty(utf8PropertyName, out KdlReadOnlyElement property))
            {
                return property;
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///   Looks for a property named <paramref name="propertyName"/> in the current object, returning
        ///   whether or not such a property existed. When the property exists <paramref name="value"/>
        ///   is assigned to the value of that property.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Property name matching is performed as an ordinal, case-sensitive, comparison.
        ///   </para>
        ///
        ///   <para>
        ///     If a property is defined multiple times for the same object, the last such definition is
        ///     what is matched.
        ///   </para>
        /// </remarks>
        /// <param name="propertyName">Name of the property to find.</param>
        /// <param name="value">Receives the value of the located property.</param>
        /// <returns>
        ///   <see langword="true"/> if the property was found, <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="EnumerateNode"/>
        public bool TryGetProperty(string propertyName, out KdlReadOnlyElement value)
        {
            if (propertyName is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyName));
            }

            return TryGetProperty(propertyName.AsSpan(), out value);
        }

        /// <summary>
        ///   Looks for a property named <paramref name="propertyName"/> in the current object, returning
        ///   whether or not such a property existed. When the property exists <paramref name="value"/>
        ///   is assigned to the value of that property.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Property name matching is performed as an ordinal, case-sensitive, comparison.
        ///   </para>
        ///
        ///   <para>
        ///     If a property is defined multiple times for the same object, the last such definition is
        ///     what is matched.
        ///   </para>
        /// </remarks>
        /// <param name="propertyName">Name of the property to find.</param>
        /// <param name="value">Receives the value of the located property.</param>
        /// <returns>
        ///   <see langword="true"/> if the property was found, <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="EnumerateNode"/>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetProperty(ReadOnlySpan<char> propertyName, out KdlReadOnlyElement value)
        {
            CheckValidInstance();

            return _parent.TryGetNamedPropertyValue(_idx, propertyName, out value);
        }

        /// <summary>
        ///   Looks for a property named <paramref name="utf8PropertyName"/> in the current object, returning
        ///   whether or not such a property existed. When the property exists <paramref name="value"/>
        ///   is assigned to the value of that property.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Property name matching is performed as an ordinal, case-sensitive, comparison.
        ///   </para>
        ///
        ///   <para>
        ///     If a property is defined multiple times for the same object, the last such definition is
        ///     what is matched.
        ///   </para>
        /// </remarks>
        /// <param name="utf8PropertyName">
        ///   The UTF-8 (with no Byte-Order-Mark (BOM)) representation of the name of the property to return.
        /// </param>
        /// <param name="value">Receives the value of the located property.</param>
        /// <returns>
        ///   <see langword="true"/> if the property was found, <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="EnumerateNode"/>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetProperty(ReadOnlySpan<byte> utf8PropertyName, out KdlReadOnlyElement value)
        {
            CheckValidInstance();

            return _parent.TryGetNamedPropertyValue(_idx, utf8PropertyName, out value);
        }

        /// <summary>
        ///   Gets the value of the element as a <see cref="bool"/>.
        /// </summary>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>The value of the element as a <see cref="bool"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is neither <see cref="KdlValueKind.True"/> or
        ///   <see cref="KdlValueKind.False"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool GetBoolean()
        {
            // CheckValidInstance is redundant.  Asking for the type will
            // return None, which then throws the same exception in the return statement.

            KdlTokenType type = TokenType;

            return
                type == KdlTokenType.True || (type != KdlTokenType.False && ThrowKdlElementWrongTypeException(type));

            static bool ThrowKdlElementWrongTypeException(KdlTokenType actualType) => throw ThrowHelper.GetKdlElementWrongTypeException(nameof(Boolean), actualType.ToValueKind());
        }

        /// <summary>
        ///   Gets the value of the element as a <see cref="string"/>.
        /// </summary>
        /// <remarks>
        ///   This method does not create a string representation of values other than KDL strings.
        /// </remarks>
        /// <returns>The value of the element as a <see cref="string"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is neither <see cref="KdlValueKind.String"/> nor <see cref="KdlValueKind.Null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="ToString"/>
        public string? GetString()
        {
            CheckValidInstance();

            return _parent.GetString(_idx, KdlTokenType.String);
        }

        /// <summary>
        ///   Attempts to represent the current KDL string as bytes assuming it is Base64 encoded.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///  This method does not create a byte[] representation of values other than base 64 encoded KDL strings.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the entire token value is encoded as valid Base64 text and can be successfully decoded to bytes.
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetBytesFromBase64([NotNullWhen(true)] out byte[]? value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the value of the element as bytes.
        /// </summary>
        /// <remarks>
        ///   This method does not create a byte[] representation of values other than Base64 encoded KDL strings.
        /// </remarks>
        /// <returns>The value decode to bytes.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value is not encoded as Base64 text and hence cannot be decoded to bytes.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="ToString"/>
        public byte[] GetBytesFromBase64()
        {
            if (!TryGetBytesFromBase64(out byte[]? value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as an <see cref="sbyte"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as an <see cref="sbyte"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public bool TryGetSByte(out sbyte value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as an <see cref="sbyte"/>.
        /// </summary>
        /// <returns>The current KDL number as an <see cref="sbyte"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as an <see cref="sbyte"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public sbyte GetSByte()
        {
            if (TryGetSByte(out sbyte value))
            {
                return value;
            }

            throw new FormatException();
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="byte"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetByte(out byte value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="byte"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="byte"/>.</returns>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="byte"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public byte GetByte()
        {
            if (TryGetByte(out byte value))
            {
                return value;
            }

            throw new FormatException();
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as an <see cref="short"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as an <see cref="short"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetInt16(out short value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as an <see cref="short"/>.
        /// </summary>
        /// <returns>The current KDL number as an <see cref="short"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as an <see cref="short"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public short GetInt16()
        {
            if (TryGetInt16(out short value))
            {
                return value;
            }

            throw new FormatException();
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="ushort"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public bool TryGetUInt16(out ushort value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="ushort"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="ushort"/>.</returns>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="ushort"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public ushort GetUInt16()
        {
            if (TryGetUInt16(out ushort value))
            {
                return value;
            }

            throw new FormatException();
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as an <see cref="int"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as an <see cref="int"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetInt32(out int value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as an <see cref="int"/>.
        /// </summary>
        /// <returns>The current KDL number as an <see cref="int"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as an <see cref="int"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public int GetInt32()
        {
            if (!TryGetInt32(out int value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="uint"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public bool TryGetUInt32(out uint value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="uint"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="uint"/>.</returns>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="uint"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public uint GetUInt32()
        {
            if (!TryGetUInt32(out uint value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="long"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="long"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetInt64(out long value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="long"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="long"/>.</returns>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="long"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public long GetInt64()
        {
            if (!TryGetInt64(out long value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="ulong"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public bool TryGetUInt64(out ulong value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="ulong"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="ulong"/>.</returns>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="ulong"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        [CLSCompliant(false)]
        public ulong GetUInt64()
        {
            if (!TryGetUInt64(out ulong value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="double"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   <para>
        ///     This method does not parse the contents of a KDL string value.
        ///   </para>
        ///
        ///   <para>
        ///     On .NET Core this method does not return <see langword="false"/> for values larger than
        ///     <see cref="double.MaxValue"/> (or smaller than <see cref="double.MinValue"/>),
        ///     instead <see langword="true"/> is returned and <see cref="double.PositiveInfinity"/> (or
        ///     <see cref="double.NegativeInfinity"/>) is emitted.
        ///   </para>
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="double"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetDouble(out double value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="double"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="double"/>.</returns>
        /// <remarks>
        ///   <para>
        ///     This method does not parse the contents of a KDL string value.
        ///   </para>
        ///
        ///   <para>
        ///     On .NET Core this method returns <see cref="double.PositiveInfinity"/> (or
        ///     <see cref="double.NegativeInfinity"/>) for values larger than
        ///     <see cref="double.MaxValue"/> (or smaller than <see cref="double.MinValue"/>).
        ///   </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="double"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public double GetDouble()
        {
            if (!TryGetDouble(out double value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="float"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   <para>
        ///     This method does not parse the contents of a KDL string value.
        ///   </para>
        ///
        ///   <para>
        ///     On .NET Core this method does not return <see langword="false"/> for values larger than
        ///     <see cref="float.MaxValue"/> (or smaller than <see cref="float.MinValue"/>),
        ///     instead <see langword="true"/> is returned and <see cref="float.PositiveInfinity"/> (or
        ///     <see cref="float.NegativeInfinity"/>) is emitted.
        ///   </para>
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="float"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetSingle(out float value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="float"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="float"/>.</returns>
        /// <remarks>
        ///   <para>
        ///     This method does not parse the contents of a KDL string value.
        ///   </para>
        ///
        ///   <para>
        ///     On .NET Core this method returns <see cref="float.PositiveInfinity"/> (or
        ///     <see cref="float.NegativeInfinity"/>) for values larger than
        ///     <see cref="float.MaxValue"/> (or smaller than <see cref="float.MinValue"/>).
        ///   </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="float"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public float GetSingle()
        {
            if (!TryGetSingle(out float value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL number as a <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the number can be represented as a <see cref="decimal"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="GetRawText"/>
        public bool TryGetDecimal(out decimal value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the current KDL number as a <see cref="decimal"/>.
        /// </summary>
        /// <returns>The current KDL number as a <see cref="decimal"/>.</returns>
        /// <remarks>
        ///   This method does not parse the contents of a KDL string value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Number"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="decimal"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="GetRawText"/>
        public decimal GetDecimal()
        {
            if (!TryGetDecimal(out decimal value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL string as a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not create a DateTime representation of values other than KDL strings.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the string can be represented as a <see cref="DateTime"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetDateTime(out DateTime value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the value of the element as a <see cref="DateTime"/>.
        /// </summary>
        /// <remarks>
        ///   This method does not create a DateTime representation of values other than KDL strings.
        /// </remarks>
        /// <returns>The value of the element as a <see cref="DateTime"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="DateTime"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="ToString"/>
        public DateTime GetDateTime()
        {
            if (!TryGetDateTime(out DateTime value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL string as a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not create a DateTimeOffset representation of values other than KDL strings.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the string can be represented as a <see cref="DateTimeOffset"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetDateTimeOffset(out DateTimeOffset value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the value of the element as a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <remarks>
        ///   This method does not create a DateTimeOffset representation of values other than KDL strings.
        /// </remarks>
        /// <returns>The value of the element as a <see cref="DateTimeOffset"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="DateTimeOffset"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="ToString"/>
        public DateTimeOffset GetDateTimeOffset()
        {
            if (!TryGetDateTimeOffset(out DateTimeOffset value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        ///   Attempts to represent the current KDL string as a <see cref="Guid"/>.
        /// </summary>
        /// <param name="value">Receives the value.</param>
        /// <remarks>
        ///   This method does not create a Guid representation of values other than KDL strings.
        /// </remarks>
        /// <returns>
        ///   <see langword="true"/> if the string can be represented as a <see cref="Guid"/>,
        ///   <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public bool TryGetGuid(out Guid value)
        {
            CheckValidInstance();

            return _parent.TryGetValue(_idx, out value);
        }

        /// <summary>
        ///   Gets the value of the element as a <see cref="Guid"/>.
        /// </summary>
        /// <remarks>
        ///   This method does not create a Guid representation of values other than KDL strings.
        /// </remarks>
        /// <returns>The value of the element as a <see cref="Guid"/>.</returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   The value cannot be represented as a <see cref="Guid"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        /// <seealso cref="ToString"/>
        public Guid GetGuid()
        {
            if (!TryGetGuid(out Guid value))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        internal string GetPropertyName()
        {
            CheckValidInstance();

            return _parent.GetNameOfPropertyValue(_idx);
        }

        internal ReadOnlySpan<byte> GetPropertyNameRaw()
        {
            CheckValidInstance();

            return _parent.GetPropertyNameRaw(_idx);
        }

        /// <summary>
        ///   Gets the original input data backing this value, returning it as a <see cref="string"/>.
        /// </summary>
        /// <returns>
        ///   The original input data backing this value, returning it as a <see cref="string"/>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public string GetRawText()
        {
            CheckValidInstance();

            return _parent.GetRawValueAsString(_idx);
        }

        internal ReadOnlyMemory<byte> GetRawValue()
        {
            CheckValidInstance();

            return _parent.GetRawValue(_idx, includeQuotes: true);
        }

        internal string GetPropertyRawText()
        {
            CheckValidInstance();

            return _parent.GetPropertyRawValueAsString(_idx);
        }

        internal string GetEntryRawText()
        {
            CheckValidInstance();

            return _parent.GetPropertyRawValueAsString(_idx);
        }

        internal bool ValueIsEscaped
        {
            get
            {
                CheckValidInstance();

                return _parent.ValueIsEscaped(_idx, isPropertyName: false);
            }
        }

        internal ReadOnlySpan<byte> ValueSpan
        {
            get
            {
                CheckValidInstance();

                return _parent.GetRawValue(_idx, includeQuotes: false).Span;
            }
        }

        /// <summary>
        /// Compares the values of two <see cref="KdlReadOnlyElement"/> values for equality, including the values of all descendant elements.
        /// </summary>
        /// <param name="element1">The first <see cref="KdlReadOnlyElement"/> to compare.</param>
        /// <param name="element2">The second <see cref="KdlReadOnlyElement"/> to compare.</param>
        /// <returns><see langword="true"/> if the two values are equal; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// Deep equality of two KDL values is defined as follows:
        /// <list type="bullet">
        /// <item>KDL values of different kinds are not equal.</item>
        /// <item>KDL constants <see langword="null"/>, <see langword="false"/>, and <see langword="true"/> only equal themselves.</item>
        /// <item>KDL numbers are equal if and only if they have they have equivalent decimal representations, with no rounding being used.</item>
        /// <item>KDL strings are equal if and only if they are equal using ordinal string comparison.</item>
        /// <item>KDL arrays are equal if and only if they are of equal length and each of their elements are pairwise equal.</item>
        /// <item>
        ///     KDL objects are equal if and only if they have the same number of properties and each property in the first object
        ///     has a corresponding property in the second object with the same name and equal value. The order of properties is not
        ///     significant, with the exception of repeated properties that must be specified in the same order (with interleaving allowed).
        /// </item>
        /// </list>
        /// </remarks>
        public static bool DeepEquals(KdlReadOnlyElement element1, KdlReadOnlyElement element2)
        {
            if (!StackHelper.TryEnsureSufficientExecutionStack())
            {
                ThrowHelper.ThrowInsufficientExecutionStackException_KdlElementDeepEqualsInsufficientExecutionStack();
            }

            element1.CheckValidInstance();
            element2.CheckValidInstance();

            KdlValueKind kind = element1.ValueKind;
            if (kind != element2.ValueKind)
            {
                return false;
            }

            switch (kind)
            {
                case KdlValueKind.Null or KdlValueKind.False or KdlValueKind.True:
                    return true;

                case KdlValueKind.Number:
                    return KdlHelpers.AreEqualKdlNumbers(element1.GetRawValue().Span, element2.GetRawValue().Span);

                case KdlValueKind.String:
                    if (element2.ValueIsEscaped)
                    {
                        if (element1.ValueIsEscaped)
                        {
                            // Need to unescape and compare both inputs.
                            return KdlReaderHelper.UnescapeAndCompareBothInputs(element1.ValueSpan, element2.ValueSpan);
                        }

                        // Swap values so that unescaping is handled by the LHS.
                        (element1, element2) = (element2, element1);
                    }

                    return element1.ValueEquals(element2.ValueSpan);

                case KdlValueKind.Node:
                    if (element1.GetEntryCount() != element2.GetEntryCount())
                    {
                        return false;
                    }

                    ArrayEnumerator arrayEnumerator2 = element2.EnumerateArray();
                    foreach (KdlReadOnlyElement e1 in element1.EnumerateArray())
                    {
                        bool success = arrayEnumerator2.MoveNext();
                        Debug.Assert(success, "enumerators must have matching length");

                        if (!DeepEquals(e1, arrayEnumerator2.Current))
                        {
                            return false;
                        }
                    }

                    Debug.Assert(!arrayEnumerator2.MoveNext());
                    return true;

                default:
                    Debug.Assert(kind is KdlValueKind.Node);

                    int count = element1.GetEntryCount();
                    if (count != element2.GetEntryCount())
                    {
                        return false;
                    }

                    NodeEnumerator objectEnumerator1 = element1.EnumerateNode();
                    NodeEnumerator objectEnumerator2 = element2.EnumerateNode();

                    // Two KDL objects are considered equal if they define the same set of properties.
                    // Start optimistically with pairwise comparison, but fall back to unordered
                    // comparison as soon as a mismatch is encountered.

                    while (objectEnumerator1.MoveNext())
                    {
                        bool success = objectEnumerator2.MoveNext();
                        Debug.Assert(success, "enumerators should have matching lengths");

                        IKdlEntry prop1 = objectEnumerator1.Current;
                        IKdlEntry prop2 = objectEnumerator2.Current;

                        if (!NameEquals(prop1, prop2))
                        {
                            // We have our first mismatch, fall back to unordered comparison.
                            return UnorderedObjectDeepEquals(objectEnumerator1, objectEnumerator2, remainingProps: count);
                        }

                        if (!DeepEquals(prop1.Value, prop2.Value))
                        {
                            return false;
                        }

                        count--;
                    }

                    Debug.Assert(!objectEnumerator2.MoveNext());
                    return true;

                    static bool UnorderedObjectDeepEquals(NodeEnumerator objectEnumerator1, NodeEnumerator objectEnumerator2, int remainingProps)
                    {
                        // KdlElement objects allow duplicate property names, which is optional per the KDL RFC.
                        // Even though this implementation of equality does not take property ordering into account,
                        // repeated property names must be specified in the same order (although they may be interleaved).
                        // This is to preserve a degree of coherence with KDL serialization, where either the first
                        // or last occurrence of a repeated property name is used. It also simplifies the implementation
                        // and keeps it at O(n + m) complexity.

                        Dictionary<string, ValueQueue<KdlReadOnlyElement>> properties2 = new(capacity: remainingProps, StringComparer.Ordinal);
                        do
                        {
                            IKdlEntry prop2 = objectEnumerator2.Current;
#if NET
                            ref ValueQueue<KdlReadOnlyElement> values = ref CollectionsMarshal.GetValueRefOrAddDefault(properties2, prop2.Name, out bool _);
#else
                            properties2.TryGetValue(prop2.Name, out ValueQueue<KdlElement> values);
#endif
                            values.Enqueue(prop2.Value);
#if !NET
                            properties2[prop2.Name] = values;
#endif
                        }
                        while (objectEnumerator2.MoveNext());

                        do
                        {
                            IKdlEntry prop = objectEnumerator1.Current;
#if NET
                            ref ValueQueue<KdlReadOnlyElement> values = ref CollectionsMarshal.GetValueRefOrAddDefault(properties2, prop.Name, out bool exists);
#else
                            bool exists = properties2.TryGetValue(prop.Name, out ValueQueue<KdlElement> values);
#endif
                            if (!exists || !values.TryDequeue(out KdlReadOnlyElement value) || !DeepEquals(prop.Value, value))
                            {
                                return false;
                            }
#if !NET
                            properties2[prop.Name] = values;
#endif
                        }
                        while (objectEnumerator1.MoveNext());

                        return true;
                    }

                    static bool NameEquals(IKdlEntry left, IKdlEntry right)
                    {
                        if (right.NameIsEscaped)
                        {
                            if (left.NameIsEscaped)
                            {
                                // Need to unescape and compare both inputs.
                                return KdlReaderHelper.UnescapeAndCompareBothInputs(left.NameSpan, right.NameSpan);
                            }

                            // Swap values so that unescaping is handled by the LHS
                            (left, right) = (right, left);
                        }

                        return left.NameEquals(right.NameSpan);
                    }
            }
        }

        /// <summary>
        ///   Compares <paramref name="text" /> to the string value of this element.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the string value of this element matches <paramref name="text"/>,
        ///   <see langword="false" /> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <remarks>
        ///   This method is functionally equal to doing an ordinal comparison of <paramref name="text" /> and
        ///   the result of calling <see cref="GetString" />, but avoids creating the string instance.
        /// </remarks>
        public bool ValueEquals(string? text)
        {
            // CheckValidInstance is done in the helper

            if (TokenType == KdlTokenType.Null)
            {
                return text == null;
            }

            return TextEqualsHelper(text.AsSpan(), isPropertyName: false);
        }

        /// <summary>
        ///   Compares the text represented by <paramref name="utf8Text" /> to the string value of this element.
        /// </summary>
        /// <param name="utf8Text">The UTF-8 encoded text to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the string value of this element has the same UTF-8 encoding as
        ///   <paramref name="utf8Text" />, <see langword="false" /> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <remarks>
        ///   This method is functionally equal to doing an ordinal comparison of the string produced by UTF-8 decoding
        ///   <paramref name="utf8Text" /> with the result of calling <see cref="GetString" />, but avoids creating the
        ///   string instances.
        /// </remarks>
        public bool ValueEquals(ReadOnlySpan<byte> utf8Text)
        {
            // CheckValidInstance is done in the helper

            if (TokenType == KdlTokenType.Null)
            {
                // This is different than Length == 0, in that it tests true for null, but false for ""
                return Unsafe.IsNullRef(ref MemoryMarshal.GetReference(utf8Text));
            }

            return TextEqualsHelper(utf8Text, isPropertyName: false, shouldUnescape: true);
        }

        /// <summary>
        ///   Compares <paramref name="text" /> to the string value of this element.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>
        ///   <see langword="true" /> if the string value of this element matches <paramref name="text"/>,
        ///   <see langword="false" /> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.String"/>.
        /// </exception>
        /// <remarks>
        ///   This method is functionally equal to doing an ordinal comparison of <paramref name="text" /> and
        ///   the result of calling <see cref="GetString" />, but avoids creating the string instance.
        /// </remarks>
        public bool ValueEquals(ReadOnlySpan<char> text)
        {
            // CheckValidInstance is done in the helper

            if (TokenType == KdlTokenType.Null)
            {
                // This is different than Length == 0, in that it tests true for null, but false for ""
                return Unsafe.IsNullRef(ref MemoryMarshal.GetReference(text));
            }

            return TextEqualsHelper(text, isPropertyName: false);
        }

        internal bool TextEqualsHelper(ReadOnlySpan<byte> utf8Text, bool isPropertyName, bool shouldUnescape)
        {
            CheckValidInstance();

            return _parent.TextEquals(_idx, utf8Text, isPropertyName, shouldUnescape);
        }

        internal bool TextEqualsHelper(ReadOnlySpan<char> text, bool isPropertyName)
        {
            CheckValidInstance();

            return _parent.TextEquals(_idx, text, isPropertyName);
        }

        internal bool ValueIsEscapedHelper(bool isPropertyName)
        {
            CheckValidInstance();

            return _parent.ValueIsEscaped(_idx, isPropertyName);
        }

        /// <summary>
        ///   Write the element into the provided writer as a KDL value.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="writer"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is <see cref="KdlValueKind.Undefined"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public void WriteTo(KdlWriter writer)
        {
            if (writer is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(writer));
            }

            CheckValidInstance();

            _parent.WriteElementTo(_idx, writer);
        }

        internal void WritePropertyNameTo(KdlWriter writer)
        {
            CheckValidInstance();

            _parent.WritePropertyName(_idx, writer);
        }

        /// <summary>
        ///   Get an enumerator to enumerate the values in the KDL array represented by this KdlElement.
        /// </summary>
        /// <returns>
        ///   An enumerator to enumerate the values in the KDL array represented by this KdlElement.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public ArrayEnumerator EnumerateArray()
        {
            CheckValidInstance();

            KdlTokenType tokenType = TokenType;

            if (tokenType != KdlTokenType.StartArray)
            {
                ThrowHelper.ThrowKdlElementWrongTypeException(KdlTokenType.StartArray, tokenType);
            }

            return new ArrayEnumerator(this);
        }

        /// <summary>
        ///   Get an enumerator to enumerate the properties in the KDL object represented by this KdlElement.
        /// </summary>
        /// <returns>
        ///   An enumerator to enumerate the properties in the KDL object represented by this KdlElement.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   This value's <see cref="ValueKind"/> is not <see cref="KdlValueKind.Node"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public NodeEnumerator EnumerateNode()
        {
            CheckValidInstance();

            KdlTokenType tokenType = TokenType;

            if (tokenType != KdlTokenType.StartObject)
            {
                ThrowHelper.ThrowKdlElementWrongTypeException(KdlTokenType.StartObject, tokenType);
            }

            return new NodeEnumerator(this);
        }

        /// <summary>
        ///   Gets a string representation for the current value appropriate to the value type.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     For KdlElement built from <see cref="KdlReadOnlyDocument"/>:
        ///   </para>
        ///
        ///   <para>
        ///     For <see cref="KdlValueKind.Null"/>, <see cref="string.Empty"/> is returned.
        ///   </para>
        ///
        ///   <para>
        ///     For <see cref="KdlValueKind.True"/>, <see cref="bool.TrueString"/> is returned.
        ///   </para>
        ///
        ///   <para>
        ///     For <see cref="KdlValueKind.False"/>, <see cref="bool.FalseString"/> is returned.
        ///   </para>
        ///
        ///   <para>
        ///     For <see cref="KdlValueKind.String"/>, the value of <see cref="GetString"/>() is returned.
        ///   </para>
        ///
        ///   <para>
        ///     For other types, the value of <see cref="GetRawText"/>() is returned.
        ///   </para>
        /// </remarks>
        /// <returns>
        ///   A string representation for the current value appropriate to the value type.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///   The parent <see cref="KdlReadOnlyDocument"/> has been disposed.
        /// </exception>
        public override string ToString()
        {
            switch (TokenType)
            {
                case KdlTokenType.None:
                case KdlTokenType.Null:
                    return string.Empty;
                case KdlTokenType.True:
                    return bool.TrueString;
                case KdlTokenType.False:
                    return bool.FalseString;
                case KdlTokenType.Number:
                case KdlTokenType.StartArray:
                case KdlTokenType.StartObject:
                {
                    // null parent should have hit the None case
                    Debug.Assert(_parent != null);
                    return _parent.GetRawValueAsString(_idx);
                }
                case KdlTokenType.String:
                    return GetString()!;
                case KdlTokenType.Comment:
                case KdlTokenType.EndArray:
                case KdlTokenType.EndObject:
                default:
                    Debug.Fail($"No handler for {nameof(KdlTokenType)}.{TokenType}");
                    return string.Empty;
            }
        }

        /// <summary>
        ///   Get a KdlElement which can be safely stored beyond the lifetime of the
        ///   original <see cref="KdlReadOnlyDocument"/>.
        /// </summary>
        /// <returns>
        ///   A KdlElement which can be safely stored beyond the lifetime of the
        ///   original <see cref="KdlReadOnlyDocument"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If this KdlElement is itself the output of a previous call to Clone, or
        ///     a value contained within another KdlElement which was the output of a previous
        ///     call to Clone, this method results in no additional memory allocation.
        ///   </para>
        /// </remarks>
        public KdlReadOnlyElement Clone()
        {
            CheckValidInstance();

            if (!_parent.IsDisposable)
            {
                return this;
            }

            return _parent.CloneElement(_idx);
        }

        private void CheckValidInstance()
        {
            if (_parent == null)
            {
                throw new InvalidOperationException();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"ValueKind = {ValueKind} : \"{ToString()}\"";
    }
}
