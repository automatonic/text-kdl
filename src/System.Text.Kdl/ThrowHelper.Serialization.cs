// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipelines;
using System.Reflection;
using System.Text.Kdl.Serialization;
using System.Text.Kdl.Serialization.Metadata;

namespace System.Text.Kdl
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentException_DeserializeWrongType(Type type, object value)
        {
            throw new ArgumentException(string.Format(SR.DeserializeWrongType, type, value.GetType()));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_SerializerDoesNotSupportComments(string paramName)
        {
            throw new ArgumentException(SR.KdlSerializerDoesNotSupportComments, paramName);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_SerializationNotSupported(Type propertyType)
        {
            throw new NotSupportedException(string.Format(SR.SerializationNotSupportedType, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_TypeRequiresAsyncSerialization(Type propertyType)
        {
            throw new NotSupportedException(string.Format(SR.TypeRequiresAsyncSerialization, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DictionaryKeyTypeNotSupported(Type keyType, KdlConverter converter)
        {
            throw new NotSupportedException(string.Format(SR.DictionaryKeyTypeNotSupported, keyType, converter.GetType()));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_DeserializeUnableToConvertValue(Type propertyType)
        {
            throw new KdlException(string.Format(SR.DeserializeUnableToConvertValue, propertyType)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowInvalidCastException_DeserializeUnableToAssignValue(Type typeOfValue, Type declaredType)
        {
            throw new InvalidCastException(string.Format(SR.DeserializeUnableToAssignValue, typeOfValue, declaredType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DeserializeUnableToAssignNull(Type declaredType)
        {
            throw new InvalidOperationException(string.Format(SR.DeserializeUnableToAssignNull, declaredType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_PropertyGetterDisallowNull(string propertyName, Type declaringType)
        {
            throw new KdlException(string.Format(SR.PropertyGetterDisallowNull, propertyName, declaringType)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_PropertySetterDisallowNull(string propertyName, Type declaringType)
        {
            throw new KdlException(string.Format(SR.PropertySetterDisallowNull, propertyName, declaringType)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_ConstructorParameterDisallowNull(string parameterName, Type declaringType)
        {
            throw new KdlException(string.Format(SR.ConstructorParameterDisallowNull, parameterName, declaringType)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPopulateNotSupportedByConverter(KdlPropertyInfo propertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ObjectCreationHandlingPopulateNotSupportedByConverter, propertyInfo.Name, propertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyMustHaveAGetter(KdlPropertyInfo propertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ObjectCreationHandlingPropertyMustHaveAGetter, propertyInfo.Name, propertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyValueTypeMustHaveASetter(KdlPropertyInfo propertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ObjectCreationHandlingPropertyValueTypeMustHaveASetter, propertyInfo.Name, propertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyCannotAllowPolymorphicDeserialization(KdlPropertyInfo propertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ObjectCreationHandlingPropertyCannotAllowPolymorphicDeserialization, propertyInfo.Name, propertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyCannotAllowReadOnlyMember(KdlPropertyInfo propertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ObjectCreationHandlingPropertyCannotAllowReadOnlyMember, propertyInfo.Name, propertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ObjectCreationHandlingPropertyCannotAllowReferenceHandling()
        {
            throw new InvalidOperationException(SR.ObjectCreationHandlingPropertyCannotAllowReferenceHandling);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ObjectCreationHandlingPropertyDoesNotSupportParameterizedConstructors()
        {
            throw new NotSupportedException(SR.ObjectCreationHandlingPropertyDoesNotSupportParameterizedConstructors);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_SerializationConverterRead(KdlConverter? converter)
        {
            throw new KdlException(string.Format(SR.SerializationConverterRead, converter)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_SerializationConverterWrite(KdlConverter? converter)
        {
            throw new KdlException(string.Format(SR.SerializationConverterWrite, converter)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowKdlException_SerializerCycleDetected(int maxDepth)
        {
            throw new KdlException(string.Format(SR.SerializerCycleDetected, maxDepth)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowKdlException(string? message = null)
        {
            throw new KdlException(message) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_CannotSerializeInvalidType(string paramName, Type typeToConvert, Type? declaringType, string? propertyName)
        {
            if (declaringType == null)
            {
                Debug.Assert(propertyName == null);
                throw new ArgumentException(string.Format(SR.CannotSerializeInvalidType, typeToConvert), paramName);
            }

            Debug.Assert(propertyName != null);
            throw new ArgumentException(string.Format(SR.CannotSerializeInvalidMember, typeToConvert, propertyName, declaringType), paramName);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_CannotSerializeInvalidType(Type typeToConvert, Type? declaringType, MemberInfo? memberInfo)
        {
            if (declaringType == null)
            {
                Debug.Assert(memberInfo == null);
                throw new InvalidOperationException(string.Format(SR.CannotSerializeInvalidType, typeToConvert));
            }

            Debug.Assert(memberInfo != null);
            throw new InvalidOperationException(string.Format(SR.CannotSerializeInvalidMember, typeToConvert, memberInfo.Name, declaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterNotCompatible(Type converterType, Type type)
        {
            throw new InvalidOperationException(string.Format(SR.SerializationConverterNotCompatible, converterType, type));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ResolverTypeNotCompatible(Type requestedType, Type actualType)
        {
            throw new InvalidOperationException(string.Format(SR.ResolverTypeNotCompatible, actualType, requestedType));
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
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeInvalid(Type classType, MemberInfo? memberInfo)
        {
            string location = classType.ToString();
            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(string.Format(SR.SerializationConverterOnAttributeInvalid, location));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(Type classTypeAttributeIsOn, MemberInfo? memberInfo, Type typeToConvert)
        {
            string location = classTypeAttributeIsOn.ToString();

            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(string.Format(SR.SerializationConverterOnAttributeNotCompatible, location, typeToConvert));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerOptionsReadOnly(KdlSerializerContext? context)
        {
            string message = context == null
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
        public static void ThrowInvalidOperationException_SerializerPropertyNameConflict(Type type, string propertyName)
        {
            throw new InvalidOperationException(string.Format(SR.SerializerPropertyNameConflict, type, propertyName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerPropertyNameNull(KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.SerializerPropertyNameNull, jsonPropertyInfo.DeclaringType, jsonPropertyInfo.MemberName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlPropertyRequiredAndNotDeserializable(KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.KdlPropertyRequiredAndNotDeserializable, jsonPropertyInfo.Name, jsonPropertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlPropertyRequiredAndExtensionData(KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.KdlPropertyRequiredAndExtensionData, jsonPropertyInfo.Name, jsonPropertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_KdlRequiredPropertyMissing(KdlTypeInfo parent, BitArray requiredPropertiesSet)
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
                    listOfMissingPropertiesBuilder.Append(CultureInfo.CurrentUICulture.TextInfo.ListSeparator);
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

            throw new KdlException(string.Format(SR.KdlRequiredPropertiesMissing, parent.Type, listOfMissingPropertiesBuilder.ToString()));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NamingPolicyReturnNull(KdlNamingPolicy namingPolicy)
        {
            throw new InvalidOperationException(string.Format(SR.NamingPolicyReturnNull, namingPolicy));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsNull(Type converterType)
        {
            throw new InvalidOperationException(string.Format(SR.SerializerConverterFactoryReturnsNull, converterType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsKdlConverterFactorty(Type converterType)
        {
            throw new InvalidOperationException(string.Format(SR.SerializerConverterFactoryReturnsKdlConverterFactory, converterType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MultiplePropertiesBindToConstructorParameters(
            Type parentType,
            string parameterName,
            string firstMatchName,
            string secondMatchName)
        {
            throw new InvalidOperationException(
                string.Format(
                    SR.MultipleMembersBindWithConstructorParameter,
                    firstMatchName,
                    secondMatchName,
                    parentType,
                    parameterName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ConstructorParameterIncompleteBinding(Type parentType)
        {
            throw new InvalidOperationException(string.Format(SR.ConstructorParamIncompleteBinding, parentType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExtensionDataCannotBindToCtorParam(string propertyName, KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ExtensionDataCannotBindToCtorParam, propertyName, jsonPropertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlIncludeOnInaccessibleProperty(string memberName, Type declaringType)
        {
            throw new InvalidOperationException(string.Format(SR.KdlIncludeOnInaccessibleProperty, memberName, declaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_IgnoreConditionOnValueTypeInvalid(string clrPropertyName, Type propertyDeclaringType)
        {
            throw new InvalidOperationException(string.Format(SR.IgnoreConditionOnValueTypeInvalid, clrPropertyName, propertyDeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NumberHandlingOnPropertyInvalid(KdlPropertyInfo jsonPropertyInfo)
        {
            Debug.Assert(!jsonPropertyInfo.IsForTypeInfo);
            throw new InvalidOperationException(string.Format(SR.NumberHandlingOnPropertyInvalid, jsonPropertyInfo.MemberName, jsonPropertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ConverterCanConvertMultipleTypes(Type runtimePropertyType, KdlConverter jsonConverter)
        {
            throw new InvalidOperationException(string.Format(SR.ConverterCanConvertMultipleTypes, jsonConverter.GetType(), jsonConverter.Type, runtimePropertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ObjectWithParameterizedCtorRefMetadataNotSupported(
            ReadOnlySpan<byte> propertyName,
            ref KdlReader reader,
            scoped ref ReadStack state)
        {
            KdlTypeInfo jsonTypeInfo = state.GetTopKdlTypeInfoWithParameterizedConstructor();
            state.Current.KdlPropertyName = propertyName.ToArray();

            NotSupportedException ex = new NotSupportedException(
                string.Format(SR.ObjectWithParameterizedCtorRefMetadataNotSupported, jsonTypeInfo.Type));
            ThrowNotSupportedException(ref state, reader, ex);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlTypeInfoOperationNotPossibleForKind(KdlTypeInfoKind kind)
        {
            throw new InvalidOperationException(string.Format(SR.InvalidKdlTypeInfoOperationForKind, kind));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlTypeInfoOnDeserializingCallbacksNotSupported(Type type)
        {
            throw new InvalidOperationException(string.Format(SR.OnDeserializingCallbacksNotSupported, type));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_CreateObjectConverterNotCompatible(Type type)
        {
            throw new InvalidOperationException(string.Format(SR.CreateObjectConverterNotCompatible, type));
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(scoped ref ReadStack state, KdlReaderException ex)
        {
            Debug.Assert(ex.Path == null);

            string path = state.KdlPath();
            string message = ex.Message;

            // Insert the "Path" portion before "LineNumber" and "BytePositionInLine".
#if NET
            int iPos = message.AsSpan().LastIndexOf(" LineNumber: ");
#else
            int iPos = message.LastIndexOf(" LineNumber: ", StringComparison.Ordinal);
#endif
            if (iPos >= 0)
            {
                message = $"{message.Substring(0, iPos)} Path: {path} |{message.Substring(iPos)}";
            }
            else
            {
                message += $" Path: {path}.";
            }

            throw new KdlException(message, path, ex.LineNumber, ex.BytePositionInLine, ex);
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(scoped ref ReadStack state, in KdlReader reader, Exception ex)
        {
            KdlException jsonException = new KdlException(null, ex);
            AddKdlExceptionInformation(ref state, reader, jsonException);
            throw jsonException;
        }

        public static void AddKdlExceptionInformation(scoped ref ReadStack state, in KdlReader reader, KdlException ex)
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
                Type propertyType = state.Current.KdlPropertyInfo?.PropertyType ?? state.Current.KdlTypeInfo.Type;
                message = string.Format(SR.DeserializeUnableToConvertValue, propertyType);
                ex.AppendPathInformation = true;
            }

            if (ex.AppendPathInformation)
            {
                message += $" Path: {path} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";
                ex.SetMessage(message);
            }
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(ref WriteStack state, Exception ex)
        {
            KdlException jsonException = new KdlException(null, ex);
            AddKdlExceptionInformation(ref state, jsonException);
            throw jsonException;
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
                message = string.Format(SR.SerializeUnableToSerialize);
                ex.AppendPathInformation = true;
            }

            if (ex.AppendPathInformation)
            {
                message += $" Path: {path}.";
                ex.SetMessage(message);
            }
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateAttribute(Type attribute, MemberInfo memberInfo)
        {
            string location = memberInfo is Type type ? type.ToString() : $"{memberInfo.DeclaringType}.{memberInfo.Name}";
            throw new InvalidOperationException(string.Format(SR.SerializationDuplicateAttribute, attribute, location));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(Type classType, Type attribute)
        {
            throw new InvalidOperationException(string.Format(SR.SerializationDuplicateTypeAttribute, classType, attribute));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateTypeAttribute<TAttribute>(Type classType)
        {
            throw new InvalidOperationException(string.Format(SR.SerializationDuplicateTypeAttribute, classType, typeof(TAttribute)));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExtensionDataConflictsWithUnmappedMemberHandling(Type classType, KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.ExtensionDataConflictsWithUnmappedMemberHandling, classType, jsonPropertyInfo.MemberName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDataExtensionPropertyInvalid(KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.SerializationDataExtensionPropertyInvalid, jsonPropertyInfo.PropertyType, jsonPropertyInfo.MemberName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PropertyTypeNotNullable(KdlPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(SR.PropertyTypeNotNullable, jsonPropertyInfo.PropertyType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeKdlObjectCustomConverterNotAllowedOnExtensionProperty()
        {
            throw new InvalidOperationException(SR.NodeKdlObjectCustomConverterNotAllowedOnExtensionProperty);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(scoped ref ReadStack state, in KdlReader reader, Exception innerException)
        {
            string message = innerException.Message;

            // The caller should check to ensure path is not already set.
            Debug.Assert(!message.Contains(" Path: "));

            // Obtain the type to show in the message.
            Type propertyType = state.Current.KdlPropertyInfo?.PropertyType ?? state.Current.KdlTypeInfo.Type;

            if (!message.Contains(propertyType.ToString()))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += string.Format(SR.SerializationNotSupportedParentType, propertyType);
            }

            long lineNumber = reader.CurrentState._lineNumber;
            long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            message += $" Path: {state.KdlPath()} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";

            throw new NotSupportedException(message, innerException);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(ref WriteStack state, Exception innerException)
        {
            string message = innerException.Message;

            // The caller should check to ensure path is not already set.
            Debug.Assert(!message.Contains(" Path: "));

            // Obtain the type to show in the message.
            Type propertyType = state.Current.KdlPropertyInfo?.PropertyType ?? state.Current.KdlTypeInfo.Type;

            if (!message.Contains(propertyType.ToString()))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += string.Format(SR.SerializationNotSupportedParentType, propertyType);
            }

            message += $" Path: {state.PropertyPath()}.";

            throw new NotSupportedException(message, innerException);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DeserializeNoConstructor(KdlTypeInfo typeInfo, ref KdlReader reader, scoped ref ReadStack state)
        {
            Type type = typeInfo.Type;
            string message;

            if (type.IsInterface || type.IsAbstract)
            {
                if (typeInfo.PolymorphicTypeResolver?.UsesTypeDiscriminators is true)
                {
                    message = string.Format(SR.DeserializationMustSpecifyTypeDiscriminator, type);
                }
                else if (typeInfo.Kind is KdlTypeInfoKind.Enumerable or KdlTypeInfoKind.Dictionary)
                {
                    message = string.Format(SR.CannotPopulateCollection, type);
                }
                else
                {
                    message = string.Format(SR.DeserializeInterfaceOrAbstractType, type);
                }
            }
            else
            {
                message = string.Format(SR.DeserializeNoConstructor, nameof(KdlConstructorAttribute), type);
            }

            ThrowNotSupportedException(ref state, reader, new NotSupportedException(message));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_CannotPopulateCollection(Type type, ref KdlReader reader, scoped ref ReadStack state)
        {
            ThrowNotSupportedException(ref state, reader, new NotSupportedException(string.Format(SR.CannotPopulateCollection, type)));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataValuesInvalidToken(KdlTokenType tokenType)
        {
            ThrowKdlException(string.Format(SR.MetadataInvalidTokenAfterValues, tokenType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataReferenceNotFound(string id)
        {
            ThrowKdlException(string.Format(SR.MetadataReferenceNotFound, id));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataValueWasNotString(KdlTokenType tokenType)
        {
            ThrowKdlException(string.Format(SR.MetadataValueWasNotString, tokenType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataValueWasNotString(KdlValueKind valueKind)
        {
            ThrowKdlException(string.Format(SR.MetadataValueWasNotString, valueKind));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataReferenceObjectCannotContainOtherProperties(ReadOnlySpan<byte> propertyName, scoped ref ReadStack state)
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException_MetadataReferenceObjectCannotContainOtherProperties();
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataUnexpectedProperty(ReadOnlySpan<byte> propertyName, scoped ref ReadStack state)
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException(string.Format(SR.MetadataUnexpectedProperty));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_UnmappedKdlProperty(Type type, string unmappedPropertyName)
        {
            throw new KdlException(string.Format(SR.UnmappedKdlProperty, unmappedPropertyName, type));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataReferenceObjectCannotContainOtherProperties()
        {
            ThrowKdlException(SR.MetadataReferenceCannotContainOtherProperties);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataIdCannotBeCombinedWithRef(ReadOnlySpan<byte> propertyName, scoped ref ReadStack state)
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException(SR.MetadataIdCannotBeCombinedWithRef);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataStandaloneValuesProperty(scoped ref ReadStack state, ReadOnlySpan<byte> propertyName)
        {
            state.Current.KdlPropertyName = propertyName.ToArray();
            ThrowKdlException(SR.MetadataStandaloneValuesProperty);
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataInvalidPropertyWithLeadingDollarSign(ReadOnlySpan<byte> propertyName, scoped ref ReadStack state, in KdlReader reader)
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
            ThrowKdlException(string.Format(SR.MetadataDuplicateIdFound, id));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_DuplicateMetadataProperty(ReadOnlySpan<byte> utf8PropertyName)
        {
            ThrowKdlException(string.Format(SR.DuplicateMetadataProperty, KdlHelpers.Utf8GetString(utf8PropertyName)));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataInvalidReferenceToValueType(Type propertyType)
        {
            ThrowKdlException(string.Format(SR.MetadataInvalidReferenceToValueType, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataInvalidPropertyInArrayMetadata(scoped ref ReadStack state, Type propertyType, in KdlReader reader)
        {
            state.Current.KdlPropertyName = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray();
            string propertyNameAsString = reader.GetString()!;

            ThrowKdlException(string.Format(SR.MetadataPreservedArrayFailed,
                string.Format(SR.MetadataInvalidPropertyInArrayMetadata, propertyNameAsString),
                string.Format(SR.DeserializeUnableToConvertValue, propertyType)));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataPreservedArrayValuesNotFound(scoped ref ReadStack state, Type propertyType)
        {
            // Missing $values, KDL path should point to the property's object.
            state.Current.KdlPropertyName = null;

            ThrowKdlException(string.Format(SR.MetadataPreservedArrayFailed,
                SR.MetadataStandaloneValuesProperty,
                string.Format(SR.DeserializeUnableToConvertValue, propertyType)));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_MetadataCannotParsePreservedObjectIntoImmutable(Type propertyType)
        {
            ThrowKdlException(string.Format(SR.MetadataCannotParsePreservedObjectToImmutable, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MetadataReferenceOfTypeCannotBeAssignedToType(string referenceId, Type currentType, Type typeToConvert)
        {
            throw new InvalidOperationException(string.Format(SR.MetadataReferenceOfTypeCannotBeAssignedToType, referenceId, currentType, typeToConvert));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlPropertyInfoIsBoundToDifferentKdlTypeInfo(KdlPropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo.DeclaringTypeInfo != null, "We should not throw this exception when ParentTypeInfo is null");
            throw new InvalidOperationException(string.Format(SR.KdlPropertyInfoBoundToDifferentParent, propertyInfo.Name, propertyInfo.DeclaringTypeInfo.Type.FullName));
        }

        [DoesNotReturn]
        internal static void ThrowUnexpectedMetadataException(
            ReadOnlySpan<byte> propertyName,
            ref KdlReader reader,
            scoped ref ReadStack state)
        {

            MetadataPropertyName name = KdlSerializer.GetMetadataPropertyName(propertyName, state.Current.BaseKdlTypeInfo.PolymorphicTypeResolver);
            if (name != 0)
            {
                ThrowKdlException_MetadataUnexpectedProperty(propertyName, ref state);
            }
            else
            {
                ThrowKdlException_MetadataInvalidPropertyWithLeadingDollarSign(propertyName, ref state, reader);
            }
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_NoMetadataForType(Type type, IKdlTypeInfoResolver? resolver)
        {
            throw new NotSupportedException(string.Format(SR.NoMetadataForType, type, resolver?.ToString() ?? "<null>"));
        }

        public static NotSupportedException GetNotSupportedException_AmbiguousMetadataForType(Type type, Type match1, Type match2)
        {
            return new NotSupportedException(string.Format(SR.AmbiguousMetadataForType, type, match1, match2));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ConstructorContainsNullParameterNames(Type declaringType)
        {
            throw new NotSupportedException(string.Format(SR.ConstructorContainsNullParameterNames, declaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NoMetadataForType(Type type, IKdlTypeInfoResolver? resolver)
        {
            throw new InvalidOperationException(string.Format(SR.NoMetadataForType, type, resolver?.ToString() ?? "<null>"));
        }

        public static Exception GetInvalidOperationException_NoMetadataForTypeProperties(IKdlTypeInfoResolver? resolver, Type type)
        {
            return new InvalidOperationException(string.Format(SR.NoMetadataForTypeProperties, resolver?.ToString() ?? "<null>", type));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NoMetadataForTypeProperties(IKdlTypeInfoResolver? resolver, Type type)
        {
            throw GetInvalidOperationException_NoMetadataForTypeProperties(resolver, type);
        }

        [DoesNotReturn]
        public static void ThrowMissingMemberException_MissingFSharpCoreMember(string missingFsharpCoreMember)
        {
            throw new MissingMemberException(string.Format(SR.MissingFSharpCoreMember, missingFsharpCoreMember));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_BaseConverterDoesNotSupportMetadata(Type derivedType)
        {
            throw new NotSupportedException(string.Format(SR.Polymorphism_DerivedConverterDoesNotSupportMetadata, derivedType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DerivedConverterDoesNotSupportMetadata(Type derivedType)
        {
            throw new NotSupportedException(string.Format(SR.Polymorphism_DerivedConverterDoesNotSupportMetadata, derivedType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_RuntimeTypeNotSupported(Type baseType, Type runtimeType)
        {
            throw new NotSupportedException(string.Format(SR.Polymorphism_RuntimeTypeNotSupported, runtimeType, baseType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_RuntimeTypeDiamondAmbiguity(Type baseType, Type runtimeType, Type derivedType1, Type derivedType2)
        {
            throw new NotSupportedException(string.Format(SR.Polymorphism_RuntimeTypeDiamondAmbiguity, runtimeType, derivedType1, derivedType2, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeDoesNotSupportPolymorphism(Type baseType)
        {
            throw new InvalidOperationException(string.Format(SR.Polymorphism_TypeDoesNotSupportPolymorphism, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DerivedTypeNotSupported(Type baseType, Type derivedType)
        {
            throw new InvalidOperationException(string.Format(SR.Polymorphism_DerivedTypeIsNotSupported, derivedType, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DerivedTypeIsAlreadySpecified(Type baseType, Type derivedType)
        {
            throw new InvalidOperationException(string.Format(SR.Polymorphism_DerivedTypeIsAlreadySpecified, baseType, derivedType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeDicriminatorIdIsAlreadySpecified(Type baseType, object typeDiscriminator)
        {
            throw new InvalidOperationException(string.Format(SR.Polymorphism_TypeDicriminatorIdIsAlreadySpecified, baseType, typeDiscriminator));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_InvalidCustomTypeDiscriminatorPropertyName()
        {
            throw new InvalidOperationException(SR.Polymorphism_InvalidCustomTypeDiscriminatorPropertyName);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PropertyConflictsWithMetadataPropertyName(Type type, string propertyName)
        {
            throw new InvalidOperationException(string.Format(SR.Polymorphism_PropertyConflictsWithMetadataPropertyName, type, propertyName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PolymorphicTypeConfigurationDoesNotSpecifyDerivedTypes(Type baseType)
        {
            throw new InvalidOperationException(string.Format(SR.Polymorphism_ConfigurationDoesNotSpecifyDerivedTypes, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_UnsupportedEnumIdentifier(Type enumType, string? enumName)
        {
            throw new InvalidOperationException(string.Format(SR.UnsupportedEnumIdentifier, enumType.Name, enumName));
        }

        [DoesNotReturn]
        public static void ThrowKdlException_UnrecognizedTypeDiscriminator(object typeDiscriminator)
        {
            ThrowKdlException(string.Format(SR.Polymorphism_UnrecognizedTypeDiscriminator, typeDiscriminator));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_KdlPolymorphismOptionsAssociatedWithDifferentKdlTypeInfo(string parameterName)
        {
            throw new ArgumentException(SR.KdlPolymorphismOptionsAssociatedWithDifferentKdlTypeInfo, paramName: parameterName);
        }

        [DoesNotReturn]
        public static void ThrowOperationCanceledException_PipeWriteCanceled()
        {
            throw new OperationCanceledException(SR.PipeWriterCanceled);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PipeWriterDoesNotImplementUnflushedBytes(PipeWriter pipeWriter)
        {
            throw new InvalidOperationException(string.Format(SR.PipeWriter_DoesNotImplementUnflushedBytes, pipeWriter.GetType().Name));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_KdlSchemaExporterDoesNotSupportReferenceHandlerPreserve()
        {
            throw new NotSupportedException(SR.KdlSchemaExporter_ReferenceHandlerPreserve_NotSupported);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_KdlSchemaExporterDepthTooLarge()
        {
            throw new InvalidOperationException(SR.KdlSchemaExporter_DepthTooLarge);
        }
    }
}
