using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// A specialized converter implementation used for root-level value
    /// streaming in the KdlSerializer.DeserializeAsyncEnumerable methods.
    /// </summary>
    internal sealed class RootLevelListConverter<T> : KdlResumableConverter<List<T?>>
    {
        private readonly KdlTypeInfo<T> _elementTypeInfo;
        private protected sealed override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.Enumerable;
        internal override Type? ElementType => typeof(T);

        public RootLevelListConverter(KdlTypeInfo<T> elementTypeInfo)
        {
            IsRootLevelMultiContentStreamingConverter = true;
            _elementTypeInfo = elementTypeInfo;
        }

        internal override bool OnTryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out List<T?>? value)
        {
            Debug.Assert(reader.AllowMultipleValues, "Can only be used by readers allowing trailing content.");

            KdlConverter<T> elementConverter = _elementTypeInfo.EffectiveConverter;
            state.Current.KdlPropertyInfo = _elementTypeInfo.PropertyInfoForTypeInfo;
            var results = (List<T?>?)state.Current.ReturnValue;

            while (true)
            {
                if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                {
                    if (!reader.TryAdvanceToNextRootLevelValueWithOptionalReadAhead(elementConverter.RequiresReadAhead, out bool isAtEndOfStream))
                    {
                        if (isAtEndOfStream)
                        {
                            // No more root-level KDL values in the stream
                            // complete the deserialization process.
                            value = results;
                            return true;
                        }

                        // New root-level KDL value found, need to read more data.
                        value = default;
                        return false;
                    }

                    state.Current.PropertyState = StackFramePropertyState.ReadValue;
                }

                // Deserialize the next root-level KDL value.
                if (!elementConverter.TryRead(ref reader, typeof(T), options, ref state, out T? element, out _))
                {
                    value = default;
                    return false;
                }

                if (results is null)
                {
                    state.Current.ReturnValue = results = [];
                }

                results.Add(element);
                state.Current.EndElement();
            }
        }
    }
}
