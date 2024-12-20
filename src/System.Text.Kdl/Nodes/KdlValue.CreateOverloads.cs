﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlValue
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(bool value, KdlNodeOptions? options = null) => new KdlValuePrimitive<bool>(value, KdlMetadataServices.BooleanConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(bool? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<bool>(value.Value, KdlMetadataServices.BooleanConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(byte value, KdlNodeOptions? options = null) => new KdlValuePrimitive<byte>(value, KdlMetadataServices.ByteConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(byte? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<byte>(value.Value, KdlMetadataServices.ByteConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(char value, KdlNodeOptions? options = null) => new KdlValuePrimitive<char>(value, KdlMetadataServices.CharConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(char? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<char>(value.Value, KdlMetadataServices.CharConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(DateTime value, KdlNodeOptions? options = null) => new KdlValuePrimitive<DateTime>(value, KdlMetadataServices.DateTimeConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(DateTime? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<DateTime>(value.Value, KdlMetadataServices.DateTimeConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(DateTimeOffset value, KdlNodeOptions? options = null) => new KdlValuePrimitive<DateTimeOffset>(value, KdlMetadataServices.DateTimeOffsetConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(DateTimeOffset? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<DateTimeOffset>(value.Value, KdlMetadataServices.DateTimeOffsetConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(decimal value, KdlNodeOptions? options = null) => new KdlValuePrimitive<decimal>(value, KdlMetadataServices.DecimalConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(decimal? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<decimal>(value.Value, KdlMetadataServices.DecimalConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(double value, KdlNodeOptions? options = null) => new KdlValuePrimitive<double>(value, KdlMetadataServices.DoubleConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(double? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<double>(value.Value, KdlMetadataServices.DoubleConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(Guid value, KdlNodeOptions? options = null) => new KdlValuePrimitive<Guid>(value, KdlMetadataServices.GuidConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(Guid? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<Guid>(value.Value, KdlMetadataServices.GuidConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(short value, KdlNodeOptions? options = null) => new KdlValuePrimitive<short>(value, KdlMetadataServices.Int16Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(short? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<short>(value.Value, KdlMetadataServices.Int16Converter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(int value, KdlNodeOptions? options = null) => new KdlValuePrimitive<int>(value, KdlMetadataServices.Int32Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(int? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<int>(value.Value, KdlMetadataServices.Int32Converter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(long value, KdlNodeOptions? options = null) => new KdlValuePrimitive<long>(value, KdlMetadataServices.Int64Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(long? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<long>(value.Value, KdlMetadataServices.Int64Converter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(sbyte value, KdlNodeOptions? options = null) => new KdlValuePrimitive<sbyte>(value, KdlMetadataServices.SByteConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(sbyte? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<sbyte>(value.Value, KdlMetadataServices.SByteConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue Create(float value, KdlNodeOptions? options = null) => new KdlValuePrimitive<float>(value, KdlMetadataServices.SingleConverter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(float? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<float>(value.Value, KdlMetadataServices.SingleConverter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [return: NotNullIfNotNull(nameof(value))]
        public static KdlValue? Create(string? value, KdlNodeOptions? options = null) => value != null ? new KdlValuePrimitive<string>(value, KdlMetadataServices.StringConverter!, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(ushort value, KdlNodeOptions? options = null) => new KdlValuePrimitive<ushort>(value, KdlMetadataServices.UInt16Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(ushort? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<ushort>(value.Value, KdlMetadataServices.UInt16Converter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(uint value, KdlNodeOptions? options = null) => new KdlValuePrimitive<uint>(value, KdlMetadataServices.UInt32Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(uint? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<uint>(value.Value, KdlMetadataServices.UInt32Converter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue Create(ulong value, KdlNodeOptions? options = null) => new KdlValuePrimitive<ulong>(value, KdlMetadataServices.UInt64Converter, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        [CLSCompliantAttribute(false)]
        public static KdlValue? Create(ulong? value, KdlNodeOptions? options = null) => value.HasValue ? new KdlValuePrimitive<ulong>(value.Value, KdlMetadataServices.UInt64Converter, options) : null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(KdlElement value, KdlNodeOptions? options = null) => KdlValue.CreateFromElement(ref value, options);

        /// <summary>
        ///   Initializes a new instance of the <see cref="KdlValue"/> class that contains the specified value.
        /// </summary>
        /// <param name="value">The underlying value of the new <see cref="KdlValue"/> instance.</param>
        /// <param name="options">Options to control the behavior.</param>
        /// <returns>The new instance of the <see cref="KdlValue"/> class that contains the specified value.</returns>
        public static KdlValue? Create(KdlElement? value, KdlNodeOptions? options = null) => value is KdlElement element ? KdlValue.CreateFromElement(ref element, options) : null;
    }
}
