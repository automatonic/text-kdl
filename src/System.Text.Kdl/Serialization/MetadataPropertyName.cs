namespace System.Text.Kdl
{
    [Flags]
    internal enum MetadataPropertyName : byte
    {
        None = 0,
        Values = 1,
        Id = 2,
        Ref = 4,
        Type = 8,
    }
}
