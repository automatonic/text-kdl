﻿using System.ComponentModel;
using System.Reflection;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides serialization metadata about a property or field.
    /// </summary>
    /// <typeparam name="T">The type to convert of the <see cref="KdlConverter{T}"/> for the property.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class KdlPropertyInfoValues<T>
    {
        /// <summary>
        /// If <see langword="true"/>, indicates that the member is a property, otherwise indicates the member is a field.
        /// </summary>
        public bool IsProperty { get; init; }

        /// <summary>
        /// Whether the property or field is public.
        /// </summary>
        public bool IsPublic { get; init; }

        /// <summary>
        /// Whether the property or field is a virtual property.
        /// </summary>
        public bool IsVirtual { get; init; }

        /// <summary>
        /// The declaring type of the property or field.
        /// </summary>
        public Type DeclaringType { get; init; } = null!;

        /// <summary>
        /// The <see cref="KdlTypeInfo"/> info for the property or field's type.
        /// </summary>
        public KdlTypeInfo PropertyTypeInfo { get; init; } = null!;

        /// <summary>
        /// A <see cref="KdlConverter"/> for the property or field, specified by <see cref="KdlConverterAttribute"/>.
        /// </summary>
        public KdlConverter<T>? Converter { get; init; }

        /// <summary>
        /// Provides a mechanism to get the property or field's value.
        /// </summary>
        public Func<object, T?>? Getter { get; init; }

        /// <summary>
        /// Provides a mechanism to set the property or field's value.
        /// </summary>
        public Action<object, T?>? Setter { get; init; }

        /// <summary>
        /// Specifies a condition for the member to be ignored.
        /// </summary>
        public KdlIgnoreCondition? IgnoreCondition { get; init; }

        /// <summary>
        /// Whether the property was annotated with <see cref="KdlIncludeAttribute"/>.
        /// </summary>
        public bool HasKdlInclude { get; init; }

        /// <summary>
        /// Whether the property was annotated with <see cref="KdlExtensionDataAttribute"/>.
        /// </summary>
        public bool IsExtensionData { get; init; }

        /// <summary>
        /// If the property or field is a number, specifies how it should processed when serializing and deserializing.
        /// </summary>
        public KdlNumberHandling? NumberHandling { get; init; }

        /// <summary>
        /// The name of the property or field.
        /// </summary>
        public string PropertyName { get; init; } = null!;

        /// <summary>
        /// The name to be used when processing the property or field, specified by <see cref="KdlPropertyNameAttribute"/>.
        /// </summary>
        public string? KdlPropertyName { get; init; }

        /// <summary>
        /// Provides a <see cref="ICustomAttributeProvider"/> factory that maps to <see cref="KdlPropertyInfo.AttributeProvider"/>.
        /// </summary>
        public Func<ICustomAttributeProvider>? AttributeProviderFactory { get; init; }
    }
}
