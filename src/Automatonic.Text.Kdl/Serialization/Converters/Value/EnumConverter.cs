using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using Automatonic.Text.Kdl.Graph;
using Automatonic.Text.Kdl.Schema;

namespace Automatonic.Text.Kdl.Serialization.Converters
{
    internal sealed class EnumConverter<T> : KdlPrimitiveConverter<T> // Do not rename FQN (legacy schema generation)
        where T : struct, Enum
    {
        private static readonly TypeCode s_enumTypeCode = Type.GetTypeCode(typeof(T));

        // Odd type codes are conveniently signed types (for enum backing types).
        private static readonly bool s_isSignedEnum = ((int)s_enumTypeCode % 2) == 1;
        private static readonly bool s_isFlagsEnum = typeof(T).IsDefined(
            typeof(FlagsAttribute),
            inherit: false
        );

        private readonly EnumConverterOptions _converterOptions; // Do not rename (legacy schema generation)
        private readonly KdlNamingPolicy? _namingPolicy; // Do not rename (legacy schema generation)

        /// <summary>
        /// Stores metadata for the individual fields declared on the enum.
        /// </summary>
        private readonly EnumFieldInfo[] _enumFieldInfo;

        /// <summary>
        /// Defines a case-insensitive index of enum field names to their metadata.
        /// In case of casing conflicts, extra fields are appended to a list in the value.
        /// This is the main dictionary that is queried by the enum parser implementation.
        /// </summary>
        private readonly Dictionary<string, EnumFieldInfo> _enumFieldInfoIndex;

        /// <summary>
        /// Holds a cache from enum value to formatted UTF-8 text including flag combinations.
        /// <see cref="ulong"/> is as the key used rather than <typeparamref name="T"/> given measurements that
        /// show private memory savings when a single type is used https://github.com/dotnet/runtime/pull/36726#discussion_r428868336.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, KdlEncodedText> _nameCacheForWriting;

        /// <summary>
        /// Holds a mapping from input text to enum values including flag combinations and alternative casings.
        /// </summary>
        private readonly ConcurrentDictionary<string, ulong> _nameCacheForReading;

        // This is used to prevent flooding the cache due to exponential bitwise combinations of flags.
        // Since multiple threads can add to the cache, a few more values might be added.
        private const int NameCacheSizeSoftLimit = 64;

        public EnumConverter(
            EnumConverterOptions converterOptions,
            KdlNamingPolicy? namingPolicy,
            KdlSerializerOptions options
        )
        {
            Debug.Assert(EnumConverterFactory.Helpers.IsSupportedTypeCode(s_enumTypeCode));

            _converterOptions = converterOptions;
            _namingPolicy = namingPolicy;
            _enumFieldInfo = ResolveEnumFields(namingPolicy);
            _enumFieldInfoIndex = new(StringComparer.OrdinalIgnoreCase);

            _nameCacheForWriting = new();
            _nameCacheForReading = new(StringComparer.Ordinal);

            JavaScriptEncoder? encoder = options.Encoder;
            foreach (EnumFieldInfo fieldInfo in _enumFieldInfo)
            {
                AddToEnumFieldIndex(fieldInfo);

                KdlEncodedText encodedName = KdlEncodedText.Encode(fieldInfo.KdlName, encoder);
                _nameCacheForWriting.TryAdd(fieldInfo.Key, encodedName);
                _nameCacheForReading.TryAdd(fieldInfo.KdlName, fieldInfo.Key);
            }

            if (namingPolicy != null)
            {
                // Additionally populate the field index with the default names of fields that used a naming policy.
                // This is done to preserve backward compat: default names should still be recognized by the parser.
                foreach (EnumFieldInfo fieldInfo in _enumFieldInfo)
                {
                    if (fieldInfo.Kind is EnumFieldNameKind.NamingPolicy)
                    {
                        AddToEnumFieldIndex(
                            new EnumFieldInfo(
                                fieldInfo.Key,
                                EnumFieldNameKind.Default,
                                fieldInfo.OriginalName,
                                fieldInfo.OriginalName
                            )
                        );
                    }
                }
            }

            void AddToEnumFieldIndex(EnumFieldInfo fieldInfo)
            {
                if (!_enumFieldInfoIndex.TryAdd(fieldInfo.KdlName, fieldInfo))
                {
                    // We have a casing conflict, append field to the existing entry.
                    EnumFieldInfo existingFieldInfo = _enumFieldInfoIndex[fieldInfo.KdlName];
                    existingFieldInfo.AppendConflictingField(fieldInfo);
                }
            }
        }

        public override T Read(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            switch (reader.TokenType)
            {
                case KdlTokenType.String
                    when (_converterOptions & EnumConverterOptions.AllowStrings) != 0:
                    if (TryParseEnumFromString(ref reader, out T result))
                    {
                        return result;
                    }
                    break;

                case KdlTokenType.Number
                    when (_converterOptions & EnumConverterOptions.AllowNumbers) != 0:
                    switch (s_enumTypeCode)
                    {
                        case TypeCode.Int32 when reader.TryGetInt32(out int int32):
                            return Unsafe.As<int, T>(ref int32);
                        case TypeCode.UInt32 when reader.TryGetUInt32(out uint uint32):
                            return Unsafe.As<uint, T>(ref uint32);
                        case TypeCode.Int64 when reader.TryGetInt64(out long int64):
                            return Unsafe.As<long, T>(ref int64);
                        case TypeCode.UInt64 when reader.TryGetUInt64(out ulong uint64):
                            return Unsafe.As<ulong, T>(ref uint64);
                        case TypeCode.Byte when reader.TryGetByte(out byte ubyte8):
                            return Unsafe.As<byte, T>(ref ubyte8);
                        case TypeCode.SByte when reader.TryGetSByte(out sbyte byte8):
                            return Unsafe.As<sbyte, T>(ref byte8);
                        case TypeCode.Int16 when reader.TryGetInt16(out short int16):
                            return Unsafe.As<short, T>(ref int16);
                        case TypeCode.UInt16 when reader.TryGetUInt16(out ushort uint16):
                            return Unsafe.As<ushort, T>(ref uint16);
                    }
                    break;
            }

            ThrowHelper.ThrowKdlException();
            return default;
        }

        public override void Write(KdlWriter writer, T value, KdlSerializerOptions options)
        {
            EnumConverterOptions converterOptions = _converterOptions;
            if ((converterOptions & EnumConverterOptions.AllowStrings) != 0)
            {
                ulong key = ConvertToUInt64(value);

                if (_nameCacheForWriting.TryGetValue(key, out KdlEncodedText formatted))
                {
                    writer.WriteStringValue(formatted);
                    return;
                }

                if (IsDefinedValueOrCombinationOfValues(key))
                {
                    Debug.Assert(s_isFlagsEnum, "Should only be entered by flags enums.");
                    string stringValue = FormatEnumAsString(key, value, dictionaryKeyPolicy: null);
                    if (_nameCacheForWriting.Count < NameCacheSizeSoftLimit)
                    {
                        formatted = KdlEncodedText.Encode(stringValue, options.Encoder);
                        writer.WriteStringValue(formatted);
                        _nameCacheForWriting.TryAdd(key, formatted);
                    }
                    else
                    {
                        // We also do not create a KdlEncodedText instance here because passing the string
                        // directly to the writer is cheaper than creating one and not caching it for reuse.
                        writer.WriteStringValue(stringValue);
                    }

                    return;
                }
            }

            if ((converterOptions & EnumConverterOptions.AllowNumbers) == 0)
            {
                ThrowHelper.ThrowKdlException();
            }

            if (s_isSignedEnum)
            {
                writer.WriteNumberValue(ConvertToInt64(value));
            }
            else
            {
                writer.WriteNumberValue(ConvertToUInt64(value));
            }
        }

        internal override T ReadAsPropertyNameCore(
            ref KdlReader reader,
            Type typeToConvert,
            KdlSerializerOptions options
        )
        {
            // NB KdlSerializerOptions.DictionaryKeyPolicy is ignored on deserialization.
            // This is true for all converters that implement dictionary key serialization.

            if (!TryParseEnumFromString(ref reader, out T result))
            {
                ThrowHelper.ThrowKdlException();
            }

            return result;
        }

        internal override void WriteAsPropertyNameCore(
            KdlWriter writer,
            T value,
            KdlSerializerOptions options,
            bool isWritingExtensionDataProperty
        )
        {
            KdlNamingPolicy? dictionaryKeyPolicy =
                options.DictionaryKeyPolicy is { } dkp && dkp != _namingPolicy ? dkp : null;
            ulong key = ConvertToUInt64(value);

            if (
                dictionaryKeyPolicy is null
                && _nameCacheForWriting.TryGetValue(key, out KdlEncodedText formatted)
            )
            {
                writer.WritePropertyName(formatted);
                return;
            }

            if (IsDefinedValueOrCombinationOfValues(key))
            {
                Debug.Assert(
                    s_isFlagsEnum || dictionaryKeyPolicy != null,
                    "Should only be entered by flags enums or dictionary key policy."
                );
                string stringValue = FormatEnumAsString(key, value, dictionaryKeyPolicy);
                if (
                    dictionaryKeyPolicy is null
                    && _nameCacheForWriting.Count < NameCacheSizeSoftLimit
                )
                {
                    // Only attempt to cache if there is no dictionary key policy.
                    formatted = KdlEncodedText.Encode(stringValue, options.Encoder);
                    writer.WritePropertyName(formatted);
                    _nameCacheForWriting.TryAdd(key, formatted);
                }
                else
                {
                    // We also do not create a KdlEncodedText instance here because passing the string
                    // directly to the writer is cheaper than creating one and not caching it for reuse.
                    writer.WritePropertyName(stringValue);
                }

                return;
            }

            if (s_isSignedEnum)
            {
                writer.WritePropertyName(ConvertToInt64(value));
            }
            else
            {
                writer.WritePropertyName(key);
            }
        }

        private bool TryParseEnumFromString(ref KdlReader reader, out T result)
        {
            Debug.Assert(reader.TokenType is KdlTokenType.String or KdlTokenType.PropertyName);

            int bufferLength = reader.ValueLength;
            char[]? rentedBuffer = null;
            bool success;

            Span<char> charBuffer =
                bufferLength <= KdlConstants.StackallocCharThreshold
                    ? stackalloc char[KdlConstants.StackallocCharThreshold]
                    : (rentedBuffer = ArrayPool<char>.Shared.Rent(bufferLength));

            int charsWritten = reader.CopyString(charBuffer);
            charBuffer = charBuffer[..charsWritten];
            ReadOnlySpan<char> source = charBuffer.Trim();
            ConcurrentDictionary<string, ulong>.AlternateLookup<ReadOnlySpan<char>> lookup =
                _nameCacheForReading.GetAlternateLookup<ReadOnlySpan<char>>();
            if (lookup.TryGetValue(source, out ulong key))
            {
                result = ConvertFromUInt64(key);
                success = true;
                goto End;
            }

            if (KdlHelpers.IntegerRegex.IsMatch(source))
            {
                // We found an integer that is not an enum field name.
                if ((_converterOptions & EnumConverterOptions.AllowNumbers) != 0)
                {
                    success = Enum.TryParse(source, out result);
                }
                else
                {
                    result = default;
                    success = false;
                }
            }
            else
            {
                success = TryParseNamedEnum(source, out result);
            }

            if (success && _nameCacheForReading.Count < NameCacheSizeSoftLimit)
            {
                lookup.TryAdd(source, ConvertToUInt64(result));
            }

            End:
            if (rentedBuffer != null)
            {
                charBuffer.Clear();
                ArrayPool<char>.Shared.Return(rentedBuffer);
            }

            return success;
        }

        private bool TryParseNamedEnum(ReadOnlySpan<char> source,
            out T result)
        {
            Dictionary<string, EnumFieldInfo>.AlternateLookup<ReadOnlySpan<char>> lookup =
                _enumFieldInfoIndex.GetAlternateLookup<ReadOnlySpan<char>>();
            ReadOnlySpan<char> rest = source;
            ulong key = 0;

            do
            {
                ReadOnlySpan<char> next;
                int i = rest.IndexOf(',');
                if (i == -1)
                {
                    next = rest;
                    rest = default;
                }
                else
                {
                    next = rest[..i].TrimEnd();
                    rest = rest[(i + 1)..].TrimStart();
                }

                if (
                    lookup.TryGetValue(next,
                        out EnumFieldInfo? firstResult)
                    && firstResult.GetMatchingField(next) is EnumFieldInfo match
                )
                {
                    key |= match.Key;
                    continue;
                }

                result = default;
                return false;
            } while (!rest.IsEmpty);

            result = ConvertFromUInt64(key);
            return true;
        }

        private static ulong ConvertToUInt64(T value)
        {
            switch (s_enumTypeCode)
            {
                case TypeCode.Int32
                or TypeCode.UInt32:
                    return Unsafe.As<T, uint>(ref value);
                case TypeCode.Int64
                or TypeCode.UInt64:
                    return Unsafe.As<T, ulong>(ref value);
                case TypeCode.Int16
                or TypeCode.UInt16:
                    return Unsafe.As<T, ushort>(ref value);
                default:
                    Debug.Assert(s_enumTypeCode is TypeCode.SByte or TypeCode.Byte);
                    return Unsafe.As<T, byte>(ref value);
            }
            ;
        }

        private static long ConvertToInt64(T value)
        {
            Debug.Assert(s_isSignedEnum);
            switch (s_enumTypeCode)
            {
                case TypeCode.Int32:
                    return Unsafe.As<T, int>(ref value);
                case TypeCode.Int64:
                    return Unsafe.As<T, long>(ref value);
                case TypeCode.Int16:
                    return Unsafe.As<T, short>(ref value);
                default:
                    Debug.Assert(s_enumTypeCode is TypeCode.SByte);
                    return Unsafe.As<T, sbyte>(ref value);
            }
            ;
        }

        private static T ConvertFromUInt64(ulong value)
        {
            switch (s_enumTypeCode)
            {
                case TypeCode.Int32
                or TypeCode.UInt32:
                    uint uintValue = (uint)value;
                    return Unsafe.As<uint, T>(ref uintValue);

                case TypeCode.Int64
                or TypeCode.UInt64:
                    ulong ulongValue = value;
                    return Unsafe.As<ulong, T>(ref ulongValue);

                case TypeCode.Int16
                or TypeCode.UInt16:
                    ushort ushortValue = (ushort)value;
                    return Unsafe.As<ushort, T>(ref ushortValue);

                default:
                    Debug.Assert(s_enumTypeCode is TypeCode.SByte or TypeCode.Byte);
                    byte byteValue = (byte)value;
                    return Unsafe.As<byte, T>(ref byteValue);
            }
            ;
        }

        /// <summary>
        /// Attempt to format the enum value as a comma-separated string of flag values, or returns false if not a valid flag combination.
        /// </summary>
        private string FormatEnumAsString(ulong key, T value, KdlNamingPolicy? dictionaryKeyPolicy)
        {
            Debug.Assert(
                IsDefinedValueOrCombinationOfValues(key),
                "must only be invoked against valid enum values."
            );
            Debug.Assert(
                s_isFlagsEnum
                    || (dictionaryKeyPolicy is not null && Enum.IsDefined(typeof(T), value)),
                "must either be a flag type or computing a dictionary key policy."
            );

            if (s_isFlagsEnum)
            {
                using ValueStringBuilder sb = new(
                    stackalloc char[KdlConstants.StackallocCharThreshold]
                );
                ulong remainingBits = key;

                foreach (EnumFieldInfo enumField in _enumFieldInfo)
                {
                    ulong fieldKey = enumField.Key;
                    if (fieldKey == 0 ? key == 0 : (remainingBits & fieldKey) == fieldKey)
                    {
                        remainingBits &= ~fieldKey;
                        string name = dictionaryKeyPolicy is not null
                            ? ResolveAndValidateKdlName(
                                enumField.OriginalName,
                                dictionaryKeyPolicy,
                                enumField.Kind
                            )
                            : enumField.KdlName;

                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(name);

                        if (remainingBits == 0)
                        {
                            break;
                        }
                    }
                }

                Debug.Assert(
                    remainingBits == 0 && sb.Length > 0,
                    "unexpected remaining bits or empty string."
                );
                return sb.ToString();
            }
            else
            {
                Debug.Assert(dictionaryKeyPolicy != null);

                foreach (EnumFieldInfo enumField in _enumFieldInfo)
                {
                    // Search for an exact match and apply the key policy.
                    if (enumField.Key == key)
                    {
                        return ResolveAndValidateKdlName(
                            enumField.OriginalName,
                            dictionaryKeyPolicy,
                            enumField.Kind
                        );
                    }
                }

                Debug.Fail("should not have been reached.");
                return null;
            }
        }

        private bool IsDefinedValueOrCombinationOfValues(ulong key)
        {
            if (s_isFlagsEnum)
            {
                ulong remainingBits = key;

                foreach (EnumFieldInfo fieldInfo in _enumFieldInfo)
                {
                    ulong fieldKey = fieldInfo.Key;
                    if (fieldKey == 0 ? key == 0 : (remainingBits & fieldKey) == fieldKey)
                    {
                        remainingBits &= ~fieldKey;

                        if (remainingBits == 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            else
            {
                foreach (EnumFieldInfo fieldInfo in _enumFieldInfo)
                {
                    if (fieldInfo.Key == key)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling numberHandling)
        {
            if ((_converterOptions & EnumConverterOptions.AllowStrings) != 0)
            {
                // This explicitly ignores the integer component in converters configured as AllowNumbers | AllowStrings
                // which is the default for KdlStringEnumConverter. This sacrifices some precision in the schema for simplicity.

                if (s_isFlagsEnum)
                {
                    // Do not report enum values in case of flags.
                    return new() { Type = KdlSchemaType.String };
                }

                KdlNode enumValues = [];
                foreach (EnumFieldInfo fieldInfo in _enumFieldInfo)
                {
                    enumValues.Add((KdlElement)fieldInfo.KdlName);
                }

                return new() { Enum = enumValues };
            }

            return new() { Type = KdlSchemaType.Integer };
        }

        private static EnumFieldInfo[] ResolveEnumFields(KdlNamingPolicy? namingPolicy)
        {
            string[] names = Enum.GetNames<T>();
            T[] values = Enum.GetValues<T>();
            Debug.Assert(names.Length == values.Length);

            Dictionary<string, string>? enumMemberAttributes = null;
            foreach (
                FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
            )
            {
                if (field.GetCustomAttribute<KdlStringEnumMemberNameAttribute>() is { } attribute)
                {
                    (enumMemberAttributes ??= new(StringComparer.Ordinal)).Add(
                        field.Name,
                        attribute.Name
                    );
                }
            }

            var enumFields = new EnumFieldInfo[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                string originalName = names[i];
                T value = values[i];
                ulong key = ConvertToUInt64(value);
                EnumFieldNameKind kind;

                if (
                    enumMemberAttributes != null
                    && enumMemberAttributes.TryGetValue(originalName, out string? attributeName)
                )
                {
                    originalName = attributeName;
                    kind = EnumFieldNameKind.Attribute;
                }
                else
                {
                    kind =
                        namingPolicy != null
                            ? EnumFieldNameKind.NamingPolicy
                            : EnumFieldNameKind.Default;
                }

                string kdlName = ResolveAndValidateKdlName(originalName, namingPolicy, kind);
                enumFields[i] = new EnumFieldInfo(key, kind, originalName, kdlName);
            }

            return enumFields;
        }

        private static string ResolveAndValidateKdlName(
            string name,
            KdlNamingPolicy? namingPolicy,
            EnumFieldNameKind kind
        )
        {
            if (kind is not EnumFieldNameKind.Attribute && namingPolicy is not null)
            {
                // Do not apply a naming policy to names that are explicitly set via attributes.
                // This is consistent with KdlPropertyNameAttribute semantics.
                name = namingPolicy.ConvertName(name);
            }

            if (
                string.IsNullOrEmpty(name)
                || char.IsWhiteSpace(name[0])
                || char.IsWhiteSpace(name[^1])
                || (s_isFlagsEnum && name.AsSpan().IndexOf(',') >= 0)
            )
            {
                // Reject null or empty strings or strings with leading or trailing whitespace.
                // In the case of flags additionally reject strings containing commas.
                ThrowHelper.ThrowInvalidOperationException_UnsupportedEnumIdentifier(
                    typeof(T),
                    name
                );
            }

            return name;
        }

        private sealed class EnumFieldInfo(
            ulong key,
            EnumFieldNameKind kind,
            string originalName,
            string kdlName
        )
        {
            private List<EnumFieldInfo>? _conflictingFields;
            public EnumFieldNameKind Kind { get; } = kind;
            public ulong Key { get; } = key;
            public string OriginalName { get; } = originalName;
            public string KdlName { get; } = kdlName;

            /// <summary>
            /// Assuming we have field that conflicts with the current up to case sensitivity,
            /// append it to a list of trailing values for use by the enum value parser.
            /// </summary>
            public void AppendConflictingField(EnumFieldInfo other)
            {
                Debug.Assert(
                    KdlName.Equals(other.KdlName, StringComparison.OrdinalIgnoreCase),
                    "The conflicting entry must be equal up to case insensitivity."
                );

                if (
                    Kind is EnumFieldNameKind.Default
                    || KdlName.Equals(other.KdlName, StringComparison.Ordinal)
                )
                {
                    // Silently discard if the preceding entry is the default or has identical name.
                    return;
                }

                List<EnumFieldInfo> conflictingFields = _conflictingFields ??= [];

                // Walk the existing list to ensure we do not add duplicates.
                foreach (EnumFieldInfo conflictingField in conflictingFields)
                {
                    if (
                        conflictingField.Kind is EnumFieldNameKind.Default
                        || conflictingField.KdlName.Equals(other.KdlName, StringComparison.Ordinal)
                    )
                    {
                        return;
                    }
                }

                conflictingFields.Add(other);
            }

            public EnumFieldInfo? GetMatchingField(ReadOnlySpan<char> input)
            {
                Debug.Assert(
                    input.Equals(KdlName.AsSpan(), StringComparison.OrdinalIgnoreCase),
                    "Must equal the field name up to case insensitivity."
                );

                if (Kind is EnumFieldNameKind.Default || input.SequenceEqual(KdlName.AsSpan()))
                {
                    // Default enum names use case insensitive parsing so are always a match.
                    return this;
                }

                if (_conflictingFields is { } conflictingFields)
                {
                    Debug.Assert(conflictingFields.Count > 0);
                    foreach (EnumFieldInfo matchingField in conflictingFields)
                    {
                        if (
                            matchingField.Kind is EnumFieldNameKind.Default
                            || input.SequenceEqual(matchingField.KdlName.AsSpan())
                        )
                        {
                            return matchingField;
                        }
                    }
                }

                return null;
            }
        }

        private enum EnumFieldNameKind
        {
            Default = 0,
            NamingPolicy = 1,
            Attribute = 2,
        }
    }
}
