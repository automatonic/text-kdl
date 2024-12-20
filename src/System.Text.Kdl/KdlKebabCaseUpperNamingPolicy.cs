namespace System.Text.Kdl
{
    internal sealed class KdlKebabCaseUpperNamingPolicy : KdlSeparatorNamingPolicy
    {
        public KdlKebabCaseUpperNamingPolicy()
            : base(lowercase: false, separator: '-')
        {
        }
    }
}
