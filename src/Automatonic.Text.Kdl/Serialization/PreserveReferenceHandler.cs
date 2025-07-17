namespace Automatonic.Text.Kdl.Serialization
{
    internal sealed class PreserveReferenceHandler : ReferenceHandler
    {
        public override ReferenceResolver CreateResolver() => throw new InvalidOperationException();

        internal override ReferenceResolver CreateResolver(bool writing) =>
            new PreserveReferenceResolver(writing);
    }
}
