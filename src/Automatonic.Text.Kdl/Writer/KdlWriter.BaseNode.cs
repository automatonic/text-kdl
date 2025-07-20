namespace Automatonic.Text.Kdl
{
    public sealed partial class KdlWriter
    {
        private bool _anyNodeHasBeenWritten;

        /// <summary>
        /// Writes the KDL document version (2) declaration.
        /// </summary>
        /// <remarks>
        /// version :=
        ///     '/-' unicode-space* 'kdl-version' unicode-space+ ('1' | '2')
        ///     unicode-space* newline
        /// </remarks>
        internal void WriteBaseNode()
        {
            _anyNodeHasBeenWritten = true;
            //TODO
        }
    }
}
