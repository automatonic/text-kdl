using System.Collections;
using System.Diagnostics;

namespace System.Text.Kdl
{
    public partial struct KdlReadOnlyElement
    {
        /// <summary>
        ///   An enumerable and enumerator for the properties of a KDL object.
        /// </summary>
        [DebuggerDisplay("{Current,nq}")]
        public struct NodeEnumerator : IEnumerable<IKdlEntry>, IEnumerator<IKdlEntry>
        {
            private readonly KdlReadOnlyElement _target;
            private int _curIdx;
            private readonly int _endIdxOrVersion;

            internal NodeEnumerator(KdlReadOnlyElement target)
            {
                _target = target;
                _curIdx = -1;

                Debug.Assert(target.TokenType == KdlTokenType.StartObject);
                _endIdxOrVersion = target._parent.GetEndIndex(_target._idx, includeEndElement: false);
            }

            /// <inheritdoc />
            public readonly IKdlEntry Current
            {
                get
                {
                    if (_curIdx < 0)
                    {
                        return default!;
                    }

                    return new KdlProperty(new KdlReadOnlyElement(_target._parent, _curIdx));
                }
            }

            /// <summary>
            ///   Returns an enumerator that iterates the properties of an object.
            /// </summary>
            /// <returns>
            ///   An <see cref="NodeEnumerator"/> value that can be used to iterate
            ///   through the object.
            /// </returns>
            /// <remarks>
            ///   The enumerator will enumerate the properties in the order they are
            ///   declared, and when an object has multiple definitions of a single
            ///   property they will all individually be returned (each in the order
            ///   they appear in the content).
            /// </remarks>
            public readonly NodeEnumerator GetEnumerator()
            {
                NodeEnumerator ator = this;
                ator._curIdx = -1;
                return ator;
            }

            /// <inheritdoc />
            readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            readonly IEnumerator<IKdlEntry> IEnumerable<IKdlEntry>.GetEnumerator() => GetEnumerator();

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

                // _curIdx is now pointing at a property name, move one more to get the value
                _curIdx += KdlReadOnlyDocument.DbRow.Size;

                return _curIdx < _endIdxOrVersion;
            }
        }
    }
}
