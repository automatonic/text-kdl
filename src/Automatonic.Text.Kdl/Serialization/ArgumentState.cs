using Automatonic.Text.Kdl.Serialization.Metadata;
using FoundProperties = System.ValueTuple<
    Automatonic.Text.Kdl.Serialization.Metadata.KdlPropertyInfo,
    Automatonic.Text.Kdl.KdlReaderState,
    long,
    byte[]?,
    string?
>;
using FoundPropertiesAsync = System.ValueTuple<
    Automatonic.Text.Kdl.Serialization.Metadata.KdlPropertyInfo,
    object?,
    string?
>;

namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// Holds relevant state when deserializing objects with parameterized constructors.
    /// Lives on the current ReadStackFrame.
    /// </summary>
    internal sealed class ArgumentState
    {
        // Cache for parsed constructor arguments.
        public object Arguments = null!;

        // When deserializing objects with parameterized ctors, the properties we find on the first pass.
        public FoundProperties[]? FoundProperties;

        // When deserializing objects with parameterized ctors asynchronously, the properties we find on the first pass.
        public FoundPropertiesAsync[]? FoundPropertiesAsync;
        public int FoundPropertyCount;

        // Current constructor parameter value.
        public KdlParameterInfo? KdlParameterInfo;
    }
}
