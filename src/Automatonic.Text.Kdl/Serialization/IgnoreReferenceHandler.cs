namespace Automatonic.Text.Kdl.Serialization
{
    internal sealed class IgnoreReferenceHandler : ReferenceHandler
    {
        public IgnoreReferenceHandler() => HandlingStrategy = KdlKnownReferenceHandler.IgnoreCycles;

        public override ReferenceResolver CreateResolver() => new IgnoreReferenceResolver();
    }
}
