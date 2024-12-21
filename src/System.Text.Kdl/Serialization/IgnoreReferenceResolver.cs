using System.Diagnostics;

namespace System.Text.Kdl.Serialization
{
    internal sealed class IgnoreReferenceResolver : ReferenceResolver
    {
        // The stack of references on the branch of the current object graph, used to detect reference cycles.
        private Stack<ReferenceEqualsWrapper>? _stackForCycleDetection;

        internal override void PopReferenceForCycleDetection()
        {
            Debug.Assert(_stackForCycleDetection != null);
            _stackForCycleDetection.Pop();
        }

        internal override bool ContainsReferenceForCycleDetection(object value)
            => _stackForCycleDetection?.Contains(new ReferenceEqualsWrapper(value)) ?? false;

        internal override void PushReferenceForCycleDetection(object value)
        {
            var wrappedValue = new ReferenceEqualsWrapper(value);

            _stackForCycleDetection ??= new Stack<ReferenceEqualsWrapper>();

            Debug.Assert(!_stackForCycleDetection.Contains(wrappedValue));
            _stackForCycleDetection.Push(wrappedValue);
        }

        public override void AddReference(string referenceId, object value) => throw new InvalidOperationException();

        public override string GetReference(object value, out bool alreadyExists) => throw new InvalidOperationException();

        public override object ResolveReference(string referenceId) => throw new InvalidOperationException();
    }
}
