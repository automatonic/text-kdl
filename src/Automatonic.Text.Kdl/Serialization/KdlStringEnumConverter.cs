using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Converters;

namespace Automatonic.Text.Kdl.Serialization
{
    /// <summary>
    /// Converter to convert enums to and from strings.
    /// </summary>
    /// <remarks>
    /// Reading is case insensitive, writing can be customized via a <see cref="KdlNamingPolicy" />.
    /// </remarks>
    /// <typeparam name="TEnum">The enum type that this converter targets.</typeparam>
    /// <remarks>
    /// Constructor.
    /// </remarks>
    /// <param name="namingPolicy">
    /// Optional naming policy for writing enum values.
    /// </param>
    /// <param name="allowIntegerValues">
    /// True to allow undefined enum values. When true, if an enum value isn't
    /// defined it will output as a number rather than a string.
    /// </param>
    public class KdlStringEnumConverter<TEnum>(KdlNamingPolicy? namingPolicy = null, bool allowIntegerValues = true) : KdlConverterFactory
        where TEnum : struct, Enum
    {
        private readonly KdlNamingPolicy? _namingPolicy = namingPolicy;
        private readonly EnumConverterOptions _converterOptions = allowIntegerValues
                ? EnumConverterOptions.AllowNumbers | EnumConverterOptions.AllowStrings
                : EnumConverterOptions.AllowStrings;

        /// <summary>
        /// Constructor. Creates the <see cref="KdlStringEnumConverter"/> with the
        /// default naming policy and allows integer values.
        /// </summary>
        public KdlStringEnumConverter() : this(namingPolicy: null, allowIntegerValues: true)
        {
            // An empty constructor is needed for construction via attributes
        }

        /// <inheritdoc />
        public sealed override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TEnum);

        /// <inheritdoc />
        public sealed override KdlConverter? CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            if (typeToConvert != typeof(TEnum))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_KdlConverterFactory_TypeNotSupported(typeToConvert);
            }

            return EnumConverterFactory.Helpers.Create<TEnum>(_converterOptions, options, _namingPolicy);
        }
    }

    /// <summary>
    /// Converter to convert enums to and from strings.
    /// </summary>
    /// <remarks>
    /// Reading is case insensitive, writing can be customized via a <see cref="KdlNamingPolicy" />.
    /// </remarks>
    /// <remarks>
    /// Constructor.
    /// </remarks>
    /// <param name="namingPolicy">
    /// Optional naming policy for writing enum values.
    /// </param>
    /// <param name="allowIntegerValues">
    /// True to allow undefined enum values. When true, if an enum value isn't
    /// defined it will output as a number rather than a string.
    /// </param>
    [RequiresDynamicCode(
        "KdlStringEnumConverter cannot be statically analyzed and requires runtime code generation. " +
        "Applications should use the generic KdlStringEnumConverter<TEnum> instead.")]
    public class KdlStringEnumConverter(KdlNamingPolicy? namingPolicy = null, bool allowIntegerValues = true) : KdlConverterFactory
    {
        private readonly KdlNamingPolicy? _namingPolicy = namingPolicy;
        private readonly EnumConverterOptions _converterOptions = allowIntegerValues
                ? EnumConverterOptions.AllowNumbers | EnumConverterOptions.AllowStrings
                : EnumConverterOptions.AllowStrings;

        /// <summary>
        /// Constructor. Creates the <see cref="KdlStringEnumConverter"/> with the
        /// default naming policy and allows integer values.
        /// </summary>
        public KdlStringEnumConverter() : this(namingPolicy: null, allowIntegerValues: true)
        {
            // An empty constructor is needed for construction via attributes
        }

        /// <inheritdoc />
        public sealed override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        /// <inheritdoc />
        public sealed override KdlConverter CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            if (!typeToConvert.IsEnum)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_KdlConverterFactory_TypeNotSupported(typeToConvert);
            }

            return EnumConverterFactory.Create(typeToConvert, _converterOptions, _namingPolicy, options);
        }
    }
}
