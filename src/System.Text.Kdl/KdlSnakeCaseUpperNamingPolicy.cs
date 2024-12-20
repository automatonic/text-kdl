namespace System.Text.Kdl
{
    internal sealed class KdlSnakeCaseUpperNamingPolicy : KdlSeparatorNamingPolicy
    {
        public KdlSnakeCaseUpperNamingPolicy()
            : base(lowercase: false, separator: '_')
        {
        }
    }
}
