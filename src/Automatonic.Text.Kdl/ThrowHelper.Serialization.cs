using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using Automatonic.Text.Kdl.RandomAccess;
using Automatonic.Text.Kdl.Serialization;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentException_DeserializeWrongType(Type type, object value)
        {
            throw new ArgumentException(Format(SR.DeserializeWrongType, type, value.GetType()));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_SerializerDoesNotSupportComments(string paramName)
        {
            throw new ArgumentException(SR.KdlSerializerDoesNotSupportComments, paramName);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_SerializationNotSupported(Type propertyType)
        {
            throw new NotSupportedException(Format(SR.SerializationNotSupportedType, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_TypeRequiresAsyncSerialization(
            Type propertyType
        )
        {
            throw new NotSupportedException(
                Format(SR.TypeRequiresAsyncSerialization, propertyType)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DictionaryKeyTypeNotSupported(
            Type keyType,
            KdlConverter converter
        )
        {
            throw new NotSupportedException(
                Format(SR.DictionaryKeyTypeNotSupported, keyType, converter.GetType())
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_DeserializeUnableToConvertValue(Type propertyType)
        {
            throw new KdlException(Format(SR.DeserializeUnableToConvertValue, propertyType))
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowInvalidCastException_DeserializeUnableToAssignValue(
            Type typeOfValue,
            Type declaredType
        )
        {
            throw new InvalidCastException(
                Format(SR.DeserializeUnableToAssignValue, typeOfValue, declaredType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DeserializeUnableToAssignNull(
            Type declaredType
        )
        {
            throw new InvalidOperationException(
                Format(SR.DeserializeUnableToAssignNull, declaredType)
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_PropertyGetterDisallowNull(
            string propertyName,
            Type declaringType
        )
        {
            throw new KdlException(
                Format(SR.PropertyGetterDisallowNull, propertyName, declaringType)
            )
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_PropertySetterDisallowNull(
            string propertyName,
            Type declaringType
        )
        {
            throw new KdlException(
                Format(SR.PropertySetterDisallowNull, propertyName, declaringType)
            )
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_ConstructorParameterDisallowNull(
            string parameterName,
            Type declaringType
        )
        {
            throw new KdlException(
                Format(SR.ConstructorParameterDisallowNull, parameterName, declaringType)
            )
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPopulateNotSupportedByConverter(
            KdlPropertyInfo propertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ObjectCreationHandlingPopulateNotSupportedByConverter,
                    propertyInfo.Name,
                    propertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyMustHaveAGetter(
            KdlPropertyInfo propertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ObjectCreationHandlingPropertyMustHaveAGetter,
                    propertyInfo.Name,
                    propertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyValueTypeMustHaveASetter(
            KdlPropertyInfo propertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ObjectCreationHandlingPropertyValueTypeMustHaveASetter,
                    propertyInfo.Name,
                    propertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyCannotAllowPolymorphicDeserialization(
            KdlPropertyInfo propertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ObjectCreationHandlingPropertyCannotAllowPolymorphicDeserialization,
                    propertyInfo.Name,
                    propertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyCannotAllowReadOnlyMember(
            KdlPropertyInfo propertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ObjectCreationHandlingPropertyCannotAllowReadOnlyMember,
                    propertyInfo.Name,
                    propertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyCannotAllowReferenceHandling()
        {
            throw new InvalidOperationException(
                SR.ObjectCreationHandlingPropertyCannotAllowReferenceHandling
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ObjectCreationHandlingPropertyDoesNotSupportParameterizedConstructors()
        {
            throw new NotSupportedException(
                SR.ObjectCreationHandlingPropertyDoesNotSupportParameterizedConstructors
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_SerializationConverterRead(KdlConverter? converter)
        {
            throw new KdlException(Format(SR.SerializationConverterRead, converter))
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_SerializationConverterWrite(KdlConverter? converter)
        {
            throw new KdlException(Format(SR.SerializationConverterWrite, converter))
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_SerializerCycleDetected(int maxDepth)
        {
            throw new KdlException(Format(SR.SerializerCycleDetected, maxDepth))
            {
                AppendPathInformation = true,
            };
        }

        [DoesNotReturn]
        public static void ThrowKdlException(string? message = null)
        {
            throw new KdlException(message) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_CannotSerializeInvalidType(
            string paramName,
            Type typeToConvert,
            Type? declaringType,
            string? propertyName
        )
        {
            if (declaringType == null)
            {
                Debug.Assert(propertyName == null);
                throw new ArgumentException(
                    Format(SR.CannotSerializeInvalidType, typeToConvert),
                    paramName
                );
            }

            Debug.Assert(propertyName != null);
            throw new ArgumentException(
                Format(SR.CannotSerializeInvalidMember, typeToConvert, propertyName, declaringType),
                paramName
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_CannotSerializeInvalidType(
            Type typeToConvert,
            Type? declaringType,
            MemberInfo? memberInfo
        )
        {
            if (declaringType == null)
            {
                Debug.Assert(memberInfo == null);
                throw new InvalidOperationException(
                    Format(SR.CannotSerializeInvalidType, typeToConvert)
                );
            }

            Debug.Assert(memberInfo != null);
            throw new InvalidOperationException(
                Format(
                    SR.CannotSerializeInvalidMember,
                    typeToConvert,
                    memberInfo.Name,
                    declaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterNotCompatible(
            Type converterType,
            Type type
        )
        {
            throw new InvalidOperationException(
                Format(SR.SerializationConverterNotCompatible, converterType, type)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ResolverTypeNotCompatible(
            Type requestedType,
            Type actualType
        )
        {
            throw new InvalidOperationException(
                Format(SR.ResolverTypeNotCompatible, actualType, requestedType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ResolverTypeInfoOptionsNotCompatible()
        {
            throw new InvalidOperationException(SR.ResolverTypeInfoOptionsNotCompatible);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlSerializerOptionsNoTypeInfoResolverSpecified()
        {
            throw new InvalidOperationException(SR.KdlSerializerOptionsNoTypeInfoResolverSpecified);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlSerializerIsReflectionDisabled()
        {
            throw new InvalidOperationException(SR.KdlSerializerIsReflectionDisabled);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeInvalid(
            Type classType,
            MemberInfo? memberInfo
        )
        {
            string location = classType.ToString();
            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(
                Format(SR.SerializationConverterOnAttributeInvalid, location)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(
            Type classTypeAttributeIsOn,
            MemberInfo? memberInfo,
            Type typeToConvert
        )
        {
            string location = classTypeAttributeIsOn.ToString();

            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(
                Format(SR.SerializationConverterOnAttributeNotCompatible, location, typeToConvert)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerOptionsReadOnly(
            KdlSerializerContext? context
        )
        {
            string message =
                context == null
                    ? SR.SerializerOptionsReadOnly
                    : SR.SerializerContextOptionsReadOnly;

            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DefaultTypeInfoResolverImmutable()
        {
            throw new InvalidOperationException(SR.DefaultTypeInfoResolverImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeInfoResolverChainImmutable()
        {
            throw new InvalidOperationException(SR.TypeInfoResolverChainImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeInfoImmutable()
        {
            throw new InvalidOperationException(SR.TypeInfoImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_InvalidChainedResolver()
        {
            throw new InvalidOperationException(SR.SerializerOptions_InvalidChainedResolver);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerPropertyNameConflict(
            Type type,
            string propertyName
        )
        {
            throw new InvalidOperationException(
                Format(SR.SerializerPropertyNameConflict, type, propertyName)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerPropertyNameNull(
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.SerializerPropertyNameNull,
                    kdlPropertyInfo.DeclaringType,
                    kdlPropertyInfo.MemberName
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlPropertyRequiredAndNotDeserializable(
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.KdlPropertyRequiredAndNotDeserializable,
                    kdlPropertyInfo.Name,
                    kdlPropertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlPropertyRequiredAndExtensionData(
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.KdlPropertyRequiredAndExtensionData,
                    kdlPropertyInfo.Name,
                    kdlPropertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_KdlRequiredPropertyMissing(
            KdlTypeInfo parent,
            BitArray requiredPropertiesSet
        )
        {
            StringBuilder listOfMissingPropertiesBuilder = new();
            bool first = true;

            // Soft cut-off length - once message becomes longer than that we won't be adding more elements
            const int CutOffLength = 60;

            foreach (KdlPropertyInfo property in parent.PropertyCache)
            {
                if (!property.IsRequired || requiredPropertiesSet[property.RequiredPropertyIndex])
                {
                    continue;
                }

                if (!first)
                {
                    listOfMissingPropertiesBuilder.Append(
                        CultureInfo.CurrentUICulture.TextInfo.ListSeparator
                    );
                    listOfMissingPropertiesBuilder.Append(' ');
                }

                listOfMissingPropertiesBuilder.Append('\'');
                listOfMissingPropertiesBuilder.Append(property.Name);
                listOfMissingPropertiesBuilder.Append('\'');
                first = false;

                if (listOfMissingPropertiesBuilder.Length >= CutOffLength)
                {
                    break;
                }
            }

            throw new KdlException(
                Format(
                    SR.KdlRequiredPropertiesMissing,
                    parent.Type,
                    listOfMissingPropertiesBuilder.ToString()
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NamingPolicyReturnNull(
            KdlNamingPolicy namingPolicy
        )
        {
            throw new InvalidOperationException(Format(SR.NamingPolicyReturnNull, namingPolicy));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsNull(
            Type converterType
        )
        {
            throw new InvalidOperationException(
                Format(SR.SerializerConverterFactoryReturnsNull, converterType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsKdlConverterFactorty(
            Type converterType
        )
        {
            throw new InvalidOperationException(
                Format(SR.SerializerConverterFactoryReturnsKdlConverterFactory, converterType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MultiplePropertiesBindToConstructorParameters(
            Type parentType,
            string parameterName,
            string firstMatchName,
            string secondMatchName
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.MultipleMembersBindWithConstructorParameter,
                    firstMatchName,
                    secondMatchName,
                    parentType,
                    parameterName
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ConstructorParameterIncompleteBinding(
            Type parentType
        )
        {
            throw new InvalidOperationException(
                Format(SR.ConstructorParamIncompleteBinding, parentType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExtensionDataCannotBindToCtorParam(
            string propertyName,
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ExtensionDataCannotBindToCtorParam,
                    propertyName,
                    kdlPropertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlIncludeOnInaccessibleProperty(
            string memberName,
            Type declaringType
        )
        {
            throw new InvalidOperationException(
                Format(SR.KdlIncludeOnInaccessibleProperty, memberName, declaringType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_IgnoreConditionOnValueTypeInvalid(
            string clrPropertyName,
            Type propertyDeclaringType
        )
        {
            throw new InvalidOperationException(
                Format(SR.IgnoreConditionOnValueTypeInvalid, clrPropertyName, propertyDeclaringType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NumberHandlingOnPropertyInvalid(
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            Debug.Assert(!kdlPropertyInfo.IsForTypeInfo);
            throw new InvalidOperationException(
                Format(
                    SR.NumberHandlingOnPropertyInvalid,
                    kdlPropertyInfo.MemberName,
                    kdlPropertyInfo.DeclaringType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ConverterCanConvertMultipleTypes(
            Type runtimePropertyType,
            KdlConverter kdlConverter
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ConverterCanConvertMultipleTypes,
                    kdlConverter.GetType(),
                    kdlConverter.Type,
                    runtimePropertyType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ObjectWithParameterizedCtorRefMetadataNotSupported(
            ReadOnlySpan<byte> propertyName,
            ref KdlReader reader,
            scoped ref ReadStack state
        )
        {
            KdlTypeInfo kdlTypeInfo = state.GetTopKdlTypeInfoWithParameterizedConstructor();
            state.Current.KdlPropertyName = propertyName.ToArray();

            NotSupportedException ex = new NotSupportedException(
                Format(SR.ObjectWithParameterizedCtorRefMetadataNotSupported, kdlTypeInfo.Type)
            );
            ThrowNotSupportedException(ref state, reader, ex);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(
            KdlTypeInfoKind kind
        )
        {
            throw new InvalidOperationException(
                Format(SR.InvalidKdlTypeInfoOperationForKind, kind)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlTypeInfoOnDeserializingCallbacksNotSupported(
            Type type
        )
        {
            throw new InvalidOperationException(
                Format(SR.OnDeserializingCallbacksNotSupported, type)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_CreateObjectConverterNotCompatible(
            Type type
        )
        {
            throw new InvalidOperationException(
                Format(SR.CreateObjectConverterNotCompatible, type)
            );
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(scoped ref ReadStack state, KdlReaderException ex)
        {
            Debug.Assert(ex.Path == null);

            string path = state.KdlPath();
            string message = ex.Message;

            // Insert the "Path" portion before "LineNumber" and "BytePositionInLine".
            int iPos = message.AsSpan().LastIndexOf(" LineNumber: ");

            if (iPos >= 0)
            {
                message = $"{message[..iPos]} Path: {path} |{message[iPos..]}";
            }
            else
            {
                message += $" Path: {path}.";
            }

            throw new KdlException(message, path, ex.LineNumber, ex.BytePositionInLine, ex);
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(
            scoped ref ReadStack state,
            in KdlReader reader,
            Exception ex
        )
        {
            KdlException kdlException = new KdlException(null, ex);
            AddKdlExceptionInformation(ref state, reader, kdlException);
            throw kdlException;
        }

        public static void AddKdlExceptionInformation(
            scoped ref ReadStack state,
            in KdlReader reader,
            KdlException ex
        )
        {
            Debug.Assert(ex.Path is null); // do not overwrite existing path information

            long lineNumber = reader.CurrentState._lineNumber;
            ex.LineNumber = lineNumber;

            long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            ex.BytePositionInLine = bytePositionInLine;

            string path = state.KdlPath();
            ex.Path = path;

            string? message = ex._message;

            if (string.IsNullOrEmpty(message))
            {
                // Use a default message.
                Type propertyType =
                    state.Current.KdlPropertyInfo?.PropertyType ?? state.Current.KdlTypeInfo.Type;
                message = Format(SR.DeserializeUnableToConvertValue, propertyType);
                ex.AppendPathInformation = true;
            }

            if (ex.AppendPathInformation)
            {
                message +=
                    $" Path: {path} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";
                ex.SetMessage(message);
            }
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(ref WriteStack state, Exception ex)
        {
            KdlException kdlException = new KdlException(null, ex);
            AddKdlExceptionInformation(ref state, kdlException);
            throw kdlException;
        }

        public static void AddKdlExceptionInformation(ref WriteStack state, KdlException ex)
        {
            Debug.Assert(ex.Path is null); // do not overwrite existing path information

            string path = state.PropertyPath();
            ex.Path = path;

            string? message = ex._message;
            if (string.IsNullOrEmpty(message))
            {
                // Use a default message.
                message = Format(SR.SerializeUnableToSerialize);
                ex.AppendPathInformation = true;
            }

            if (ex.AppendPathInformation)
            {
                message += $" Path: {path}.";
                ex.SetMessage(message);
            }
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateAttribute(
            Type attribute,
            MemberInfo memberInfo
        )
        {
            string location = memberInfo is Type type
                ? type.ToString()
                : $"{memberInfo.DeclaringType}.{memberInfo.Name}";
            throw new InvalidOperationException(
                Format(SR.SerializationDuplicateAttribute, attribute, location)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(
            Type classType,
            Type attribute
        )
        {
            throw new InvalidOperationException(
                Format(SR.SerializationDuplicateTypeAttribute, classType, attribute)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateTypeAttribute<TAttribute>(
            Type classType
        )
        {
            throw new InvalidOperationException(
                Format(SR.SerializationDuplicateTypeAttribute, classType, typeof(TAttribute))
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExtensionDataConflictsWithUnmappedMemberHandling(
            Type classType,
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.ExtensionDataConflictsWithUnmappedMemberHandling,
                    classType,
                    kdlPropertyInfo.MemberName
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDataExtensionPropertyInvalid(
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.SerializationDataExtensionPropertyInvalid,
                    kdlPropertyInfo.PropertyType,
                    kdlPropertyInfo.MemberName
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PropertyTypeNotNullable(
            KdlPropertyInfo kdlPropertyInfo
        )
        {
            throw new InvalidOperationException(
                Format(SR.PropertyTypeNotNullable, kdlPropertyInfo.PropertyType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeKdlObjectCustomConverterNotAllowedOnExtensionProperty()
        {
            throw new InvalidOperationException(
                SR.NodeKdlObjectCustomConverterNotAllowedOnExtensionProperty
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(
            scoped ref ReadStack state,
            in KdlReader reader,
            Exception innerException
        )
        {
            string message = innerException.Message;

            // The caller should check to ensure path is not already set.
            Debug.Assert(!message.Contains(" Path: "));

            // Obtain the type to show in the message.
            Type propertyType =
                state.Current.KdlPropertyInfo?.PropertyType ?? state.Current.KdlTypeInfo.Type;

            if (!message.Contains(propertyType.ToString()))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += Format(SR.SerializationNotSupportedParentType, propertyType);
            }

            long lineNumber = reader.CurrentState._lineNumber;
            long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            message +=
                $" Path: {state.KdlPath()} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";

            throw new NotSupportedException(message, innerException);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(
            ref WriteStack state,
            Exception innerException
        )
        {
            string message = innerException.Message;

            // The caller should check to ensure path is not already set.
            Debug.Assert(!message.Contains(" Path: "));

            // Obtain the type to show in the message.
            Type propertyType =
                state.Current.KdlPropertyInfo?.PropertyType ?? state.Current.KdlTypeInfo.Type;

            if (!message.Contains(propertyType.ToString()))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += Format(SR.SerializationNotSupportedParentType, propertyType);
            }

            message += $" Path: {state.PropertyPath()}.";

            throw new NotSupportedException(message, innerException);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DeserializeNoConstructor(
            KdlTypeInfo typeInfo,
            ref KdlReader reader,
            scoped ref ReadStack state
        )
        {
            Type type = typeInfo.Type;
            string message;

            if (type.IsInterface || type.IsAbstract)
            {
                if (typeInfo.PolymorphicTypeResolver?.UsesTypeDiscriminators is true)
                {
                    message = Format(SR.DeserializationMustSpecifyTypeDiscriminator, type);
                }
                else if (typeInfo.Kind is KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary)
                {
                    message = Format(SR.CannotPopulateCollection, type);
                }
                else
                {
                    message = Format(SR.DeserializeInterfaceOrAbstractType, type);
                }
            }
            else
            {
                message = Format(
                    SR.DeserializeNoConstructor,
                    nameof(KdlConstructorAttribute),
                    type
                );
            }

            ThrowNotSupportedException(ref state, reader, new NotSupportedException(message));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_CannotPopulateCollection(
            Type type,
            ref KdlReader reader,
            scoped ref ReadStack state
        )
        {
            ThrowNotSupportedException(
                ref state,
                reader,
                new NotSupportedException(Format(SR.CannotPopulateCollection, type))
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataValuesInvalidToken(KdlTokenType tokenType)
        {
            ThrowKdlException(Format(SR.MetadataInvalidTokenAfterValues, tokenType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataReferenceNotFound(string id)
        {
            ThrowKdlException(Format(SR.MetadataReferenceNotFound, id));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataValueWasNotString(KdlTokenType tokenType)
        {
            ThrowKdlException(Format(SR.MetadataValueWasNotString, tokenType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataValueWasNotString(KdlValueKind valueKind)
        {
            ThrowKdlException(Format(SR.MetadataValueWasNotString, valueKind));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataReferenceObjectCannotContainOtherProperties(
            ReadOnlySpan<byte> propertyName,
            scoped ref ReadStack state
        )
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException_MetadataReferenceObjectCannotContainOtherProperties();
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataUnexpectedProperty(
            ReadOnlySpan<byte> propertyName,
            scoped ref ReadStack state
        )
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException(Format(SR.MetadataUnexpectedProperty));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_UnmappedKdlProperty(
            Type type,
            string unmappedPropertyName
        )
        {
            throw new KdlException(Format(SR.UnmappedKdlProperty, unmappedPropertyName, type));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataReferenceObjectCannotContainOtherProperties()
        {
            ThrowKdlException(SR.MetadataReferenceCannotContainOtherProperties);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataIdCannotBeCombinedWithRef(
            ReadOnlySpan<byte> propertyName,
            scoped ref ReadStack state
        )
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException(SR.MetadataIdCannotBeCombinedWithRef);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataStandaloneValuesProperty(
            scoped ref ReadStack state,
            ReadOnlySpan<byte> propertyName
        )
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException(SR.MetadataStandaloneValuesProperty);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataInvalidPropertyWithLeadingDollarSign(
            ReadOnlySpan<byte> propertyName,
            scoped ref ReadStack state,
            in KdlReader reader
        )
        {
            // Set PropertyInfo or KeyName to write down the conflicting property name in KdlException.Path
            if (state.Current.IsProcessingDictionary())
            {
                state.Current.KdlPropertyNameAsString = reader.GetString();
            }
            else
            {
                state.Current.KdlPropertyName = propertyName.ToArray();
            }

            ThrowKdlException(SR.MetadataInvalidPropertyWithLeadingDollarSign);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataDuplicateIdFound(string id)
        {
            ThrowKdlException(Format(SR.MetadataDuplicateIdFound, id));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_DuplicateMetadataProperty(
            ReadOnlySpan<byte> utf8PropertyName
        )
        {
            ThrowKdlException(
                Format(SR.DuplicateMetadataProperty, KdlHelpers.Utf8GetString(utf8PropertyName))
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataInvalidReferenceToValueType(Type propertyType)
        {
            ThrowKdlException(Format(SR.MetadataInvalidReferenceToValueType, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataInvalidPropertyInArrayMetadata(
            scoped ref ReadStack state,
            Type propertyType,
            in KdlReader reader
        )
        {
            state.Current.KdlPropertyName = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan.ToArray();
            string propertyNameAsString = reader.GetString()!;

            ThrowKdlException(
                Format(
                    SR.MetadataPreservedArrayFailed,
                    Format(SR.MetadataInvalidPropertyInArrayMetadata, propertyNameAsString),
                    Format(SR.DeserializeUnableToConvertValue, propertyType)
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataPreservedArrayValuesNotFound(
            scoped ref ReadStack state,
            Type propertyType
        )
        {
            // Missing $values, KDL path should point to the property's object.
            state.Current.KdlPropertyName = null;

            ThrowKdlException(
                Format(
                    SR.MetadataPreservedArrayFailed,
                    SR.MetadataStandaloneValuesProperty,
                    Format(SR.DeserializeUnableToConvertValue, propertyType)
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataCannotParsePreservedObjectIntoImmutable(
            Type propertyType
        )
        {
            ThrowKdlException(
                Format(SR.MetadataCannotParsePreservedObjectToImmutable, propertyType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MetadataReferenceOfTypeCannotBeAssignedToType(
            string referenceId,
            Type currentType,
            Type typeToConvert
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.MetadataReferenceOfTypeCannotBeAssignedToType,
                    referenceId,
                    currentType,
                    typeToConvert
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlPropertyInfoIsBoundToDifferentKdlTypeInfo(
            KdlPropertyInfo propertyInfo
        )
        {
            Debug.Assert(
                propertyInfo.DeclaringTypeInfo != null,
                "We should not throw this exception when ParentTypeInfo is null"
            );
            throw new InvalidOperationException(
                Format(
                    SR.KdlPropertyInfoBoundToDifferentParent,
                    propertyInfo.Name,
                    propertyInfo.DeclaringTypeInfo.Type.FullName
                )
            );
        }

        [DoesNotReturn]
        internal static void ThrowUnexpectedMetadataException(
            ReadOnlySpan<byte> propertyName,
            ref KdlReader reader,
            scoped ref ReadStack state
        )
        {
            MetadataPropertyName name = KdlSerializer.GetMetadataPropertyName(
                propertyName,
                state.Current.BaseKdlTypeInfo.PolymorphicTypeResolver
            );
            if (name != 0)
            {
                ThrowKdlException_MetadataUnexpectedProperty(propertyName, ref state);
            }
            else
            {
                ThrowKdlException_MetadataInvalidPropertyWithLeadingDollarSign(
                    propertyName,
                    ref state,
                    reader
                );
            }
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_NoMetadataForType(
            Type type,
            IKdlTypeInfoResolver? resolver
        )
        {
            throw new NotSupportedException(
                Format(SR.NoMetadataForType, type, resolver?.ToString() ?? "<null>")
            );
        }

        public static NotSupportedException GetNotSupportedException_AmbiguousMetadataForType(
            Type type,
            Type match1,
            Type match2
        )
        {
            return new NotSupportedException(
                Format(SR.AmbiguousMetadataForType, type, match1, match2)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ConstructorContainsNullParameterNames(
            Type declaringType
        )
        {
            throw new NotSupportedException(
                Format(SR.ConstructorContainsNullParameterNames, declaringType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NoMetadataForType(
            Type type,
            IKdlTypeInfoResolver? resolver
        )
        {
            throw new InvalidOperationException(
                Format(SR.NoMetadataForType, type, resolver?.ToString() ?? "<null>")
            );
        }

        public static Exception GetInvalidOperationException_NoMetadataForTypeProperties(
            IKdlTypeInfoResolver? resolver,
            Type type
        )
        {
            return new InvalidOperationException(
                Format(SR.NoMetadataForTypeProperties, resolver?.ToString() ?? "<null>", type)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NoMetadataForTypeProperties(
            IKdlTypeInfoResolver? resolver,
            Type type
        )
        {
            throw GetInvalidOperationException_NoMetadataForTypeProperties(resolver, type);
        }

        [DoesNotReturn]
        public static void ThrowMissingMemberException_MissingFSharpCoreMember(
            string missingFsharpCoreMember
        )
        {
            throw new MissingMemberException(
                Format(SR.MissingFSharpCoreMember, missingFsharpCoreMember)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_BaseConverterDoesNotSupportMetadata(
            Type derivedType
        )
        {
            throw new NotSupportedException(
                Format(SR.Polymorphism_DerivedConverterDoesNotSupportMetadata, derivedType)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DerivedConverterDoesNotSupportMetadata(
            Type derivedType
        )
        {
            throw new NotSupportedException(
                Format(SR.Polymorphism_DerivedConverterDoesNotSupportMetadata, derivedType)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_RuntimeTypeNotSupported(
            Type baseType,
            Type runtimeType
        )
        {
            throw new NotSupportedException(
                Format(SR.Polymorphism_RuntimeTypeNotSupported, runtimeType, baseType)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_RuntimeTypeDiamondAmbiguity(
            Type baseType,
            Type runtimeType,
            Type derivedType1,
            Type derivedType2
        )
        {
            throw new NotSupportedException(
                Format(
                    SR.Polymorphism_RuntimeTypeDiamondAmbiguity,
                    runtimeType,
                    derivedType1,
                    derivedType2,
                    baseType
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeDoesNotSupportPolymorphism(
            Type baseType
        )
        {
            throw new InvalidOperationException(
                Format(SR.Polymorphism_TypeDoesNotSupportPolymorphism, baseType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DerivedTypeNotSupported(
            Type baseType,
            Type derivedType
        )
        {
            throw new InvalidOperationException(
                Format(SR.Polymorphism_DerivedTypeIsNotSupported, derivedType, baseType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DerivedTypeIsAlreadySpecified(
            Type baseType,
            Type derivedType
        )
        {
            throw new InvalidOperationException(
                Format(SR.Polymorphism_DerivedTypeIsAlreadySpecified, baseType, derivedType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeDicriminatorIdIsAlreadySpecified(
            Type baseType,
            object typeDiscriminator
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.Polymorphism_TypeDicriminatorIdIsAlreadySpecified,
                    baseType,
                    typeDiscriminator
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_InvalidCustomTypeDiscriminatorPropertyName()
        {
            throw new InvalidOperationException(
                SR.Polymorphism_InvalidCustomTypeDiscriminatorPropertyName
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PropertyConflictsWithMetadataPropertyName(
            Type type,
            string propertyName
        )
        {
            throw new InvalidOperationException(
                Format(
                    SR.Polymorphism_PropertyConflictsWithMetadataPropertyName,
                    type,
                    propertyName
                )
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PolymorphicTypeConfigurationDoesNotSpecifyDerivedTypes(
            Type baseType
        )
        {
            throw new InvalidOperationException(
                Format(SR.Polymorphism_ConfigurationDoesNotSpecifyDerivedTypes, baseType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_UnsupportedEnumIdentifier(
            Type enumType,
            string? enumName
        )
        {
            throw new InvalidOperationException(
                Format(SR.UnsupportedEnumIdentifier, enumType.Name, enumName)
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlException_UnrecognizedTypeDiscriminator(object typeDiscriminator)
        {
            ThrowKdlException(
                Format(SR.Polymorphism_UnrecognizedTypeDiscriminator, typeDiscriminator)
            );
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_KdlPolymorphismOptionsAssociatedWithDifferentKdlTypeInfo(
            string parameterName
        )
        {
            throw new ArgumentException(
                SR.KdlPolymorphismOptionsAssociatedWithDifferentKdlTypeInfo,
                paramName: parameterName
            );
        }

        [DoesNotReturn]
        public static void ThrowOperationCanceledException_PipeWriteCanceled()
        {
            throw new OperationCanceledException(SR.PipeWriterCanceled);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PipeWriterDoesNotImplementUnflushedBytes(
            PipeWriter pipeWriter
        )
        {
            throw new InvalidOperationException(
                Format(SR.PipeWriter_DoesNotImplementUnflushedBytes, pipeWriter.GetType().Name)
            );
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_KdlSchemaExporterDoesNotSupportReferenceHandlerPreserve()
        {
            throw new NotSupportedException(
                SR.KdlSchemaExporter_ReferenceHandlerPreserve_NotSupported
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlSchemaExporterDepthTooLarge()
        {
            throw new InvalidOperationException(SR.KdlSchemaExporter_DepthTooLarge);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlWriter_DocumentBomOnlyAtStart()
        {
            throw new InvalidOperationException(SR.KdlWriter_DocumentBomOnlyAtStart);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlWriter_VersionMustPrecedeAnyNode()
        {
            throw new InvalidOperationException(SR.KdlWriter_VersionMustPrecedeAnyNode);
        }
    }
}
