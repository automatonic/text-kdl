using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Implementation of <cref>KdlObjectConverter{T}</cref> that supports the deserialization
    /// of KDL objects using parameterized constructors.
    /// </summary>
    internal sealed class SmallObjectWithParameterizedConstructorConverter<T, TArg0, TArg1, TArg2, TArg3> : ObjectWithParameterizedConstructorConverter<T> where T : notnull
    {
        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var createObject = (KdlTypeInfo.ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>)
                frame.KdlTypeInfo.CreateObjectWithArgs!;
            var arguments = (Arguments<TArg0, TArg1, TArg2, TArg3>)frame.CtorArgumentState!.Arguments;
            return createObject!(arguments.Arg0, arguments.Arg1, arguments.Arg2, arguments.Arg3);
        }

        protected override bool ReadAndCacheConstructorArgument(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlParameterInfo jsonParameterInfo)
        {
            Debug.Assert(state.Current.CtorArgumentState!.Arguments != null);
            var arguments = (Arguments<TArg0, TArg1, TArg2, TArg3>)state.Current.CtorArgumentState.Arguments;

            bool success;

            switch (jsonParameterInfo.Position)
            {
                case 0:
                    success = TryRead(ref state, ref reader, jsonParameterInfo, out arguments.Arg0);
                    break;
                case 1:
                    success = TryRead(ref state, ref reader, jsonParameterInfo, out arguments.Arg1);
                    break;
                case 2:
                    success = TryRead(ref state, ref reader, jsonParameterInfo, out arguments.Arg2);
                    break;
                case 3:
                    success = TryRead(ref state, ref reader, jsonParameterInfo, out arguments.Arg3);
                    break;
                default:
                    Debug.Fail("More than 4 params: we should be in override for LargeObjectWithParameterizedConstructorConverter.");
                    throw new InvalidOperationException();
            }

            return success;
        }

        private static bool TryRead<TArg>(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlParameterInfo jsonParameterInfo,
            out TArg? arg)
        {
            Debug.Assert(jsonParameterInfo.ShouldDeserialize);

            var info = (KdlParameterInfo<TArg>)jsonParameterInfo;

            bool success = info.EffectiveConverter.TryRead(ref reader, info.ParameterType, info.Options, ref state, out TArg? value, out _);

            if (success)
            {
                if (value is null)
                {
                    if (info.IgnoreNullTokensOnRead)
                    {
                        // Use default value specified on parameter, if any.
                        value = info.EffectiveDefaultValue;
                    }
                    else if (!info.IsNullable && info.Options.RespectNullableAnnotations)
                    {
                        ThrowHelper.ThrowKdlException_ConstructorParameterDisallowNull(info.Name, state.Current.KdlTypeInfo.Type);
                    }
                }
            }

            arg = value;
            return success;
        }

        protected override void InitializeConstructorArgumentCaches(ref ReadStack state, KdlSerializerOptions options)
        {
            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;

            Debug.Assert(typeInfo.CreateObjectWithArgs != null);

            var arguments = new Arguments<TArg0, TArg1, TArg2, TArg3>();

            foreach (KdlParameterInfo parameterInfo in typeInfo.ParameterCache)
            {
                switch (parameterInfo.Position)
                {
                    case 0:
                        arguments.Arg0 = ((KdlParameterInfo<TArg0>)parameterInfo).EffectiveDefaultValue;
                        break;
                    case 1:
                        arguments.Arg1 = ((KdlParameterInfo<TArg1>)parameterInfo).EffectiveDefaultValue;
                        break;
                    case 2:
                        arguments.Arg2 = ((KdlParameterInfo<TArg2>)parameterInfo).EffectiveDefaultValue;
                        break;
                    case 3:
                        arguments.Arg3 = ((KdlParameterInfo<TArg3>)parameterInfo).EffectiveDefaultValue;
                        break;
                    default:
                        Debug.Fail("More than 4 params: we should be in override for LargeObjectWithParameterizedConstructorConverter.");
                        break;
                }
            }

            state.Current.CtorArgumentState!.Arguments = arguments;
        }

        [RequiresUnreferencedCode(KdlSerializer.SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
        internal override void ConfigureKdlTypeInfoUsingReflection(KdlTypeInfo jsonTypeInfo, KdlSerializerOptions options)
        {
            jsonTypeInfo.CreateObjectWithArgs = DefaultKdlTypeInfoResolver.MemberAccessor.CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(ConstructorInfo!);
        }
    }
}
