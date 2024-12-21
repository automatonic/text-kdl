using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace System.Text.Kdl
{
    /// <summary>
    /// A struct variant of <see cref="Queue{T}"/> that only allocates for Counts > 1.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal struct ValueQueue<T>
    {
        private byte _state; // 0 = empty, 1 = single, 2 = multiple
        private T? _single;
        private Queue<T>? _multiple;

        public readonly int Count => _state < 2 ? _state : _multiple!.Count;

        public void Enqueue(T value)
        {
            switch (_state)
            {
                case 0:
                    _single = value;
                    _state = 1;
                    break;

                case 1:
                    // Once a queue gets allocated the struct will always remain in the multiple state.
                    (_multiple ??= new()).Enqueue(_single!);
                    _single = default;
                    _state = 2;
                    goto default;

                default:
                    Debug.Assert(_multiple != null);
                    _multiple.Enqueue(value);
                    break;
            }
        }

        public bool TryDequeue([MaybeNullWhen(false)] out T? value)
        {
            switch (_state)
            {
                case 0:
                    value = default;
                    return false;

                case 1:
                    value = _single;
                    _single = default;
                    _state = 0;
                    return true;

                default:
                    Debug.Assert(_multiple != null);
                    return _multiple.TryDequeue(out value);
            }
        }
    }
}
