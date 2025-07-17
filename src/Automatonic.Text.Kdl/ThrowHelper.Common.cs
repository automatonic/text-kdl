namespace Automatonic.Text.Kdl;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

internal static partial class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowArgumentNullException(string parameterName) =>
        throw new ArgumentNullException(parameterName);

    internal static string Format(string format, object? arg0) =>
        string.Format(CultureInfo.InvariantCulture, format, arg0);

    internal static string Format(string format, object? arg0, object? arg1) =>
        string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);

    internal static string Format(string format, object? arg0, object? arg1, object? arg2) =>
        string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);

    internal static string Format(string format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format, args);
}
