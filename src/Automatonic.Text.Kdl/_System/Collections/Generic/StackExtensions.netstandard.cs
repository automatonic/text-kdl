using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic
{
    /// <summary>Polyfills for <see cref="Stack{T}"/>.</summary>
    internal static class StackExtensions
    {
        public static bool TryPeek<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T result)
        {
            if (stack.Count > 0)
            {
                result = stack.Peek();
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T result)
        {
            if (stack.Count > 0)
            {
                result = stack.Pop();
                return true;
            }

            result = default;
            return false;
        }
    }
}
