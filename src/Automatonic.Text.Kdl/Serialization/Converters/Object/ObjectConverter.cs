using System.Diagnostics;
using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Schema;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal abstract class ObjectConverter : KdlConverter<object?>
    {
        private protected override ConverterStrategy GetDefaultConverterStrategy() => ConverterStrategy.Object;

        public ObjectConverter() => CanBePolymorphic = true;

        public sealed override object ReadAsPropertyName(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            ThrowHelper.ThrowNotSupportedException_DictionaryKeyTypeNotSupported(Type, this);
            return null!;
        }

        internal sealed override object ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            ThrowHelper.ThrowNotSupportedException_DictionaryKeyTypeNotSupported(Type, this);
            return null!;
        }

        public sealed override void Write(KdlWriter writer, object? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteEndObject();
        }

        public sealed override void WriteAsPropertyName(KdlWriter writer, object value, KdlSerializerOptions options)
        {
            WriteAsPropertyNameCore(writer, value, options, isWritingExtensionDataProperty: false);
        }

        internal sealed override void WriteAsPropertyNameCore(KdlWriter writer, object value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

            Type runtimeType = value.GetType();
            if (runtimeType == Type)
            {
                ThrowHelper.ThrowNotSupportedException_DictionaryKeyTypeNotSupported(runtimeType, this);
            }

            KdlConverter runtimeConverter = options.GetConverterInternal(runtimeType);
            runtimeConverter.WriteAsPropertyNameCoreAsObject(writer, value, options, isWritingExtensionDataProperty);
        }
    }

    /// <summary>
    /// Defines an object converter that only supports (polymorphic) serialization but not deserialization.
    /// This is done to avoid rooting dependencies to KdlVertex/KdlElement necessary to drive object deserialization.
    /// Source generator users need to explicitly declare support for object so that the derived converter gets used.
    /// </summary>
    internal sealed class SlimObjectConverter(IKdlTypeInfoResolver originatingResolver) : ObjectConverter
    {
        // Keep track of the originating resolver so that the converter surfaces
        // an accurate error message whenever deserialization is attempted.
        private readonly IKdlTypeInfoResolver _originatingResolver = originatingResolver;

        public override object? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            ThrowHelper.ThrowNotSupportedException_NoMetadataForType(typeToConvert, _originatingResolver);
            return null;
        }
    }

    /// <summary>
    /// Defines an object converter that supports deserialization via KdlElement/KdlVertex representations.
    /// Used as the default in reflection or if object is declared in the KdlSerializerContext type graph.
    /// </summary>
    internal sealed class DefaultObjectConverter : ObjectConverter
    {
        public DefaultObjectConverter() =>
            // KdlElement/KdlVertex parsing does not support async; force read ahead for now.
            RequiresReadAhead = true;

        public override object? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (options.UnknownTypeHandling == KdlUnknownTypeHandling.KdlElement)
            {
                return KdlReadOnlyElement.ParseValue(ref reader);
            }

            Debug.Assert(options.UnknownTypeHandling == KdlUnknownTypeHandling.KdlVertex);
            return KdlVertexConverter.Instance.Read(ref reader, typeToConvert, options);
        }

        internal override bool OnTryRead(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options, scoped ref ReadStack state, out object? value)
        {
            object? referenceValue;

            if (options.UnknownTypeHandling == KdlUnknownTypeHandling.KdlElement)
            {
                KdlReadOnlyElement element = KdlReadOnlyElement.ParseValue(ref reader);

                // Edge case where we want to lookup for a reference when parsing into typeof(object)
                if (options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve &&
                    KdlSerializer.TryHandleReferenceFromKdlElement(ref reader, ref state, element, out referenceValue))
                {
                    value = referenceValue;
                }
                else
                {
                    value = element;
                }

                return true;
            }

            Debug.Assert(options.UnknownTypeHandling == KdlUnknownTypeHandling.KdlVertex);

            KdlElement? node = KdlVertexConverter.Instance.Read(ref reader, typeToConvert, options);

            if (options.ReferenceHandlingStrategy == KdlKnownReferenceHandler.Preserve &&
                KdlSerializer.TryHandleReferenceFromKdlNode(ref reader, ref state, node, out referenceValue))
            {
                value = referenceValue;
            }
            else
            {
                value = node;
            }

            return true;
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) => KdlSchema.CreateTrueSchema();
    }
}
