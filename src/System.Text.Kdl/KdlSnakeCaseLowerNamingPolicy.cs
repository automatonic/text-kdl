namespace System.Text.Kdl
{
    internal sealed class KdlSnakeCaseLowerNamingPolicy : KdlSeparatorNamingPolicy
    {
        public KdlSnakeCaseLowerNamingPolicy()
            : base(lowercase: true, separator: '_')
        {
        }
    }
}
