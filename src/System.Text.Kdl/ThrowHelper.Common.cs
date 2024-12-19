using System.Diagnostics.CodeAnalysis;

namespace System.Text.Kdl
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}