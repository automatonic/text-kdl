using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization;

namespace Automatonic.Text.Kdl
{
    public static partial class KdlSerializer
    {
        // Pre-encoded metadata properties.
        internal static readonly KdlEncodedText s_metadataId = KdlEncodedText.Encode(IdPropertyName, encoder: null);
        internal static readonly KdlEncodedText s_metadataRef = KdlEncodedText.Encode(RefPropertyName, encoder: null);
        internal static readonly KdlEncodedText s_metadataType = KdlEncodedText.Encode(TypePropertyName, encoder: null);
        internal static readonly KdlEncodedText s_metadataValues = KdlEncodedText.Encode(ValuesPropertyName, encoder: null);

        internal static MetadataPropertyName WriteMetadataForObject(
            KdlConverter jsonConverter,
            ref WriteStack state,
            KdlWriter writer)
        {
            Debug.Assert(jsonConverter.CanHaveMetadata);
            Debug.Assert(!state.IsContinuation);
            Debug.Assert(state.CurrentContainsMetadata);

            MetadataPropertyName writtenMetadata = MetadataPropertyName.None;

            if (state.NewReferenceId != null)
            {
                writer.WriteString(s_metadataId, state.NewReferenceId);
                writtenMetadata |= MetadataPropertyName.Id;
                state.NewReferenceId = null;
            }

            if (state.PolymorphicTypeDiscriminator is object discriminator)
            {
                Debug.Assert(state.PolymorphicTypeResolver != null);

                KdlEncodedText propertyName =
                    state.PolymorphicTypeResolver.CustomTypeDiscriminatorPropertyNameKdlEncoded is KdlEncodedText customPropertyName
                    ? customPropertyName
                    : s_metadataType;

                if (discriminator is string stringId)
                {
                    writer.WriteString(propertyName, stringId);
                }
                else
                {
                    Debug.Assert(discriminator is int);
                    writer.WriteNumber(propertyName, (int)discriminator);
                }

                writtenMetadata |= MetadataPropertyName.Type;
                state.PolymorphicTypeDiscriminator = null;
            }

            Debug.Assert(writtenMetadata != MetadataPropertyName.None);
            return writtenMetadata;
        }

        internal static MetadataPropertyName WriteMetadataForCollection(
            KdlConverter jsonConverter,
            ref WriteStack state,
            KdlWriter writer)
        {
            // For collections with metadata, we nest the array payload within a KDL object.
            writer.WriteStartObject();
            MetadataPropertyName writtenMetadata = WriteMetadataForObject(jsonConverter, ref state, writer);
            writer.WritePropertyName(s_metadataValues); // property name containing nested array values.
            return writtenMetadata;
        }

        /// <summary>
        /// Compute reference id for the next value to be serialized.
        /// </summary>
        internal static bool TryGetReferenceForValue(object currentValue, ref WriteStack state, KdlWriter writer)
        {
            Debug.Assert(state.NewReferenceId == null);

            string referenceId = state.ReferenceResolver.GetReference(currentValue, out bool alreadyExists);
            Debug.Assert(referenceId != null);

            if (alreadyExists)
            {
                // Instance already serialized, write as { "$ref" : "referenceId" }
                writer.WriteStartObject();
                writer.WriteString(s_metadataRef, referenceId);
                writer.WriteEndObject();

                // clear out any polymorphism state.
                state.PolymorphicTypeDiscriminator = null;
                state.PolymorphicTypeResolver = null;
            }
            else
            {
                // New instance, store computed reference id in the state
                state.NewReferenceId = referenceId;
            }

            return alreadyExists;
        }
    }
}
