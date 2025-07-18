namespace Automatonic.Text.Kdl
{
    /// <summary>
    /// The current state of a property that supports continuation.
    /// The values are typically compared with the less-than operator so the ordering is important.
    /// </summary>
    internal enum StackFramePropertyState : byte
    {
        None = 0,

        ReadName, // Read the name of the property.
        Name, // Verify or process the name.
        ReadValue, // Read the value of the property.
        ReadValueIsEnd, // Determine if we are done reading.
        TryRead, // Perform the actual call to the converter's TryRead().
    }
}
