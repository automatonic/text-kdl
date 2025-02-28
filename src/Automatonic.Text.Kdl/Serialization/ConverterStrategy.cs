using Automatonic.Text.Kdl.Serialization;

namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Determines how a given class is treated when it is (de)serialized.
    /// </summary>
    /// <remarks>
    /// Although bit flags are used, a given ConverterStrategy can only be one value.
    /// Bit flags are used to efficiently compare against more than one value.
    /// </remarks>
    internal enum ConverterStrategy : byte
    {
        /// <summary>
        /// Default value; only used by <see cref="KdlConverterFactory"/>.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Objects with properties.
        /// </summary>
        Object = 0x1,
        /// <summary>
        /// Simple values or user-provided custom converters.
        /// </summary>
        Value = 0x2,
        /// <summary>
        /// Enumerable collections except dictionaries.
        /// </summary>
        Enumerable = 0x8,
        /// <summary>
        /// Dictionary types.
        /// </summary>
        Dictionary = 0x10,
    }
}
