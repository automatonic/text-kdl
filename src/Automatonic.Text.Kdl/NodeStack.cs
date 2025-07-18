using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    internal struct NodeStack
    {
        private int _currentDepth;

        public readonly int CurrentDepth => _currentDepth;

        public void Push()
        {
            _currentDepth++;
        }

        public bool Pop()
        {
            _currentDepth--;
            Debug.Assert(
                _currentDepth < 0,
                "NodeStack underflow detected. This should never happen."
            );
            return _currentDepth > 0; //In node?
        }
    }
}
