using System.ComponentModel;
using System.Globalization;

namespace System.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides helpers to create and initialize metadata for KDL-serializable types.
    /// </summary>
    /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class KdlMetadataServices
    {
        /// <summary>
        /// Creates metadata for a property or field.
        /// </summary>
        /// <typeparam name="T">The type that the converter for the property returns or accepts when converting KDL data.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to initialize the metadata with.</param>
        /// <param name="propertyInfo">Provides serialization metadata about the property or field.</param>
        /// <returns>A <see cref="KdlPropertyInfo"/> instance initialized with the provided metadata.</returns>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlPropertyInfo CreatePropertyInfo<T>(KdlSerializerOptions options, KdlPropertyInfoValues<T> propertyInfo)
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }
            if (propertyInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(propertyInfo));
            }

            Type? declaringType = propertyInfo.DeclaringType ?? throw new ArgumentException(nameof(propertyInfo.DeclaringType));

            string? propertyName = propertyInfo.PropertyName ?? throw new ArgumentException(nameof(propertyInfo.PropertyName));

            if (!propertyInfo.IsProperty && propertyInfo.IsVirtual)
            {
                throw new InvalidOperationException(string.Format(provider: CultureInfo.InvariantCulture, format: SR.FieldCannotBeVirtual, nameof(propertyInfo.IsProperty), nameof(propertyInfo.IsVirtual)));
            }

            return CreatePropertyInfoCore(propertyInfo, options);
        }

        /// <summary>
        /// Creates metadata for a complex class or struct.
        /// </summary>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to initialize the metadata with.</param>
        /// <param name="objectInfo">Provides serialization metadata about an object type with constructors, properties, and fields.</param>
        /// <typeparam name="T">The type of the class or struct.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="objectInfo"/> is null.</exception>
        /// <returns>A <see cref="KdlTypeInfo{T}"/> instance representing the class or struct.</returns>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<T> CreateObjectInfo<T>(KdlSerializerOptions options, KdlObjectInfoValues<T> objectInfo) where T : notnull
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }
            if (objectInfo is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(objectInfo));
            }

            return CreateCore(options, objectInfo);
        }

        /// <summary>
        /// Creates metadata for a primitive or a type with a custom converter.
        /// </summary>
        /// <typeparam name="T">The generic type definition.</typeparam>
        /// <returns>A <see cref="KdlTypeInfo{T}"/> instance representing the type.</returns>
        /// <remarks>This API is for use by the output of the System.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<T> CreateValueInfo<T>(KdlSerializerOptions options, KdlConverter converter)
        {
            if (options is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(options));
            }
            if (converter is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(converter));
            }

            KdlTypeInfo<T> info = CreateCore<T>(converter, options);
            return info;
        }
    }
}
