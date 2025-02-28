using System.Collections;
using System.Diagnostics;

namespace System.Text.Kdl.RandomAccess
{
    public partial struct KdlReadOnlyElement
    {
        /// <summary>
        ///   An enumerable and enumerator for the contents of a KDL array.
        /// </summary>
        [DebuggerDisplay("{Current,nq}")]
        public struct ArrayEnumerator : IEnumerable<KdlReadOnlyElement>, IEnumerator<KdlReadOnlyElement>
        {
            private readonly KdlReadOnlyElement _target;
            private int _curIdx;
            private readonly int _endIdxOrVersion;

            internal ArrayEnumerator(KdlReadOnlyElement target)
            {
                _target = target;
                _curIdx = -1;

                Debug.Assert(target.TokenType == KdlTokenType.StartArray);

                _endIdxOrVersion = target._parent.GetEndIndex(_target._idx, includeEndElement: false);
            }

            /// <inheritdoc />
            public readonly KdlReadOnlyElement Current
            {
                get
                {
                    if (_curIdx < 0)
                    {
                        return default;
                    }

                    return new KdlReadOnlyElement(_target._parent, _curIdx);
                }
            }

            /// <summary>
            ///   Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            ///   An <see cref="ArrayEnumerator"/> value that can be used to iterate
            ///   through the array.
            /// </returns>
            public readonly ArrayEnumerator GetEnumerator()
            {
                ArrayEnumerator ator = this;
                ator._curIdx = -1;
                return ator;
            }

            /// <inheritdoc />
            readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            readonly IEnumerator<KdlReadOnlyElement> IEnumerable<KdlReadOnlyElement>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public void Dispose()
            {
                _curIdx = _endIdxOrVersion;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _curIdx = -1;
            }

            /// <inheritdoc />
            readonly object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_curIdx >= _endIdxOrVersion)
                {
                    return false;
                }

                if (_curIdx < 0)
                {
                    _curIdx = _target._idx + KdlReadOnlyDocument.DbRow.Size;
                }
                else
                {
                    _curIdx = _target._parent.GetEndIndex(_curIdx, includeEndElement: true);
                }

                return _curIdx < _endIdxOrVersion;
            }
        }
    }
}
