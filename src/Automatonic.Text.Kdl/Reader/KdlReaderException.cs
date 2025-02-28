namespace Automatonic.Text.Kdl
{
    // This class exists because the serializer needs to catch reader-originated exceptions in order to throw KdlException which has Path information.
    [Serializable]
    internal sealed class KdlReaderException(string message, long lineNumber, long bytePositionInLine) : KdlException(message, path: null, lineNumber, bytePositionInLine)
    {
    }
}