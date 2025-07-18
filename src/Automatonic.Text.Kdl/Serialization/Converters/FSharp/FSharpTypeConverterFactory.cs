using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;
using FSharpKind = Automatonic.Text.Kdl.Serialization.Metadata.FSharpCoreReflectionProxy.FSharpKind;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    [RequiresDynamicCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
    [method: RequiresUnreferencedCode(FSharpCoreReflectionProxy.FSharpCoreUnreferencedCodeMessage)]
    internal sealed class FSharpTypeConverterFactory() : KdlConverterFactory
    {
        private ObjectConverterFactory? _recordConverterFactory;

        [UnconditionalSuppressMessage(
            "ReflectionAnalysis",
            "IL2026:RequiresUnreferencedCode",
            Justification = "The ctor is marked RequiresUnreferencedCode."
        )]
        public override bool CanConvert(Type typeToConvert) =>
            FSharpCoreReflectionProxy.IsFSharpType(typeToConvert)
            && FSharpCoreReflectionProxy.Instance.DetectFSharpKind(typeToConvert)
                is not FSharpKind.Unrecognized;

        [UnconditionalSuppressMessage(
            "ReflectionAnalysis",
            "IL2026:RequiresUnreferencedCode",
            Justification = "The ctor is marked RequiresUnreferencedCode."
        )]
        [UnconditionalSuppressMessage(
            "ReflectionAnalysis",
            "IL2055:MakeGenericType",
            Justification = "The ctor is marked RequiresUnreferencedCode."
        )]
        public override KdlConverter? CreateConverter(
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(CanConvert(typeToConvert));

            Type elementType;
            Type converterFactoryType;
            object?[]? constructorArguments = null;

            switch (FSharpCoreReflectionProxy.Instance.DetectFSharpKind(typeToConvert))
            {
                case FSharpKind.Option:
                    elementType = typeToConvert.GetGenericArguments()[0];
                    converterFactoryType = typeof(FSharpOptionConverter<,>).MakeGenericType(
                        typeToConvert,
                        elementType
                    );
                    constructorArguments = [options.GetConverterInternal(elementType)];
                    break;
                case FSharpKind.ValueOption:
                    elementType = typeToConvert.GetGenericArguments()[0];
                    converterFactoryType = typeof(FSharpValueOptionConverter<,>).MakeGenericType(
                        typeToConvert,
                        elementType
                    );
                    constructorArguments = [options.GetConverterInternal(elementType)];
                    break;
                case FSharpKind.List:
                    elementType = typeToConvert.GetGenericArguments()[0];
                    converterFactoryType = typeof(FSharpListConverter<,>).MakeGenericType(
                        typeToConvert,
                        elementType
                    );
                    break;
                case FSharpKind.Set:
                    elementType = typeToConvert.GetGenericArguments()[0];
                    converterFactoryType = typeof(FSharpSetConverter<,>).MakeGenericType(
                        typeToConvert,
                        elementType
                    );
                    break;
                case FSharpKind.Map:
                    Type[] genericArgs = typeToConvert.GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    converterFactoryType = typeof(FSharpMapConverter<,,>).MakeGenericType(
                        typeToConvert,
                        keyType,
                        valueType
                    );
                    break;
                case FSharpKind.Record:
                    // Use a modified object converter factory that picks the right constructor for struct record deserialization.
                    ObjectConverterFactory objectFactory = _recordConverterFactory ??=
                        new ObjectConverterFactory(
                            useDefaultConstructorInUnannotatedStructs: false
                        );
                    Debug.Assert(objectFactory.CanConvert(typeToConvert));
                    return objectFactory.CreateConverter(typeToConvert, options);
                case FSharpKind.Union:
                    return UnsupportedTypeConverterFactory.CreateUnsupportedConverterForType(
                        typeToConvert,
                        SR.FSharpDiscriminatedUnionsNotSupported
                    );
                default:
                    Debug.Fail("Unrecognized F# type.");
                    throw new Exception();
            }

            return (KdlConverter)
                Activator.CreateInstance(converterFactoryType, constructorArguments)!;
        }
    }
}
