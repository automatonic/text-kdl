using System.Runtime.CompilerServices;

namespace Automatonic.Text.Kdl
{
    /// <summary>Provides tools for avoiding stack overflows.</summary>
    internal static class StackHelper
    {
        /// <summary>Tries to ensure there is sufficient stack to execute the average .NET function.</summary>
        public static bool TryEnsureSufficientExecutionStack()
        {
            return RuntimeHelpers.TryEnsureSufficientExecutionStack();
        }
    }
}
