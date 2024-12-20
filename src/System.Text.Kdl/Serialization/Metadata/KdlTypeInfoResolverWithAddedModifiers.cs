using System.Diagnostics;

namespace System.Text.Kdl.Serialization.Metadata
{
    internal sealed class KdlTypeInfoResolverWithAddedModifiers : IKdlTypeInfoResolver
    {
        private readonly IKdlTypeInfoResolver _source;
        private readonly Action<KdlTypeInfo>[] _modifiers;

        public KdlTypeInfoResolverWithAddedModifiers(IKdlTypeInfoResolver source, Action<KdlTypeInfo>[] modifiers)
        {
            Debug.Assert(modifiers.Length > 0);
            _source = source;
            _modifiers = modifiers;
        }

        public KdlTypeInfoResolverWithAddedModifiers WithAddedModifier(Action<KdlTypeInfo> modifier)
        {
            var newModifiers = new Action<KdlTypeInfo>[_modifiers.Length + 1];
            _modifiers.CopyTo(newModifiers, 0);
            newModifiers[_modifiers.Length] = modifier;

            return new KdlTypeInfoResolverWithAddedModifiers(_source, newModifiers);
        }

        public KdlTypeInfo? GetTypeInfo(Type type, KdlSerializerOptions options)
        {
            KdlTypeInfo? typeInfo = _source.GetTypeInfo(type, options);

            if (typeInfo != null)
            {
                foreach (Action<KdlTypeInfo> modifier in _modifiers)
                {
                    modifier(typeInfo);
                }
            }

            return typeInfo;
        }
    }
}
