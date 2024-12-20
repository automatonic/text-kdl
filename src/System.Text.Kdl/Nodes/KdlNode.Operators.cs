using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlNode
    {
        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="bool"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(bool value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="bool"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(bool? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="byte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(byte value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="byte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(byte? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="char"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(char value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="char"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(char? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(DateTime value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(DateTime? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(DateTimeOffset value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(DateTimeOffset? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="decimal"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(decimal value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="decimal"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(decimal? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="double"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(double value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="double"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(double? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="Guid"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(Guid value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="Guid"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(Guid? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="short"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(short value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="short"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(short? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="int"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(int value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="int"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(int? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="long"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(long value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="long"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(long? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode(sbyte value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode?(sbyte? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="float"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode(float value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="float"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlNode?(float? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="string"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [return: NotNullIfNotNull(nameof(value))]
        public static implicit operator KdlNode?(string? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ushort"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode(ushort value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ushort"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode?(ushort? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="uint"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode(uint value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="uint"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode?(uint? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ulong"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode(ulong value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ulong"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlNode"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlNode?(ulong? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="bool"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator bool(KdlNode value) => value.GetValue<bool>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="bool"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator bool?(KdlNode? value) => value?.GetValue<bool>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="byte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator byte(KdlNode value) => value.GetValue<byte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="byte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator byte?(KdlNode? value) => value?.GetValue<byte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="char"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator char(KdlNode value) => value.GetValue<char>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="char"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator char?(KdlNode? value) => value?.GetValue<char>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator DateTime(KdlNode value) => value.GetValue<DateTime>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator DateTime?(KdlNode? value) => value?.GetValue<DateTime>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator DateTimeOffset(KdlNode value) => value.GetValue<DateTimeOffset>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator DateTimeOffset?(KdlNode? value) => value?.GetValue<DateTimeOffset>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="decimal"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator decimal(KdlNode value) => value.GetValue<decimal>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="decimal"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator decimal?(KdlNode? value) => value?.GetValue<decimal>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="double"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator double(KdlNode value) => value.GetValue<double>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="double"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator double?(KdlNode? value) => value?.GetValue<double>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="Guid"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator Guid(KdlNode value) => value.GetValue<Guid>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="Guid"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator Guid?(KdlNode? value) => value?.GetValue<Guid>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="short"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator short(KdlNode value) => value.GetValue<short>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="short"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator short?(KdlNode? value) => value?.GetValue<short>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="int"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator int(KdlNode value) => value.GetValue<int>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="int"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator int?(KdlNode? value) => value?.GetValue<int>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="long"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator long(KdlNode value) => value.GetValue<long>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="long"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator long?(KdlNode? value) => value?.GetValue<long>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator sbyte(KdlNode value) => value.GetValue<sbyte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator sbyte?(KdlNode? value) => value?.GetValue<sbyte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="float"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator float(KdlNode value) => value.GetValue<float>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="float"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator float?(KdlNode? value) => value?.GetValue<float>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="string"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        public static explicit operator string?(KdlNode? value) => value?.GetValue<string>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ushort"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ushort(KdlNode value) => value.GetValue<ushort>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ushort"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ushort?(KdlNode? value) => value?.GetValue<ushort>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="uint"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator uint(KdlNode value) => value.GetValue<uint>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="uint"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator uint?(KdlNode? value) => value?.GetValue<uint>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ulong"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ulong(KdlNode value) => value.GetValue<ulong>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ulong"/> to a <see cref="KdlNode"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlNode"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ulong?(KdlNode? value) => value?.GetValue<ulong>();
    }
}
