namespace System.Text.Kdl.Serialization.Converters
{
    [Flags]
    internal enum EnumConverterOptions // Do not modify (legacy schema generation)
    {
        /// <summary>
        /// Allow string values.
        /// </summary>
        AllowStrings = 0b0001,

        /// <summary>
        /// Allow number values.
        /// </summary>
        AllowNumbers = 0b0010
    }
}
