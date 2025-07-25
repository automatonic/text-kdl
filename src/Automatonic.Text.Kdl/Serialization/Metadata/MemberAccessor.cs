using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    internal abstract class MemberAccessor
    {
        public abstract Func<object>? CreateParameterlessConstructor(
            Type type,
            ConstructorInfo? constructorInfo
        );

        public abstract Func<object[], T> CreateParameterizedConstructor<T>(
            ConstructorInfo constructor
        );

        public abstract KdlTypeInfo.ParameterizedConstructorDelegate<
            T,
            TArg0,
            TArg1,
            TArg2,
            TArg3
        >? CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(
            ConstructorInfo constructor
        );

        public abstract Action<TCollection, object?> CreateAddMethodDelegate<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TCollection
        >();

        public abstract Func<
            IEnumerable<TElement>,
            TCollection
        > CreateImmutableEnumerableCreateRangeDelegate<TCollection, TElement>();

        public abstract Func<
            IEnumerable<KeyValuePair<TKey, TValue>>,
            TCollection
        > CreateImmutableDictionaryCreateRangeDelegate<TCollection, TKey, TValue>();

        public abstract Func<object, TProperty> CreatePropertyGetter<TProperty>(
            PropertyInfo propertyInfo
        );

        public abstract Action<object, TProperty> CreatePropertySetter<TProperty>(
            PropertyInfo propertyInfo
        );

        public abstract Func<object, TProperty> CreateFieldGetter<TProperty>(FieldInfo fieldInfo);

        public abstract Action<object, TProperty> CreateFieldSetter<TProperty>(FieldInfo fieldInfo);

        public virtual void Clear() { }
    }
}
