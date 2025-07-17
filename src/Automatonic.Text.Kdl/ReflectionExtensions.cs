using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Automatonic.Text.Kdl.Reflection
{
    internal static partial class ReflectionExtensions
    {
        // Immutable collection types.
        private const string ImmutableArrayGenericTypeName =
            "System.Collections.Immutable.ImmutableArray`1";
        private const string ImmutableListGenericTypeName =
            "System.Collections.Immutable.ImmutableList`1";
        private const string ImmutableListGenericInterfaceTypeName =
            "System.Collections.Immutable.IImmutableList`1";
        private const string ImmutableStackGenericTypeName =
            "System.Collections.Immutable.ImmutableStack`1";
        private const string ImmutableStackGenericInterfaceTypeName =
            "System.Collections.Immutable.IImmutableStack`1";
        private const string ImmutableQueueGenericTypeName =
            "System.Collections.Immutable.ImmutableQueue`1";
        private const string ImmutableQueueGenericInterfaceTypeName =
            "System.Collections.Immutable.IImmutableQueue`1";
        private const string ImmutableSortedSetGenericTypeName =
            "System.Collections.Immutable.ImmutableSortedSet`1";
        private const string ImmutableHashSetGenericTypeName =
            "System.Collections.Immutable.ImmutableHashSet`1";
        private const string ImmutableSetGenericInterfaceTypeName =
            "System.Collections.Immutable.IImmutableSet`1";
        private const string ImmutableDictionaryGenericTypeName =
            "System.Collections.Immutable.ImmutableDictionary`2";
        private const string ImmutableDictionaryGenericInterfaceTypeName =
            "System.Collections.Immutable.IImmutableDictionary`2";
        private const string ImmutableSortedDictionaryGenericTypeName =
            "System.Collections.Immutable.ImmutableSortedDictionary`2";

        // Immutable collection builder types.
        private const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray";
        private const string ImmutableListTypeName = "System.Collections.Immutable.ImmutableList";
        private const string ImmutableStackTypeName = "System.Collections.Immutable.ImmutableStack";
        private const string ImmutableQueueTypeName = "System.Collections.Immutable.ImmutableQueue";
        private const string ImmutableSortedSetTypeName =
            "System.Collections.Immutable.ImmutableSortedSet";
        private const string ImmutableHashSetTypeName =
            "System.Collections.Immutable.ImmutableHashSet";
        private const string ImmutableDictionaryTypeName =
            "System.Collections.Immutable.ImmutableDictionary";
        private const string ImmutableSortedDictionaryTypeName =
            "System.Collections.Immutable.ImmutableSortedDictionary";

        public const string CreateRangeMethodName = "CreateRange";

        public static Type? GetCompatibleGenericBaseClass(this Type type, Type? baseType)
        {
            if (baseType is null)
            {
                return null;
            }

            Debug.Assert(baseType.IsGenericType);
            Debug.Assert(!baseType.IsInterface);
            Debug.Assert(baseType == baseType.GetGenericTypeDefinition());

            Type? baseTypeToCheck = type;

            while (baseTypeToCheck != null && baseTypeToCheck != typeof(object))
            {
                if (baseTypeToCheck.IsGenericType)
                {
                    Type genericTypeToCheck = baseTypeToCheck.GetGenericTypeDefinition();
                    if (genericTypeToCheck == baseType)
                    {
                        return baseTypeToCheck;
                    }
                }

                baseTypeToCheck = baseTypeToCheck.BaseType;
            }

            return null;
        }

        [UnconditionalSuppressMessage(
            "ReflectionAnalysis",
            "IL2070:UnrecognizedReflectionPattern",
            Justification = "The 'interfaceType' must exist and so trimmer kept it. In which case "
                + "It also kept it on any type which implements it. The below call to GetInterfaces "
                + "may return fewer results when trimmed but it will return the 'interfaceType' "
                + "if the type implemented it, even after trimming."
        )]
        public static Type? GetCompatibleGenericInterface(this Type type, Type? interfaceType)
        {
            if (interfaceType is null)
            {
                return null;
            }

            Debug.Assert(interfaceType.IsGenericType);
            Debug.Assert(interfaceType.IsInterface);
            Debug.Assert(interfaceType == interfaceType.GetGenericTypeDefinition());

            Type interfaceToCheck = type;

            if (interfaceToCheck.IsGenericType)
            {
                interfaceToCheck = interfaceToCheck.GetGenericTypeDefinition();
            }

            if (interfaceToCheck == interfaceType)
            {
                return type;
            }

            foreach (Type typeToCheck in type.GetInterfaces())
            {
                if (typeToCheck.IsGenericType)
                {
                    Type genericInterfaceToCheck = typeToCheck.GetGenericTypeDefinition();
                    if (genericInterfaceToCheck == interfaceType)
                    {
                        return typeToCheck;
                    }
                }
            }

            return null;
        }

        public static bool IsImmutableDictionaryType(this Type type)
        {
            if (
                !type.IsGenericType
                || !type.Assembly.FullName!.StartsWith(
                    "System.Collections.Immutable",
                    StringComparison.Ordinal
                )
            )
            {
                return false;
            }

            return GetBaseNameFromGenericType(type) switch
            {
                ImmutableDictionaryGenericTypeName
                or ImmutableDictionaryGenericInterfaceTypeName
                or ImmutableSortedDictionaryGenericTypeName => true,
                _ => false,
            };
        }

        public static bool IsImmutableEnumerableType(this Type type)
        {
            if (
                !type.IsGenericType
                || !type.Assembly.FullName!.StartsWith(
                    "System.Collections.Immutable",
                    StringComparison.Ordinal
                )
            )
            {
                return false;
            }

            return GetBaseNameFromGenericType(type) switch
            {
                ImmutableArrayGenericTypeName
                or ImmutableListGenericTypeName
                or ImmutableListGenericInterfaceTypeName
                or ImmutableStackGenericTypeName
                or ImmutableStackGenericInterfaceTypeName
                or ImmutableQueueGenericTypeName
                or ImmutableQueueGenericInterfaceTypeName
                or ImmutableSortedSetGenericTypeName
                or ImmutableHashSetGenericTypeName
                or ImmutableSetGenericInterfaceTypeName => true,
                _ => false,
            };
        }

        public static string? GetImmutableDictionaryConstructingTypeName(this Type type)
        {
            Debug.Assert(type.IsImmutableDictionaryType());

            // Use the generic type definition of the immutable collection to determine
            // an appropriate constructing type, i.e. a type that we can invoke the
            // `CreateRange<T>` method on, which returns the desired immutable collection.
            return GetBaseNameFromGenericType(type) switch
            {
                ImmutableDictionaryGenericTypeName or ImmutableDictionaryGenericInterfaceTypeName =>
                    ImmutableDictionaryTypeName,
                ImmutableSortedDictionaryGenericTypeName => ImmutableSortedDictionaryTypeName,
                _ => null, // We verified that the type is an immutable collection, so the
                // generic definition is one of the above.
            };
        }

        public static string? GetImmutableEnumerableConstructingTypeName(this Type type)
        {
            Debug.Assert(type.IsImmutableEnumerableType());

            // Use the generic type definition of the immutable collection to determine
            // an appropriate constructing type, i.e. a type that we can invoke the
            // `CreateRange<T>` method on, which returns the desired immutable collection.
            return GetBaseNameFromGenericType(type) switch
            {
                ImmutableArrayGenericTypeName => ImmutableArrayTypeName,
                ImmutableListGenericTypeName or ImmutableListGenericInterfaceTypeName =>
                    ImmutableListTypeName,
                ImmutableStackGenericTypeName or ImmutableStackGenericInterfaceTypeName =>
                    ImmutableStackTypeName,
                ImmutableQueueGenericTypeName or ImmutableQueueGenericInterfaceTypeName =>
                    ImmutableQueueTypeName,
                ImmutableSortedSetGenericTypeName => ImmutableSortedSetTypeName,
                ImmutableHashSetGenericTypeName or ImmutableSetGenericInterfaceTypeName =>
                    ImmutableHashSetTypeName,
                _ => null, // We verified that the type is an immutable collection, so the
                // generic definition is one of the above.
            };
        }

        private static string GetBaseNameFromGenericType(Type genericType)
        {
            Type genericTypeDef = genericType.GetGenericTypeDefinition();
            return genericTypeDef.FullName!;
        }

        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod?.IsVirtual == true
                || propertyInfo.SetMethod?.IsVirtual == true;
        }

        public static bool IsKeyValuePair(this Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

        public static bool TryGetDeserializationConstructor(
            [DynamicallyAccessedMembers(
                DynamicallyAccessedMemberTypes.PublicConstructors
                    | DynamicallyAccessedMemberTypes.NonPublicConstructors
            )]
                this Type type,
            bool useDefaultCtorInAnnotatedStructs,
            out ConstructorInfo? deserializationCtor
        )
        {
            ConstructorInfo? ctorWithAttribute = null;
            ConstructorInfo? publicParameterlessCtor = null;
            ConstructorInfo? lonePublicCtor = null;

            ConstructorInfo[] constructors = type.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance
            );

            if (constructors.Length == 1)
            {
                lonePublicCtor = constructors[0];
            }

            foreach (ConstructorInfo constructor in constructors)
            {
                if (HasKdlConstructorAttribute(constructor))
                {
                    if (ctorWithAttribute != null)
                    {
                        deserializationCtor = null;
                        return false;
                    }

                    ctorWithAttribute = constructor;
                }
                else if (constructor.GetParameters().Length == 0)
                {
                    publicParameterlessCtor = constructor;
                }
            }

            // Search for non-public ctors with [KdlConstructor].
            foreach (
                ConstructorInfo constructor in type.GetConstructors(
                    BindingFlags.NonPublic | BindingFlags.Instance
                )
            )
            {
                if (HasKdlConstructorAttribute(constructor))
                {
                    if (ctorWithAttribute != null)
                    {
                        deserializationCtor = null;
                        return false;
                    }

                    ctorWithAttribute = constructor;
                }
            }

            // Structs will use default constructor if attribute isn't used.
            if (useDefaultCtorInAnnotatedStructs && type.IsValueType && ctorWithAttribute == null)
            {
                deserializationCtor = null;
                return true;
            }

            deserializationCtor = ctorWithAttribute ?? publicParameterlessCtor ?? lonePublicCtor;
            return true;
        }

        public static object? GetDefaultValue(this ParameterInfo parameterInfo)
        {
            Type parameterType = parameterInfo.ParameterType;
            object? defaultValue = parameterInfo.DefaultValue;

            if (defaultValue is null)
            {
                return null;
            }

            // DBNull.Value is sometimes used as the default value (returned by reflection) of nullable params in place of null.
            if (defaultValue == DBNull.Value && parameterType != typeof(DBNull))
            {
                return null;
            }

            // Default values of enums or nullable enums are represented using the underlying type and need to be cast explicitly
            // cf. https://github.com/dotnet/runtime/issues/68647
            if (parameterType.IsEnum)
            {
                return Enum.ToObject(parameterType, defaultValue);
            }

            if (
                Nullable.GetUnderlyingType(parameterType) is Type underlyingType
                && underlyingType.IsEnum
            )
            {
                return Enum.ToObject(underlyingType, defaultValue);
            }

            return defaultValue;
        }

        /// <summary>
        /// Returns the type hierarchy for the given type, starting from the current type up to the base type(s) in the hierarchy.
        /// Interface hierarchies with multiple inheritance will return results using topological sorting.
        /// </summary>
        [RequiresUnreferencedCode("Should only be used by the reflection-based serializer.")]
        public static Type[] GetSortedTypeHierarchy(this Type type)
        {
            if (!type.IsInterface)
            {
                // Non-interface hierarchies are linear, just walk up to the earliest ancestor.

                var results = new List<Type>();
                for (Type? current = type; current != null; current = current.BaseType)
                {
                    results.Add(current);
                }

                return [.. results];
            }
            else
            {
                // Interface hierarchies support multiple inheritance.
                // For consistency with class hierarchy resolution order,
                // sort topologically from most derived to least derived.
                return KdlHelpers.TraverseGraphWithTopologicalSort(
                    type,
                    static t => t.GetInterfaces()
                );
            }
        }
    }
}
