namespace Automatonic.Text.Kdl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0075:Simplify conditional expression",
        Justification = "<Pending>"
    )]
    internal static class AppContextSwitchHelper
    {
        public static bool IsSourceGenReflectionFallbackEnabled { get; } =
            AppContext.TryGetSwitch(
                switchName: "Automatonic.Text.Kdl.Serialization.EnableSourceGenReflectionFallback",
                isEnabled: out bool value
            )
                ? value
                : false;

        public static bool RespectNullableAnnotationsDefault { get; } =
            AppContext.TryGetSwitch(
                switchName: "Automatonic.Text.Kdl.Serialization.RespectNullableAnnotationsDefault",
                isEnabled: out bool value
            )
                ? value
                : false;

        public static bool RespectRequiredConstructorParametersDefault { get; } =
            AppContext.TryGetSwitch(
                switchName: "Automatonic.Text.Kdl.Serialization.RespectRequiredConstructorParametersDefault",
                isEnabled: out bool value
            )
                ? value
                : false;
    }
}
