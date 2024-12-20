namespace System.Text.Kdl.Serialization.Metadata
{
    internal class KdlTypeInfoResolverChain : ConfigurationList<IKdlTypeInfoResolver>, IKdlTypeInfoResolver, IBuiltInKdlTypeInfoResolver
    {
        public KdlTypeInfoResolverChain() : base(null) { }
        public override bool IsReadOnly => true;
        protected override void OnCollectionModifying()
            => ThrowHelper.ThrowInvalidOperationException_TypeInfoResolverChainImmutable();

        public KdlTypeInfo? GetTypeInfo(Type type, KdlSerializerOptions options)
        {
            foreach (IKdlTypeInfoResolver resolver in _list)
            {
                KdlTypeInfo? typeInfo = resolver.GetTypeInfo(type, options);
                if (typeInfo != null)
                {
                    return typeInfo;
                }
            }

            return null;
        }

        internal void AddFlattened(IKdlTypeInfoResolver? resolver)
        {
            switch (resolver)
            {
                case null or EmptyKdlTypeInfoResolver:
                    break;

                case KdlTypeInfoResolverChain otherChain:
                    _list.AddRange(otherChain);
                    break;

                default:
                    _list.Add(resolver);
                    break;
            }
        }

        bool IBuiltInKdlTypeInfoResolver.IsCompatibleWithOptions(KdlSerializerOptions options)
        {
            foreach (IKdlTypeInfoResolver component in _list)
            {
                if (!component.IsCompatibleWithOptions(options))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            foreach (IKdlTypeInfoResolver resolver in _list)
            {
                sb.Append(resolver);
                sb.Append(", ");
            }

            if (_list.Count > 0)
                sb.Length -= 2;

            sb.Append(']');
            return sb.ToString();
        }
    }
}
