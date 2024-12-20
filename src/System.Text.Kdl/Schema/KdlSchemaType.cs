namespace System.Text.Kdl.Schema
{
    [Flags]
    internal enum KdlSchemaType
    {
        Any = 0,
        Null = 1,
        Boolean = 2,
        Integer = 4,
        Number = 8,
        String = 16,
        Object = 32,
        Array = 64,
    }
}
