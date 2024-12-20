namespace System.Text.Kdl
{
    internal sealed class KdlKebabCaseLowerNamingPolicy : KdlSeparatorNamingPolicy
    {
        public KdlKebabCaseLowerNamingPolicy()
            : base(lowercase: true, separator: '-')
        {
        }
    }
}
