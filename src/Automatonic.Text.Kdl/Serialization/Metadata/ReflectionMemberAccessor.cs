using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    [RequiresDynamicCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(KdlSerializer.SerializationRequiresDynamicCodeMessage)]
    internal sealed class ReflectionMemberAccessor : MemberAccessor
    {
        public ReflectionMemberAccessor() { }

        public override Func<object>? CreateParameterlessConstructor(
            Type type,
            ConstructorInfo? ctorInfo
        )
        {
            Debug.Assert(type != null);
            Debug.Assert(ctorInfo is null || ctorInfo.GetParameters().Length == 0);

            if (type.IsAbstract)
            {
                return null;
            }

            if (ctorInfo is null)
            {
                return type.IsValueType
                    ? () => Activator.CreateInstance(type, nonPublic: false)!
                    : null;
            }

            return () => ctorInfo.Invoke(null);
        }

        public override Func<object[], T> CreateParameterizedConstructor<T>(
            ConstructorInfo constructor
        )
        {
            Type type = typeof(T);

            Debug.Assert(!type.IsAbstract);
            Debug.Assert(
                constructor.DeclaringType == type && constructor.IsPublic && !constructor.IsStatic
            );

            int parameterCount = constructor.GetParameters().Length;

            return (arguments) =>
            {
                // The input array was rented from the shared ArrayPool, so its size is likely to be larger than the param count.
                // The emit equivalent of this method does not (need to) allocate here + transfer the objects.
                object[] argsToPass = new object[parameterCount];

                for (int i = 0; i < parameterCount; i++)
                {
                    argsToPass[i] = arguments[i];
                }

                try
                {
                    return (T)constructor.Invoke(argsToPass);
                }
                catch (TargetInvocationException e)
                {
                    // Plumb ArgumentException through for tuples with more than 7 generic parameters, e.g.
                    // System.ArgumentException : The last element of an eight element tuple must be a Tuple.
                    // This doesn't apply to the method below as it supports a max of 4 constructor params.
                    throw e.InnerException ?? e;
                }
            };
        }

        public override KdlTypeInfo.ParameterizedConstructorDelegate<
            T,
            TArg0,
            TArg1,
            TArg2,
            TArg3
        >? CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(
            ConstructorInfo constructor
        )
        {
            Type type = typeof(T);

            Debug.Assert(!type.IsAbstract);
            Debug.Assert(
                constructor.DeclaringType == type && constructor.IsPublic && !constructor.IsStatic
            );

            int parameterCount = constructor.GetParameters().Length;

            Debug.Assert(parameterCount <= KdlConstants.UnboxedParameterCountThreshold);

            return (arg0, arg1, arg2, arg3) =>
            {
                object[] arguments = new object[parameterCount];

                for (int i = 0; i < parameterCount; i++)
                {
                    switch (i)
                    {
                        case 0:
                            arguments[0] = arg0!;
                            break;
                        case 1:
                            arguments[1] = arg1!;
                            break;
                        case 2:
                            arguments[2] = arg2!;
                            break;
                        case 3:
                            arguments[3] = arg3!;
                            break;
                        default:
                            Debug.Fail("We shouldn't be here if there are more than 4 parameters.");
                            throw new InvalidOperationException();
                    }
                }

                return (T)constructor.Invoke(arguments);
            };
        }

        public override Action<TCollection, object?> CreateAddMethodDelegate<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TCollection
        >()
        {
            Type collectionType = typeof(TCollection);
            Type elementType = KdlTypeInfo.ObjectType;

            // We verified this won't be null when we created the converter for the collection type.
            MethodInfo addMethod = (
                collectionType.GetMethod("Push") ?? collectionType.GetMethod("Enqueue")
            )!;

            return delegate(TCollection collection, object? element)
            {
                addMethod.Invoke(collection, [element!]);
            };
        }

        public override Func<
            IEnumerable<TElement>,
            TCollection
        > CreateImmutableEnumerableCreateRangeDelegate<TCollection, TElement>()
        {
            MethodInfo createRange = typeof(TCollection).GetImmutableEnumerableCreateRangeMethod(
                typeof(TElement)
            );
            return (Func<IEnumerable<TElement>, TCollection>)
                createRange.CreateDelegate(typeof(Func<IEnumerable<TElement>, TCollection>));
        }

        public override Func<
            IEnumerable<KeyValuePair<TKey, TValue>>,
            TCollection
        > CreateImmutableDictionaryCreateRangeDelegate<TCollection, TKey, TValue>()
        {
            MethodInfo createRange = typeof(TCollection).GetImmutableDictionaryCreateRangeMethod(
                typeof(TKey),
                typeof(TValue)
            );
            return (Func<IEnumerable<KeyValuePair<TKey, TValue>>, TCollection>)
                createRange.CreateDelegate(
                    typeof(Func<IEnumerable<KeyValuePair<TKey, TValue>>, TCollection>)
                );
        }

        public override Func<object, TProperty> CreatePropertyGetter<TProperty>(
            PropertyInfo propertyInfo
        )
        {
            MethodInfo getMethodInfo = propertyInfo.GetMethod!;

            return delegate(object obj)
            {
                return (TProperty)getMethodInfo.Invoke(obj, null)!;
            };
        }

        public override Action<object, TProperty> CreatePropertySetter<TProperty>(
            PropertyInfo propertyInfo
        )
        {
            MethodInfo setMethodInfo = propertyInfo.SetMethod!;

            return delegate(object obj, TProperty value)
            {
                setMethodInfo.Invoke(obj, [value!]);
            };
        }

        public override Func<object, TProperty> CreateFieldGetter<TProperty>(FieldInfo fieldInfo) =>
            delegate(object obj)
            {
                return (TProperty)fieldInfo.GetValue(obj)!;
            };

        public override Action<object, TProperty> CreateFieldSetter<TProperty>(
            FieldInfo fieldInfo
        ) =>
            delegate(object obj, TProperty value)
            {
                fieldInfo.SetValue(obj, value);
            };
    }
}
