using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Automatonic.Text.Kdl.Reflection;

namespace Automatonic.Text.Kdl.Serialization
{
    internal static class IEnumerableConverterFactoryHelpers
    {
        // Automatonic.Text.Kdl doesn't take a direct reference to System.Collections.Immutable so
        // any netstandard2.0 consumers don't need to reference System.Collections.Immutable.
        // So instead, implement a "weak reference" by using strings to check for Immutable types.

        // Don't use DynamicDependency attributes to the Immutable Collection types so they can be trimmed in applications that don't use Immutable Collections.
        internal const string ImmutableConvertersUnreferencedCodeMessage =
            "System.Collections.Immutable converters use Reflection to find and create Immutable Collection types, which requires unreferenced code.";

        [RequiresUnreferencedCode(ImmutableConvertersUnreferencedCodeMessage)]
        [RequiresDynamicCode(ImmutableConvertersUnreferencedCodeMessage)]
        public static MethodInfo GetImmutableEnumerableCreateRangeMethod(
            this Type type,
            Type elementType
        )
        {
            Type? constructingType = GetImmutableEnumerableConstructingType(type);
            if (constructingType != null)
            {
                MethodInfo[] constructingTypeMethods = constructingType.GetMethods();
                foreach (MethodInfo method in constructingTypeMethods)
                {
                    if (
                        method.Name == ReflectionExtensions.CreateRangeMethodName
                        && method.GetParameters().Length == 1
                        && method.IsGenericMethod
                        && method.GetGenericArguments().Length == 1
                    )
                    {
                        return method.MakeGenericMethod(elementType);
                    }
                }
            }

            ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
            return null!;
        }

        [RequiresUnreferencedCode(ImmutableConvertersUnreferencedCodeMessage)]
        [RequiresDynamicCode(ImmutableConvertersUnreferencedCodeMessage)]
        public static MethodInfo GetImmutableDictionaryCreateRangeMethod(
            this Type type,
            Type keyType,
            Type valueType
        )
        {
            Type? constructingType = GetImmutableDictionaryConstructingType(type);
            if (constructingType != null)
            {
                MethodInfo[] constructingTypeMethods = constructingType.GetMethods();
                foreach (MethodInfo method in constructingTypeMethods)
                {
                    if (
                        method.Name == ReflectionExtensions.CreateRangeMethodName
                        && method.GetParameters().Length == 1
                        && method.IsGenericMethod
                        && method.GetGenericArguments().Length == 2
                    )
                    {
                        return method.MakeGenericMethod(keyType, valueType);
                    }
                }
            }

            ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
            return null!;
        }

        [RequiresUnreferencedCode(ImmutableConvertersUnreferencedCodeMessage)]
        [RequiresDynamicCode(ImmutableConvertersUnreferencedCodeMessage)]
        private static Type? GetImmutableEnumerableConstructingType(Type type)
        {
            Debug.Assert(type.IsImmutableEnumerableType());

            string? constructingTypeName = type.GetImmutableEnumerableConstructingTypeName();

            return constructingTypeName == null
                ? null
                : type.Assembly.GetType(constructingTypeName);
        }

        [RequiresUnreferencedCode(ImmutableConvertersUnreferencedCodeMessage)]
        [RequiresDynamicCode(ImmutableConvertersUnreferencedCodeMessage)]
        private static Type? GetImmutableDictionaryConstructingType(Type type)
        {
            Debug.Assert(type.IsImmutableDictionaryType());

            string? constructingTypeName = type.GetImmutableDictionaryConstructingTypeName();

            return constructingTypeName == null
                ? null
                : type.Assembly.GetType(constructingTypeName);
        }

        public static bool IsNonGenericStackOrQueue(this Type type)
        {
            // Optimize for linking scenarios where mscorlib is trimmed out.
            const string stackTypeName = "System.Collections.Stack, System.Collections.NonGeneric";
            const string queueTypeName = "System.Collections.Queue, System.Collections.NonGeneric";

            Type? stackType = GetTypeIfExists(stackTypeName);
            if (stackType?.IsAssignableFrom(type) == true)
            {
                return true;
            }

            Type? queueType = GetTypeIfExists(queueTypeName);
            if (queueType?.IsAssignableFrom(type) == true)
            {
                return true;
            }

            return false;
        }

        // This method takes an unannotated string which makes trimming reflection analysis lose track of the type we are
        // looking for. This indirection allows the removal of the type if it is not used in the calling application.
        [UnconditionalSuppressMessage(
            "ReflectionAnalysis",
            "IL2057:TypeGetType",
            Justification = "This method exists to allow for 'weak references' to the Stack and Queue types. If those types are used in the app, "
                + "they will be preserved by the app and Type.GetType will return them. If those types are not used in the app, we don't want to preserve them here."
        )]
        private static Type? GetTypeIfExists(string name) => Type.GetType(name, false);
    }
}
