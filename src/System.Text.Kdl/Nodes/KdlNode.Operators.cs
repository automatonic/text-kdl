using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlVertex
    {
        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="bool"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(bool value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="bool"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(bool? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="byte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(byte value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="byte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(byte? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="char"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(char value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="char"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(char? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(DateTime value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(DateTime? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(DateTimeOffset value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(DateTimeOffset? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="decimal"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(decimal value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="decimal"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(decimal? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="double"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(double value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="double"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(double? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="Guid"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(Guid value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="Guid"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(Guid? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="short"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(short value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="short"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(short? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="int"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(int value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="int"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(int? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="long"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(long value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="long"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(long? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex(sbyte value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex?(sbyte? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="float"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex(float value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="float"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlVertex?(float? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="string"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [return: NotNullIfNotNull(nameof(value))]
        public static implicit operator KdlVertex?(string? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ushort"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex(ushort value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ushort"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex?(ushort? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="uint"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex(uint value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="uint"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex?(uint? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ulong"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex(ulong value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ulong"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlVertex"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlVertex?(ulong? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="bool"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator bool(KdlVertex value) => value.GetValue<bool>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="bool"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator bool?(KdlVertex? value) => value?.GetValue<bool>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="byte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator byte(KdlVertex value) => value.GetValue<byte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="byte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator byte?(KdlVertex? value) => value?.GetValue<byte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="char"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator char(KdlVertex value) => value.GetValue<char>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="char"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator char?(KdlVertex? value) => value?.GetValue<char>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator DateTime(KdlVertex value) => value.GetValue<DateTime>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator DateTime?(KdlVertex? value) => value?.GetValue<DateTime>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator DateTimeOffset(KdlVertex value) => value.GetValue<DateTimeOffset>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator DateTimeOffset?(KdlVertex? value) => value?.GetValue<DateTimeOffset>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="decimal"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator decimal(KdlVertex value) => value.GetValue<decimal>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="decimal"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator decimal?(KdlVertex? value) => value?.GetValue<decimal>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="double"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator double(KdlVertex value) => value.GetValue<double>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="double"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator double?(KdlVertex? value) => value?.GetValue<double>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="Guid"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator Guid(KdlVertex value) => value.GetValue<Guid>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="Guid"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator Guid?(KdlVertex? value) => value?.GetValue<Guid>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="short"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator short(KdlVertex value) => value.GetValue<short>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="short"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator short?(KdlVertex? value) => value?.GetValue<short>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="int"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator int(KdlVertex value) => value.GetValue<int>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="int"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator int?(KdlVertex? value) => value?.GetValue<int>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="long"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator long(KdlVertex value) => value.GetValue<long>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="long"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator long?(KdlVertex? value) => value?.GetValue<long>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator sbyte(KdlVertex value) => value.GetValue<sbyte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator sbyte?(KdlVertex? value) => value?.GetValue<sbyte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="float"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator float(KdlVertex value) => value.GetValue<float>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="float"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator float?(KdlVertex? value) => value?.GetValue<float>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="string"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        public static explicit operator string?(KdlVertex? value) => value?.GetValue<string>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ushort"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ushort(KdlVertex value) => value.GetValue<ushort>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ushort"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ushort?(KdlVertex? value) => value?.GetValue<ushort>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="uint"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator uint(KdlVertex value) => value.GetValue<uint>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="uint"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator uint?(KdlVertex? value) => value?.GetValue<uint>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ulong"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ulong(KdlVertex value) => value.GetValue<ulong>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ulong"/> to a <see cref="KdlVertex"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlVertex"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ulong?(KdlVertex? value) => value?.GetValue<ulong>();
    }
}
