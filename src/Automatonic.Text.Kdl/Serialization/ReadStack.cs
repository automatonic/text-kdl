using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal struct ReadStack
    {
        /// <summary>
        /// Exposes the stack frame that is currently active.
        /// </summary>
        public ReadStackFrame Current;

        /// <summary>
        /// Gets the parent stack frame, if it exists.
        /// </summary>
        public readonly ref ReadStackFrame Parent
        {
            get
            {
                Debug.Assert(_count > 1);
                Debug.Assert(_stack is not null);
                return ref _stack[_count - 2];
            }
        }

        public readonly KdlPropertyInfo? ParentProperty
            => Current.HasParentObject ? Parent.KdlPropertyInfo : null;

        /// <summary>
        /// Buffer containing all frames in the stack. For performance it is only populated for serialization depths > 1.
        /// </summary>
        private ReadStackFrame[] _stack;

        /// <summary>
        /// Tracks the current depth of the stack.
        /// </summary>
        private int _count;

        /// <summary>
        /// If not zero, indicates that the stack is part of a re-entrant continuation of given depth.
        /// </summary>
        private int _continuationCount;

        /// <summary>
        /// Indicates that the state still contains suspended frames waiting re-entry.
        /// </summary>
        public readonly bool IsContinuation => _continuationCount != 0;

        // The bag of preservable references.
        public ReferenceResolver ReferenceResolver;

        /// <summary>
        /// Whether we need to read ahead in the inner read loop.
        /// </summary>
        public bool SupportContinuation;

        /// <summary>
        /// Holds the value of $id or $ref of the currently read object
        /// </summary>
        public string? ReferenceId;

        /// <summary>
        /// Holds the value of $type of the currently read object
        /// </summary>
        public object? PolymorphicTypeDiscriminator;

        /// <summary>
        /// Global flag indicating whether we can read preserved references.
        /// </summary>
        public bool PreserveReferences;

        /// <summary>
        /// Ensures that the stack buffer has sufficient capacity to hold an additional frame.
        /// </summary>
        private void EnsurePushCapacity()
        {
            if (_stack is null)
            {
                _stack = new ReadStackFrame[4];
            }
            else if (_count - 1 == _stack.Length)
            {
                Array.Resize(ref _stack, 2 * _stack.Length);
            }
        }

        internal void Initialize(KdlTypeInfo jsonTypeInfo, bool supportContinuation = false)
        {
            KdlSerializerOptions options = jsonTypeInfo.Options;
            if (options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve)
            {
                ReferenceResolver = options.ReferenceHandler!.CreateResolver(writing: false);
                PreserveReferences = true;
            }

            Current.KdlTypeInfo = jsonTypeInfo;
            Current.KdlPropertyInfo = jsonTypeInfo.PropertyInfoForTypeInfo;
            Current.NumberHandling = Current.KdlPropertyInfo.EffectiveNumberHandling;
            Current.CanContainMetadata = PreserveReferences || jsonTypeInfo.PolymorphicTypeResolver?.UsesTypeDiscriminators == true;
            SupportContinuation = supportContinuation;
        }

        public void Push()
        {
            if (_continuationCount == 0)
            {
                if (_count == 0)
                {
                    // Performance optimization: reuse the first stack frame on the first push operation.
                    // NB need to be careful when making writes to Current _before_ the first `Push`
                    // operation is performed.
                    _count = 1;
                }
                else
                {
                    KdlTypeInfo jsonTypeInfo = Current.KdlPropertyInfo?.KdlTypeInfo ?? Current.CtorArgumentState!.KdlParameterInfo!.KdlTypeInfo;
                    KdlNumberHandling? numberHandling = Current.NumberHandling;

                    EnsurePushCapacity();
                    _stack[_count - 1] = Current;
                    Current = default;
                    _count++;

                    Current.KdlTypeInfo = jsonTypeInfo;
                    Current.KdlPropertyInfo = jsonTypeInfo.PropertyInfoForTypeInfo;
                    // Allow number handling on property to win over handling on type.
                    Current.NumberHandling = numberHandling ?? Current.KdlPropertyInfo.EffectiveNumberHandling;
                    Current.CanContainMetadata = PreserveReferences || jsonTypeInfo.PolymorphicTypeResolver?.UsesTypeDiscriminators == true;
                }
            }
            else
            {
                // We are re-entering a continuation, adjust indices accordingly.

                if (_count++ > 0)
                {
                    _stack[_count - 2] = Current;
                    Current = _stack[_count - 1];
                }

                // check if we are done
                if (_continuationCount == _count)
                {
                    _continuationCount = 0;
                }
            }

            SetConstructorArgumentState();
#if DEBUG
            // Ensure the method is always exercised in debug builds.
            _ = KdlPath();
#endif
        }

        public void Pop(bool success)
        {
            Debug.Assert(_count > 0);
            Debug.Assert(KdlPath() is not null);

            if (!success)
            {
                // Check if we need to initialize the continuation.
                if (_continuationCount == 0)
                {
                    if (_count == 1)
                    {
                        // No need to copy any frames here.
                        _continuationCount = 1;
                        _count = 0;
                        return;
                    }

                    // Need to push the Current frame to the stack,
                    // ensure that we have sufficient capacity.
                    EnsurePushCapacity();
                    _continuationCount = _count--;
                }
                else if (--_count == 0)
                {
                    // reached the root, no need to copy frames.
                    return;
                }

                _stack[_count] = Current;
                Current = _stack[_count - 1];
            }
            else
            {
                Debug.Assert(_continuationCount == 0);

                if (--_count > 0)
                {
                    Current = _stack[_count - 1];
                }
            }
        }

        /// <summary>
        /// Configures the current stack frame for a polymorphic converter.
        /// </summary>
        public KdlConverter InitializePolymorphicReEntry(KdlTypeInfo derivedKdlTypeInfo)
        {
            Debug.Assert(!IsContinuation);
            Debug.Assert(Current.PolymorphicKdlTypeInfo == null);
            Debug.Assert(Current.PolymorphicSerializationState == PolymorphicSerializationState.None);

            Current.PolymorphicKdlTypeInfo = Current.KdlTypeInfo;
            Current.KdlTypeInfo = derivedKdlTypeInfo;
            Current.KdlPropertyInfo = derivedKdlTypeInfo.PropertyInfoForTypeInfo;
            Current.NumberHandling ??= Current.KdlPropertyInfo.NumberHandling;
            Current.PolymorphicSerializationState = PolymorphicSerializationState.PolymorphicReEntryStarted;
            SetConstructorArgumentState();

            return derivedKdlTypeInfo.Converter;
        }


        /// <summary>
        /// Configures the current frame for a continuation of a polymorphic converter.
        /// </summary>
        public KdlConverter ResumePolymorphicReEntry()
        {
            Debug.Assert(Current.PolymorphicKdlTypeInfo != null);
            Debug.Assert(Current.PolymorphicSerializationState == PolymorphicSerializationState.PolymorphicReEntrySuspended);

            // Swap out the two values as we resume the polymorphic converter
            (Current.KdlTypeInfo, Current.PolymorphicKdlTypeInfo) = (Current.PolymorphicKdlTypeInfo, Current.KdlTypeInfo);
            Current.PolymorphicSerializationState = PolymorphicSerializationState.PolymorphicReEntryStarted;
            return Current.KdlTypeInfo.Converter;
        }

        /// <summary>
        /// Updates frame state after a polymorphic converter has returned.
        /// </summary>
        public void ExitPolymorphicConverter(bool success)
        {
            Debug.Assert(Current.PolymorphicKdlTypeInfo != null);
            Debug.Assert(Current.PolymorphicSerializationState == PolymorphicSerializationState.PolymorphicReEntryStarted);

            // Swap out the two values as we exit the polymorphic converter
            (Current.KdlTypeInfo, Current.PolymorphicKdlTypeInfo) = (Current.PolymorphicKdlTypeInfo, Current.KdlTypeInfo);
            Current.PolymorphicSerializationState = success ? PolymorphicSerializationState.None : PolymorphicSerializationState.PolymorphicReEntrySuspended;
        }

        // Return a KDLPath using simple dot-notation when possible. When special characters are present, bracket-notation is used:
        // $.x.y[0].z
        // $['PropertyName.With.Special.Chars']
        public string KdlPath()
        {
            StringBuilder sb = new StringBuilder("$");

            (int frameCount, bool includeCurrentFrame) = _continuationCount switch
            {
                0 => (_count - 1, true), // Not a continuation, report previous frames and Current.
                1 => (0, true), // Continuation of depth 1, just report Current frame.
                int c => (c, false) // Continuation of depth > 1, report the entire stack.
            };

            for (int i = 0; i < frameCount; i++)
            {
                AppendStackFrame(sb, ref _stack[i]);
            }

            if (includeCurrentFrame)
            {
                AppendStackFrame(sb, ref Current);
            }

            return sb.ToString();

            static void AppendStackFrame(StringBuilder sb, ref ReadStackFrame frame)
            {
                // Append the property name.
                string? propertyName = GetPropertyName(ref frame);
                AppendPropertyName(sb, propertyName);

                if (frame.KdlTypeInfo != null && frame.IsProcessingEnumerable())
                {
                    if (frame.ReturnValue is not IEnumerable enumerable)
                    {
                        return;
                    }

                    // For continuation scenarios only, before or after all elements are read, the exception is not within the array.
                    if (frame.ObjectState is StackFrameObjectState.None or
                        StackFrameObjectState.CreatedObject or
                        StackFrameObjectState.ReadElements)
                    {
                        sb.Append('[');
                        sb.Append(GetCount(enumerable));
                        sb.Append(']');
                    }
                }
            }

            static int GetCount(IEnumerable enumerable)
            {
                if (enumerable is ICollection collection)
                {
                    return collection.Count;
                }

                int count = 0;
                IEnumerator enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    count++;
                }

                return count;
            }

            static void AppendPropertyName(StringBuilder sb, string? propertyName)
            {
                if (propertyName != null)
                {
                    if (propertyName.AsSpan().ContainsSpecialCharacters())
                    {
                        sb.Append(@"['");
                        sb.Append(propertyName);
                        sb.Append(@"']");
                    }
                    else
                    {
                        sb.Append('.');
                        sb.Append(propertyName);
                    }
                }
            }

            static string? GetPropertyName(ref ReadStackFrame frame)
            {
                string? propertyName = null;

                // Attempt to get the KDL property name from the frame.
                byte[]? utf8PropertyName = frame.KdlPropertyName;
                if (utf8PropertyName == null)
                {
                    if (frame.KdlPropertyNameAsString != null)
                    {
                        // Attempt to get the KDL property name set manually for dictionary
                        // keys and KeyValuePair property names.
                        propertyName = frame.KdlPropertyNameAsString;
                    }
                    else
                    {
                        // Attempt to get the KDL property name from the KdlPropertyInfo or KdlParameterInfo.
                        utf8PropertyName = frame.KdlPropertyInfo?.NameAsUtf8Bytes ??
                            frame.CtorArgumentState?.KdlParameterInfo?.KdlNameAsUtf8Bytes;
                    }
                }

                if (utf8PropertyName != null)
                {
                    propertyName = KdlHelpers.Utf8GetString(utf8PropertyName);
                }

                return propertyName;
            }
        }

        // Traverses the stack for the outermost object being deserialized using constructor parameters
        // Only called when calculating exception information.
        public readonly KdlTypeInfo GetTopKdlTypeInfoWithParameterizedConstructor()
        {
            Debug.Assert(!IsContinuation);

            for (int i = 0; i < _count - 1; i++)
            {
                if (_stack[i].KdlTypeInfo.UsesParameterizedConstructor)
                {
                    return _stack[i].KdlTypeInfo;
                }
            }

            Debug.Assert(Current.KdlTypeInfo.UsesParameterizedConstructor);
            return Current.KdlTypeInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetConstructorArgumentState()
        {
            if (Current.KdlTypeInfo.UsesParameterizedConstructor)
            {
                Current.CtorArgumentState ??= new();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Path = {KdlPath()}, Current = ConverterStrategy.{Current.KdlTypeInfo?.Converter.ConverterStrategy}, {Current.KdlTypeInfo?.Type.Name}";
    }
}
