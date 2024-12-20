// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Kdl.Serialization.Converters;

namespace System.Text.Kdl.Serialization
{
    /// <summary>
    /// Converter to convert enums to and from numeric values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type that this converter targets.</typeparam>
    /// <remarks>
    /// This is the default converter for enums and can be used to override
    /// <see cref="KdlSourceGenerationOptionsAttribute.UseStringEnumConverter"/>
    /// on individual types or properties.
    /// </remarks>
    public sealed class KdlNumberEnumConverter<TEnum> : KdlConverterFactory
        where TEnum : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of <see cref="KdlNumberEnumConverter{TEnum}"/>.
        /// </summary>
        public KdlNumberEnumConverter() { }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TEnum);

        /// <inheritdoc />
        public override KdlConverter? CreateConverter(Type typeToConvert, KdlSerializerOptions options)
        {
            if (typeToConvert != typeof(TEnum))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_KdlConverterFactory_TypeNotSupported(typeToConvert);
            }

            return EnumConverterFactory.Helpers.Create<TEnum>(EnumConverterOptions.AllowNumbers, options);
        }
    }
}
