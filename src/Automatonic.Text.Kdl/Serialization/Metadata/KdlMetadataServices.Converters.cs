using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization.Converters;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    public static partial class KdlMetadataServices
    {
        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="bool"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<bool> BooleanConverter => s_booleanConverter ??= new BooleanConverter();
        private static KdlConverter<bool>? s_booleanConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts byte array values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<byte[]?> ByteArrayConverter => s_byteArrayConverter ??= new ByteArrayConverter();
        private static KdlConverter<byte[]?>? s_byteArrayConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="byte"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<byte> ByteConverter => s_byteConverter ??= new ByteConverter();
        private static KdlConverter<byte>? s_byteConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="char"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<char> CharConverter => s_charConverter ??= new CharConverter();
        private static KdlConverter<char>? s_charConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="DateTime"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<DateTime> DateTimeConverter => s_dateTimeConverter ??= new DateTimeConverter();
        private static KdlConverter<DateTime>? s_dateTimeConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="DateTimeOffset"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<DateTimeOffset> DateTimeOffsetConverter => s_dateTimeOffsetConverter ??= new DateTimeOffsetConverter();
        private static KdlConverter<DateTimeOffset>? s_dateTimeOffsetConverter;

#if NET
        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="DateOnly"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<DateOnly> DateOnlyConverter => s_dateOnlyConverter ??= new DateOnlyConverter();
        private static KdlConverter<DateOnly>? s_dateOnlyConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="TimeOnly"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<TimeOnly> TimeOnlyConverter => s_timeOnlyConverter ??= new TimeOnlyConverter();
        private static KdlConverter<TimeOnly>? s_timeOnlyConverter;
#endif

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="decimal"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<decimal> DecimalConverter => s_decimalConverter ??= new DecimalConverter();
        private static KdlConverter<decimal>? s_decimalConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="double"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<double> DoubleConverter => s_doubleConverter ??= new DoubleConverter();
        private static KdlConverter<double>? s_doubleConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="Guid"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<Guid> GuidConverter => s_guidConverter ??= new GuidConverter();
        private static KdlConverter<Guid>? s_guidConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="short"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<short> Int16Converter => s_int16Converter ??= new Int16Converter();
        private static KdlConverter<short>? s_int16Converter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="int"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<int> Int32Converter => s_int32Converter ??= new Int32Converter();
        private static KdlConverter<int>? s_int32Converter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="long"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<long> Int64Converter => s_int64Converter ??= new Int64Converter();
        private static KdlConverter<long>? s_int64Converter;

#if NET
        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="Int128"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<Int128> Int128Converter => s_int128Converter ??= new Int128Converter();
        private static KdlConverter<Int128>? s_int128Converter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="UInt128"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        [CLSCompliant(false)]
        public static KdlConverter<UInt128> UInt128Converter => s_uint128Converter ??= new UInt128Converter();
        private static KdlConverter<UInt128>? s_uint128Converter;
#endif

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="KdlNode"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<KdlNode?> KdlArrayConverter => s_jsonArrayConverter ??= new KdlArrayConverter();
        private static KdlConverter<KdlNode?>? s_jsonArrayConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="KdlReadOnlyElement"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<KdlReadOnlyElement> KdlElementConverter => s_jsonElementConverter ??= new KdlElementConverter();
        private static KdlConverter<KdlReadOnlyElement>? s_jsonElementConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="KdlElement"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<KdlElement?> KdlNodeConverter => s_jsonNodeConverter ??= new KdlVertexConverter();
        private static KdlConverter<KdlElement?>? s_jsonNodeConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="KdlNode"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<KdlNode?> KdlObjectConverter => s_jsonObjectConverter ??= new KdlObjectConverter();
        private static KdlConverter<KdlNode?>? s_jsonObjectConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="KdlNode"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<KdlValue?> KdlValueConverter => s_jsonValueConverter ??= new KdlValueConverter();
        private static KdlConverter<KdlValue?>? s_jsonValueConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="KdlReadOnlyDocument"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<KdlReadOnlyDocument?> KdlDocumentConverter => s_jsonDocumentConverter ??= new KdlDocumentConverter();
        private static KdlConverter<KdlReadOnlyDocument?>? s_jsonDocumentConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="Memory{Byte}"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<Memory<byte>> MemoryByteConverter => s_memoryByteConverter ??= new MemoryByteConverter();
        private static KdlConverter<Memory<byte>>? s_memoryByteConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="ReadOnlyMemory{Byte}"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<ReadOnlyMemory<byte>> ReadOnlyMemoryByteConverter => s_readOnlyMemoryByteConverter ??= new ReadOnlyMemoryByteConverter();
        private static KdlConverter<ReadOnlyMemory<byte>>? s_readOnlyMemoryByteConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="object"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<object?> ObjectConverter => s_objectConverter ??= new DefaultObjectConverter();
        private static KdlConverter<object?>? s_objectConverter;

#if NET
        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="Half"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<Half> HalfConverter => s_halfConverter ??= new HalfConverter();
        private static KdlConverter<Half>? s_halfConverter;
#endif

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="float"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<float> SingleConverter => s_singleConverter ??= new SingleConverter();
        private static KdlConverter<float>? s_singleConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="sbyte"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        [CLSCompliant(false)]
        public static KdlConverter<sbyte> SByteConverter => s_sbyteConverter ??= new SByteConverter();
        private static KdlConverter<sbyte>? s_sbyteConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="string"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<string?> StringConverter => s_stringConverter ??= new StringConverter();
        private static KdlConverter<string?>? s_stringConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="TimeSpan"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<TimeSpan> TimeSpanConverter => s_timeSpanConverter ??= new TimeSpanConverter();
        private static KdlConverter<TimeSpan>? s_timeSpanConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="ushort"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        [CLSCompliant(false)]
        public static KdlConverter<ushort> UInt16Converter => s_uint16Converter ??= new UInt16Converter();
        private static KdlConverter<ushort>? s_uint16Converter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="uint"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        [CLSCompliant(false)]
        public static KdlConverter<uint> UInt32Converter => s_uint32Converter ??= new UInt32Converter();
        private static KdlConverter<uint>? s_uint32Converter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="ulong"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        [CLSCompliant(false)]
        public static KdlConverter<ulong> UInt64Converter => s_uint64Converter ??= new UInt64Converter();
        private static KdlConverter<ulong>? s_uint64Converter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="Uri"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<Uri?> UriConverter => s_uriConverter ??= new UriConverter();
        private static KdlConverter<Uri?>? s_uriConverter;

        /// <summary>
        /// Returns a <see cref="KdlConverter{T}"/> instance that converts <see cref="Version"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<Version?> VersionConverter => s_versionConverter ??= new VersionConverter();
        private static KdlConverter<Version?>? s_versionConverter;

        /// <summary>
        /// Creates a <see cref="KdlConverter{T}"/> instance that throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <typeparam name="T">The generic definition for the type.</typeparam>
        /// <returns>A <see cref="KdlConverter{T}"/> instance that throws <see cref="NotSupportedException"/></returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<T> GetUnsupportedTypeConverter<T>()
            => new UnsupportedTypeConverter<T>();

        /// <summary>
        /// Creates a <see cref="KdlConverter{T}"/> instance that converts <typeparamref name="T"/> values.
        /// </summary>
        /// <typeparam name="T">The generic definition for the enum type.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to use for serialization and deserialization.</param>
        /// <returns>A <see cref="KdlConverter{T}"/> instance that converts <typeparamref name="T"/> values.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<T> GetEnumConverter<T>(KdlSerializerOptions options) where T : struct, Enum
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            return EnumConverterFactory.Helpers.Create<T>(EnumConverterOptions.AllowNumbers, options);
        }

        /// <summary>
        /// Creates a <see cref="KdlConverter{T}"/> instance that converts <typeparamref name="T?"/> values.
        /// </summary>
        /// <typeparam name="T">The generic definition for the underlying nullable type.</typeparam>
        /// <param name="underlyingTypeInfo">Serialization metadata for the underlying nullable type.</param>
        /// <returns>A <see cref="KdlConverter{T}"/> instance that converts <typeparamref name="T?"/> values</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<T?> GetNullableConverter<T>(KdlTypeInfo<T> underlyingTypeInfo) where T : struct
        {
            if (underlyingTypeInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(underlyingTypeInfo));
            }

            KdlConverter<T> underlyingConverter = GetTypedConverter<T>(underlyingTypeInfo.Converter);

            return new NullableConverter<T>(underlyingConverter);
        }

        /// <summary>
        /// Creates a <see cref="KdlConverter{T}"/> instance that converts <typeparamref name="T?"/> values.
        /// </summary>
        /// <typeparam name="T">The generic definition for the underlying nullable type.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to use for serialization and deserialization.</param>
        /// <returns>A <see cref="KdlConverter{T}"/> instance that converts <typeparamref name="T?"/> values</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlConverter<T?> GetNullableConverter<T>(KdlSerializerOptions options) where T : struct
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }

            KdlConverter<T> underlyingConverter = GetTypedConverter<T>(options.GetConverterInternal(typeof(T)));

            return new NullableConverter<T>(underlyingConverter);
        }

        internal static KdlConverter<T> GetTypedConverter<T>(KdlConverter converter)
        {
            KdlConverter<T>? typedConverter = converter as KdlConverter<T>;
            if (typedConverter == null)
            {
                throw new InvalidOperationException(string.Format(SR.SerializationConverterNotCompatible, typedConverter, typeof(T)));
            }

            return typedConverter;
        }
    }
}
