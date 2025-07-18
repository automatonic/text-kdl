using System.Collections;
using System.Collections.Concurrent;
using Automatonic.Text.Kdl.Serialization.Converters;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    public static partial class KdlMetadataServices
    {
        /// <summary>
        /// Creates serialization metadata for an array.
        /// </summary>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to use.</param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TElement[]> CreateArrayInfo<TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TElement[]> collectionInfo
        ) => CreateCore(options, collectionInfo, new ArrayConverter<TElement[], TElement>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to use.</param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateListInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : List<TElement> =>
            CreateCore(options, collectionInfo, new ListOfTConverter<TCollection, TElement>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TKey">The generic definition of the key type.</typeparam>
        /// <typeparam name="TValue">The generic definition of the value type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateDictionaryInfo<TCollection, TKey, TValue>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : Dictionary<TKey, TValue>
            where TKey : notnull =>
            CreateCore(
                options,
                collectionInfo,
                new DictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>()
            );

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
        /// <summary>
        /// Creates serialization metadata for <see cref="System.Collections.Immutable.ImmutableDictionary{TKey, TValue}"/> and
        /// types assignable to <see cref="System.Collections.Immutable.IImmutableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TKey">The generic definition of the key type.</typeparam>
        /// <typeparam name="TValue">The generic definition of the value type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <param name="createRangeFunc">A method to create an immutable dictionary instance.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
        public static KdlTypeInfo<TCollection> CreateImmutableDictionaryInfo<
            TCollection,
            TKey,
            TValue
        >(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo,
            Func<IEnumerable<KeyValuePair<TKey, TValue>>, TCollection> createRangeFunc
        )
            where TCollection : IReadOnlyDictionary<TKey, TValue>
            where TKey : notnull
        {
            if (createRangeFunc is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(createRangeFunc));
            }

            return CreateCore(
                options,
                collectionInfo,
                new ImmutableDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>(),
                createObjectWithArgs: createRangeFunc
            );
        }

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TKey">The generic definition of the key type.</typeparam>
        /// <typeparam name="TValue">The generic definition of the value type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIDictionaryInfo<TCollection, TKey, TValue>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IDictionary<TKey, TValue>
            where TKey : notnull =>
            CreateCore(
                options,
                collectionInfo,
                new IDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>()
            );

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TKey">The generic definition of the key type.</typeparam>
        /// <typeparam name="TValue">The generic definition of the value type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIReadOnlyDictionaryInfo<
            TCollection,
            TKey,
            TValue
        >(KdlSerializerOptions options, KdlCollectionInfoValues<TCollection> collectionInfo)
            where TCollection : IReadOnlyDictionary<TKey, TValue>
            where TKey : notnull =>
            CreateCore(
                options,
                collectionInfo,
                new IReadOnlyDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>()
            );

        /// <summary>
        /// Creates serialization metadata for non-dictionary immutable collection types.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <param name="createRangeFunc">A method to create an immutable dictionary instance.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateImmutableEnumerableInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo,
            Func<IEnumerable<TElement>, TCollection> createRangeFunc
        )
            where TCollection : IEnumerable<TElement>
        {
            if (createRangeFunc is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(createRangeFunc));
            }

            return CreateCore(
                options,
                collectionInfo,
                new ImmutableEnumerableOfTConverter<TCollection, TElement>(),
                createObjectWithArgs: createRangeFunc
            );
        }

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IList"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIListInfo<TCollection>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IList =>
            CreateCore(options, collectionInfo, new IListConverter<TCollection>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IList{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIListInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IList<TElement> =>
            CreateCore(options, collectionInfo, new IListOfTConverter<TCollection, TElement>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="ISet{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateISetInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : ISet<TElement> =>
            CreateCore(options, collectionInfo, new ISetOfTConverter<TCollection, TElement>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateICollectionInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : ICollection<TElement> =>
            CreateCore(
                options,
                collectionInfo,
                new ICollectionOfTConverter<TCollection, TElement>()
            );

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="Stack{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateStackInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : Stack<TElement> =>
            CreateCore(options, collectionInfo, new StackOfTConverter<TCollection, TElement>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateQueueInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : Queue<TElement> =>
            CreateCore(options, collectionInfo, new QueueOfTConverter<TCollection, TElement>());

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="ConcurrentStack{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateConcurrentStackInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : ConcurrentStack<TElement> =>
            CreateCore(
                options,
                collectionInfo,
                new ConcurrentStackOfTConverter<TCollection, TElement>()
            );

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateConcurrentQueueInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : ConcurrentQueue<TElement> =>
            CreateCore(
                options,
                collectionInfo,
                new ConcurrentQueueOfTConverter<TCollection, TElement>()
            );

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIEnumerableInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IEnumerable<TElement> =>
            CreateCore(
                options,
                collectionInfo,
                new IEnumerableOfTConverter<TCollection, TElement>()
            );

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIAsyncEnumerableInfo<TCollection, TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IAsyncEnumerable<TElement> =>
            CreateCore(
                options,
                collectionInfo,
                new IAsyncEnumerableOfTConverter<TCollection, TElement>()
            );

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IDictionary"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIDictionaryInfo<TCollection>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IDictionary =>
            CreateCore(options, collectionInfo, new IDictionaryConverter<TCollection>());

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
        /// <summary>
        /// Creates serialization metadata for <see cref="System.Collections.Stack"/> types.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <param name="addFunc">A method for adding elements to the collection when using the serializer's code-paths.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
        public static KdlTypeInfo<TCollection> CreateStackInfo<TCollection>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo,
            Action<TCollection, object?> addFunc
        )
            where TCollection : IEnumerable =>
            CreateStackOrQueueInfo(options, collectionInfo, addFunc);

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
        /// <summary>
        /// Creates serialization metadata for <see cref="System.Collections.Queue"/> types.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <param name="addFunc">A method for adding elements to the collection when using the serializer's code-paths.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
        public static KdlTypeInfo<TCollection> CreateQueueInfo<TCollection>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo,
            Action<TCollection, object?> addFunc
        )
            where TCollection : IEnumerable =>
            CreateStackOrQueueInfo(options, collectionInfo, addFunc);

        private static KdlTypeInfo<TCollection> CreateStackOrQueueInfo<TCollection>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo,
            Action<TCollection, object?> addFunc
        )
            where TCollection : IEnumerable
        {
            if (addFunc is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(addFunc));
            }

            return CreateCore(
                options,
                collectionInfo,
                new StackOrQueueConverter<TCollection>(),
                createObjectWithArgs: null,
                addFunc: addFunc
            );
        }

        /// <summary>
        /// Creates serialization metadata for types assignable to <see cref="IList"/>.
        /// </summary>
        /// <typeparam name="TCollection">The generic definition of the type.</typeparam>
        /// <param name="options"></param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<TCollection> CreateIEnumerableInfo<TCollection>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<TCollection> collectionInfo
        )
            where TCollection : IEnumerable =>
            CreateCore(options, collectionInfo, new IEnumerableConverter<TCollection>());

        /// <summary>
        /// Creates serialization metadata for <see cref="Memory{T}"/>.
        /// </summary>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to use.</param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<Memory<TElement>> CreateMemoryInfo<TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<Memory<TElement>> collectionInfo
        ) => CreateCore(options, collectionInfo, new MemoryConverter<TElement>());

        /// <summary>
        /// Creates serialization metadata for <see cref="ReadOnlyMemory{T}"/>.
        /// </summary>
        /// <typeparam name="TElement">The generic definition of the element type.</typeparam>
        /// <param name="options">The <see cref="KdlSerializerOptions"/> to use.</param>
        /// <param name="collectionInfo">Provides serialization metadata about the collection type.</param>
        /// <returns>Serialization metadata for the given type.</returns>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public static KdlTypeInfo<ReadOnlyMemory<TElement>> CreateReadOnlyMemoryInfo<TElement>(
            KdlSerializerOptions options,
            KdlCollectionInfoValues<ReadOnlyMemory<TElement>> collectionInfo
        ) => CreateCore(options, collectionInfo, new ReadOnlyMemoryConverter<TElement>());
    }
}
