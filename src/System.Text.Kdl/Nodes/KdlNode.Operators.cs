using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl.Nodes
{
    public partial class KdlElement
    {
        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="bool"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(bool value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="bool"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(bool? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="byte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(byte value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="byte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(byte? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="char"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(char value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="char"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(char? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(DateTime value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(DateTime? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(DateTimeOffset value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(DateTimeOffset? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="decimal"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(decimal value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="decimal"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(decimal? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="double"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(double value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="double"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(double? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="Guid"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(Guid value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="Guid"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(Guid? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="short"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(short value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="short"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(short? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="int"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(int value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="int"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(int? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="long"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(long value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="long"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(long? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement(sbyte value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement?(sbyte? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="float"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement(float value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="float"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        public static implicit operator KdlElement?(float? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="string"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [return: NotNullIfNotNull(nameof(value))]
        public static implicit operator KdlElement?(string? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ushort"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement(ushort value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ushort"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement?(ushort? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="uint"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement(uint value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="uint"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement?(uint? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ulong"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement(ulong value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an implicit conversion of a given <see cref="ulong"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to implicitly convert.</param>
        /// <returns>A <see cref="KdlElement"/> instance converted from the <paramref name="value"/> parameter.</returns>
        [System.CLSCompliantAttribute(false)]
        public static implicit operator KdlElement?(ulong? value) => KdlValue.Create(value);

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="bool"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator bool(KdlElement value) => value.GetValue<bool>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="bool"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator bool?(KdlElement? value) => value?.GetValue<bool>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="byte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator byte(KdlElement value) => value.GetValue<byte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="byte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator byte?(KdlElement? value) => value?.GetValue<byte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="char"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator char(KdlElement value) => value.GetValue<char>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="char"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="char"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator char?(KdlElement? value) => value?.GetValue<char>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator DateTime(KdlElement value) => value.GetValue<DateTime>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTime"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator DateTime?(KdlElement? value) => value?.GetValue<DateTime>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator DateTimeOffset(KdlElement value) => value.GetValue<DateTimeOffset>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="DateTimeOffset"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="DateTimeOffset"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator DateTimeOffset?(KdlElement? value) => value?.GetValue<DateTimeOffset>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="decimal"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator decimal(KdlElement value) => value.GetValue<decimal>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="decimal"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="decimal"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator decimal?(KdlElement? value) => value?.GetValue<decimal>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="double"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator double(KdlElement value) => value.GetValue<double>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="double"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="double"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator double?(KdlElement? value) => value?.GetValue<double>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="Guid"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator Guid(KdlElement value) => value.GetValue<Guid>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="Guid"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="Guid"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator Guid?(KdlElement? value) => value?.GetValue<Guid>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="short"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator short(KdlElement value) => value.GetValue<short>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="short"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="short"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator short?(KdlElement? value) => value?.GetValue<short>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="int"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator int(KdlElement value) => value.GetValue<int>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="int"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="int"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator int?(KdlElement? value) => value?.GetValue<int>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="long"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator long(KdlElement value) => value.GetValue<long>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="long"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="long"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator long?(KdlElement? value) => value?.GetValue<long>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator sbyte(KdlElement value) => value.GetValue<sbyte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="sbyte"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="sbyte"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator sbyte?(KdlElement? value) => value?.GetValue<sbyte>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="float"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator float(KdlElement value) => value.GetValue<float>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="float"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="float"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator float?(KdlElement? value) => value?.GetValue<float>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="string"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        public static explicit operator string?(KdlElement? value) => value?.GetValue<string>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ushort"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ushort(KdlElement value) => value.GetValue<ushort>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ushort"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ushort"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ushort?(KdlElement? value) => value?.GetValue<ushort>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="uint"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator uint(KdlElement value) => value.GetValue<uint>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="uint"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="uint"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator uint?(KdlElement? value) => value?.GetValue<uint>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ulong"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ulong(KdlElement value) => value.GetValue<ulong>();

        /// <summary>
        ///   Defines an explicit conversion of a given <see cref="ulong"/> to a <see cref="KdlElement"/>.
        /// </summary>
        /// <param name="value">A <see cref="ulong"/> to explicitly convert.</param>
        /// <returns>A value converted from the <see cref="KdlElement"/> instance.</returns>
        [System.CLSCompliantAttribute(false)]
        public static explicit operator ulong?(KdlElement? value) => value?.GetValue<ulong>();
    }
}
