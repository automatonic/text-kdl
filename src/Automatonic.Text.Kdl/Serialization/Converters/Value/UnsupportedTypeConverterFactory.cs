﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class UnsupportedTypeConverterFactory : KdlConverterFactory
    {
        public override bool CanConvert(Type type)
        {
            // If a type is added, also add to the SourceGeneration project.

            return
                // There's no safe way to construct a Type/MemberInfo from untrusted user input.
                typeof(MemberInfo).IsAssignableFrom(type)
                ||
                // (De)serialization of SerializationInfo is already disallowed due to Type being disallowed
                // (the two ctors on SerializationInfo take a Type, and a Type member is present when serializing).
                // Explicitly disallowing this type provides a clear exception when ctors with
                // .ctor(SerializationInfo, StreamingContext) signatures are attempted to be used for deserialization.
                // Invoking such ctors is not safe when used with untrusted user input.
                type == typeof(SerializationInfo)
                || type == typeof(IntPtr)
                || type == typeof(UIntPtr)
                ||
                // Exclude delegates.
                typeof(Delegate).IsAssignableFrom(type);
        }

        public override KdlConverter CreateConverter(Type type, KdlSerializerOptions options)
        {
            Debug.Assert(CanConvert(type));
            return CreateUnsupportedConverterForType(type);
        }

        internal static KdlConverter CreateUnsupportedConverterForType(
            Type type,
            string? errorMessage = null
        )
        {
            KdlConverter converter = (KdlConverter)
                Activator.CreateInstance(
                    typeof(UnsupportedTypeConverter<>).MakeGenericType(type),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: [errorMessage],
                    culture: null
                )!;

            return converter;
        }
    }
}
