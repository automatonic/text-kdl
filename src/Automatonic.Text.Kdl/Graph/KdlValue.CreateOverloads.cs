using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Graph
{
    public partial class KdlValue
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(bool value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<bool>(value, KdlMetadataServices.BooleanConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(bool? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<bool>(
                    value.Value,
                    KdlMetadataServices.BooleanConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(byte value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<byte>(value, KdlMetadataServices.ByteConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(byte? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<byte>(
                    value.Value,
                    KdlMetadataServices.ByteConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(char value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<char>(value, KdlMetadataServices.CharConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(char? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<char>(
                    value.Value,
                    KdlMetadataServices.CharConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(DateTime value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<DateTime>(value, KdlMetadataServices.DateTimeConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(DateTime? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<DateTime>(
                    value.Value,
                    KdlMetadataServices.DateTimeConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(DateTimeOffset value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<DateTimeOffset>(
                value,
                KdlMetadataServices.DateTimeOffsetConverter,
                options
            );

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(DateTimeOffset? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<DateTimeOffset>(
                    value.Value,
                    KdlMetadataServices.DateTimeOffsetConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(decimal value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<decimal>(value, KdlMetadataServices.DecimalConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(decimal? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<decimal>(
                    value.Value,
                    KdlMetadataServices.DecimalConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(double value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<double>(value, KdlMetadataServices.DoubleConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(double? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<double>(
                    value.Value,
                    KdlMetadataServices.DoubleConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(Guid value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<Guid>(value, KdlMetadataServices.GuidConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(Guid? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<Guid>(
                    value.Value,
                    KdlMetadataServices.GuidConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(short value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<short>(value, KdlMetadataServices.Int16Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(short? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<short>(
                    value.Value,
                    KdlMetadataServices.Int16Converter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(int value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<int>(value, KdlMetadataServices.Int32Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(int? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<int>(
                    value.Value,
                    KdlMetadataServices.Int32Converter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(long value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<long>(value, KdlMetadataServices.Int64Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(long? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<long>(
                    value.Value,
                    KdlMetadataServices.Int64Converter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(sbyte value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<sbyte>(value, KdlMetadataServices.SByteConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(sbyte? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<sbyte>(
                    value.Value,
                    KdlMetadataServices.SByteConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(float value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<float>(value, KdlMetadataServices.SingleConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(float? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<float>(
                    value.Value,
                    KdlMetadataServices.SingleConverter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [return: NotNullIfNotNull(nameof(value))]
        public static KdlValue? Create(string? value, KdlElementOptions? options = null) =>
            value != null
                ? new KdlValuePrimitive<string>(
                    value,
                    KdlMetadataServices.StringConverter!,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(ushort value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<ushort>(value, KdlMetadataServices.UInt16Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(ushort? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<ushort>(
                    value.Value,
                    KdlMetadataServices.UInt16Converter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(uint value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<uint>(value, KdlMetadataServices.UInt32Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(uint? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<uint>(
                    value.Value,
                    KdlMetadataServices.UInt32Converter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(ulong value, KdlElementOptions? options = null) =>
            new KdlValuePrimitive<ulong>(value, KdlMetadataServices.UInt64Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(ulong? value, KdlElementOptions? options = null) =>
            value.HasValue
                ? new KdlValuePrimitive<ulong>(
                    value.Value,
                    KdlMetadataServices.UInt64Converter,
                    options
                )
                : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(
            KdlReadOnlyElement value,
            KdlElementOptions? options = null
        ) => KdlValue.CreateFromElement(ref value, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(
            KdlReadOnlyElement? value,
            KdlElementOptions? options = null
        ) =>
            value is KdlReadOnlyElement element
                ? KdlValue.CreateFromElement(ref element, options)
                : null;
    }
}
