using System.Buffers;
using System.Diagnostics;
using Automatonic.Text.Kdl.Serialization.Metadata;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    /// <summary>
    /// Implementation of <cref>KdlObjectConverter{T}</cref> that supports the deserialization
    /// of KDL objects using parameterized constructors.
    /// </summary>
    internal class LargeObjectWithParameterizedConstructorConverter<T>
        : ObjectWithParameterizedConstructorConverter<T>
        where T : notnull
    {
        protected sealed override bool ReadAndCacheConstructorArgument(
            scoped ref ReadStack state,
            ref KdlReader reader,
            KdlParameterInfo kdlParameterInfo
        )
        {
            Debug.Assert(kdlParameterInfo.ShouldDeserialize);

            bool success = kdlParameterInfo.EffectiveConverter.TryReadAsObject(
                ref reader,
                kdlParameterInfo.ParameterType,
                kdlParameterInfo.Options,
                ref state,
                out object? arg
            );

            if (success && !(arg == null && kdlParameterInfo.IgnoreNullTokensOnRead))
            {
                if (
                    arg == null
                    && !kdlParameterInfo.IsNullable
                    && kdlParameterInfo.Options.RespectNullableAnnotations
                )
                {
                    ThrowHelper.ThrowKdlException_ConstructorParameterDisallowNull(
                        kdlParameterInfo.Name,
                        state.Current.KdlTypeInfo.Type
                    );
                }

                ((object[])state.Current.CtorArgumentState!.Arguments)[kdlParameterInfo.Position] =
                    arg!;
            }

            return success;
        }

        protected sealed override object CreateObject(ref ReadStackFrame frame)
        {
            Debug.Assert(frame.CtorArgumentState != null);
            Debug.Assert(frame.KdlTypeInfo.CreateObjectWithArgs != null);

            object[] arguments = (object[])frame.CtorArgumentState.Arguments;
            frame.CtorArgumentState.Arguments = null!;

            Func<object[], T> createObject =
                (Func<object[], T>)frame.KdlTypeInfo.CreateObjectWithArgs;

            object obj = createObject(arguments);

            ArrayPool<object>.Shared.Return(arguments, clearArray: true);
            return obj;
        }

        protected sealed override void InitializeConstructorArgumentCaches(
            ref ReadStack state,
            KdlSerializerOptions options
        )
        {
            KdlTypeInfo typeInfo = state.Current.KdlTypeInfo;

            object?[] arguments = ArrayPool<object>.Shared.Rent(typeInfo.ParameterCache.Length);
            foreach (KdlParameterInfo parameterInfo in typeInfo.ParameterCache)
            {
                arguments[parameterInfo.Position] = parameterInfo.EffectiveDefaultValue;
            }

            state.Current.CtorArgumentState!.Arguments = arguments;
        }
    }
}
