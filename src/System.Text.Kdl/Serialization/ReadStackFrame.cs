using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal struct ReadStackFrame
    {
        // Current property values.
        public KdlPropertyInfo? KdlPropertyInfo;
        public StackFramePropertyState PropertyState;
        public bool UseExtensionProperty;

        // Support KDL Path on exceptions and non-string Dictionary keys.
        // This is Utf8 since we don't want to convert to string until an exception is thrown.
        // For dictionary keys we don't want to convert to TKey until we have both key and value when parsing the dictionary elements on stream cases.
        public byte[]? KdlPropertyName;
        public string? KdlPropertyNameAsString; // This is used for string dictionary keys and re-entry cases that specify a property name.

        // Stores the non-string dictionary keys for continuation.
        public object? DictionaryKey;

        /// <summary>
        /// Records the KdlReader Depth at the start of the current value.
        /// </summary>
        public int OriginalDepth;
#if DEBUG
        /// <summary>
        /// Records the KdlReader TokenType at the start of the current value.
        /// Only used to validate debug builds.
        /// </summary>
        public KdlTokenType OriginalTokenType;
#endif

        // Current object (POCO or IEnumerable).
        public object? ReturnValue; // The current return value used for re-entry.
        public KdlTypeInfo KdlTypeInfo;
        public StackFrameObjectState ObjectState; // State tracking the current object.

        // Current object can contain metadata
        public bool CanContainMetadata;
        public MetadataPropertyName LatestMetadataPropertyName;
        public MetadataPropertyName MetadataPropertyNames;

        // Serialization state for value serialized by the current frame.
        public PolymorphicSerializationState PolymorphicSerializationState;

        // Holds any entered polymorphic KdlTypeInfo metadata.
        public KdlTypeInfo? PolymorphicKdlTypeInfo;

        // Gets the initial KdlTypeInfo metadata used when deserializing the current value.
        public KdlTypeInfo BaseKdlTypeInfo
            => PolymorphicSerializationState == PolymorphicSerializationState.PolymorphicReEntryStarted
                ? PolymorphicKdlTypeInfo!
                : KdlTypeInfo;

        // For performance, we order the properties by the first deserialize and PropertyIndex helps find the right slot quicker.
        public int PropertyIndex;

        // Tracks newly encounentered UTF-8 encoded properties during the current deserialization, to be appended to the cache.
        public PropertyRefCacheBuilder? PropertyRefCacheBuilder;

        // Holds relevant state when deserializing objects with parameterized constructors.
        public ArgumentState? CtorArgumentState;

        // Whether to use custom number handling.
        public KdlNumberHandling? NumberHandling;

        // Represents required properties which have value assigned.
        // Each bit corresponds to a required property.
        // False means that property is not set (not yet occurred in the payload).
        // Length of the BitArray is equal to number of required properties.
        // Every required KdlPropertyInfo has RequiredPropertyIndex property which maps to an index in this BitArray.
        public BitArray? RequiredPropertiesSet;

        // Tracks state related to property population.
        public bool HasParentObject;
        public bool IsPopulating;

        public void EndConstructorParameter()
        {
            CtorArgumentState!.KdlParameterInfo = null;
            KdlPropertyName = null;
            PropertyState = StackFramePropertyState.None;
        }

        public void EndProperty()
        {
            KdlPropertyInfo = null!;
            KdlPropertyName = null;
            KdlPropertyNameAsString = null;
            PropertyState = StackFramePropertyState.None;

            // No need to clear these since they are overwritten each time:
            //  NumberHandling
            //  UseExtensionProperty
        }

        public void EndElement()
        {
            KdlPropertyNameAsString = null;
            PropertyState = StackFramePropertyState.None;
        }

        /// <summary>
        /// Is the current object a Dictionary.
        /// </summary>
        public bool IsProcessingDictionary()
        {
            return KdlTypeInfo.Kind is KdlTypeInfoKind.Dictionary;
        }

        /// <summary>
        /// Is the current object an Enumerable.
        /// </summary>
        public bool IsProcessingEnumerable()
        {
            return KdlTypeInfo.Kind is KdlTypeInfoKind.Enumerable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkRequiredPropertyAsRead(KdlPropertyInfo propertyInfo)
        {
            if (propertyInfo.IsRequired)
            {
                Debug.Assert(RequiredPropertiesSet != null);
                RequiredPropertiesSet[propertyInfo.RequiredPropertyIndex] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InitializeRequiredPropertiesValidationState(KdlTypeInfo typeInfo)
        {
            Debug.Assert(RequiredPropertiesSet == null);

            if (typeInfo.NumberOfRequiredProperties > 0)
            {
                RequiredPropertiesSet = new BitArray(typeInfo.NumberOfRequiredProperties);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ValidateAllRequiredPropertiesAreRead(KdlTypeInfo typeInfo)
        {
            if (typeInfo.NumberOfRequiredProperties > 0)
            {
                Debug.Assert(RequiredPropertiesSet != null);

                if (!RequiredPropertiesSet.HasAllSet())
                {
                    ThrowHelper.ThrowKdlException_KdlRequiredPropertyMissing(typeInfo, RequiredPropertiesSet);
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"ConverterStrategy.{KdlTypeInfo?.Converter.ConverterStrategy}, {KdlTypeInfo?.Type.Name}";
    }
}
