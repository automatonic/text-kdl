namespace System.Text.Kdl
{
    internal static partial class KdlConstants
    {
        // Standard format for double and single on non-inbox frameworks.
        public const string DoubleFormatString = "G17";
        public const string SingleFormatString = "G9";

        public const int StackallocByteThreshold = 256;
        public const int StackallocCharThreshold = StackallocByteThreshold / 2;
    }
}