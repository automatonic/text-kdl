using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Kdl.Nodes;

namespace System.Text.Kdl.Schema
{
    internal sealed class KdlSchema
    {
        internal const string RefPropertyName = "$ref";
        internal const string CommentPropertyName = "$comment";
        internal const string TypePropertyName = "type";
        internal const string FormatPropertyName = "format";
        internal const string PatternPropertyName = "pattern";
        internal const string PropertiesPropertyName = "properties";
        internal const string RequiredPropertyName = "required";
        internal const string ItemsPropertyName = "items";
        internal const string AdditionalPropertiesPropertyName = "additionalProperties";
        internal const string EnumPropertyName = "enum";
        internal const string NotPropertyName = "not";
        internal const string AnyOfPropertyName = "anyOf";
        internal const string ConstPropertyName = "const";
        internal const string DefaultPropertyName = "default";
        internal const string MinLengthPropertyName = "minLength";
        internal const string MaxLengthPropertyName = "maxLength";

        public static KdlSchema CreateFalseSchema() => new(false);
        public static KdlSchema CreateTrueSchema() => new(true);

        public KdlSchema() { }
        private KdlSchema(bool trueOrFalse) { _trueOrFalse = trueOrFalse; }

        public bool IsTrue => _trueOrFalse is true;
        public bool IsFalse => _trueOrFalse is false;

        /// <summary>
        /// Per the KDL schema core specification section 4.3
        /// (https://kdl-schema.org/draft/2020-12/kdl-schema-core#name-kdl-schema-documents)
        /// A KDL schema must either be an object or a boolean.
        /// We represent false and true schemas using this flag.
        /// It is not possible to specify keywords in boolean schemas.
        /// </summary>
        private readonly bool? _trueOrFalse;

        public string? Ref { get => _ref; set { VerifyMutable(); _ref = value; } }
        private string? _ref;

        public string? Comment { get => _comment; set { VerifyMutable(); _comment = value; } }
        private string? _comment;

        public KdlSchemaType Type { get => _type; set { VerifyMutable(); _type = value; } }
        private KdlSchemaType _type = KdlSchemaType.Any;

        public string? Format { get => _format; set { VerifyMutable(); _format = value; } }
        private string? _format;

        public string? Pattern { get => _pattern; set { VerifyMutable(); _pattern = value; } }
        private string? _pattern;

        public KdlNode? Constant { get => _constant; set { VerifyMutable(); _constant = value; } }
        private KdlNode? _constant;

        public List<KeyValuePair<string, KdlSchema>>? Properties { get => _properties; set { VerifyMutable(); _properties = value; } }
        private List<KeyValuePair<string, KdlSchema>>? _properties;

        public List<string>? Required { get => _required; set { VerifyMutable(); _required = value; } }
        private List<string>? _required;

        public KdlSchema? Items { get => _items; set { VerifyMutable(); _items = value; } }
        private KdlSchema? _items;

        public KdlSchema? AdditionalProperties { get => _additionalProperties; set { VerifyMutable(); _additionalProperties = value; } }
        private KdlSchema? _additionalProperties;

        public KdlArray? Enum { get => _enum; set { VerifyMutable(); _enum = value; } }
        private KdlArray? _enum;

        public KdlSchema? Not { get => _not; set { VerifyMutable(); _not = value; } }
        private KdlSchema? _not;

        public List<KdlSchema>? AnyOf { get => _anyOf; set { VerifyMutable(); _anyOf = value; } }
        private List<KdlSchema>? _anyOf;

        public bool HasDefaultValue { get => _hasDefaultValue; set { VerifyMutable(); _hasDefaultValue = value; } }
        private bool _hasDefaultValue;

        public KdlNode? DefaultValue { get => _defaultValue; set { VerifyMutable(); _defaultValue = value; } }
        private KdlNode? _defaultValue;

        public int? MinLength { get => _minLength; set { VerifyMutable(); _minLength = value; } }
        private int? _minLength;

        public int? MaxLength { get => _maxLength; set { VerifyMutable(); _maxLength = value; } }
        private int? _maxLength;

        public KdlSchemaExporterContext? ExporterContext { get; set; }

        public int KeywordCount
        {
            get
            {
                if (_trueOrFalse != null)
                {
                    // Boolean schemas admit no keywords
                    return 0;
                }

                int count = 0;
                Count(Ref != null);
                Count(Comment != null);
                Count(Type != KdlSchemaType.Any);
                Count(Format != null);
                Count(Pattern != null);
                Count(Constant != null);
                Count(Properties != null);
                Count(Required != null);
                Count(Items != null);
                Count(AdditionalProperties != null);
                Count(Enum != null);
                Count(Not != null);
                Count(AnyOf != null);
                Count(HasDefaultValue);
                Count(MinLength != null);
                Count(MaxLength != null);

                return count;

                void Count(bool isKeywordSpecified)
                {
                    count += isKeywordSpecified ? 1 : 0;
                }
            }
        }

        public void MakeNullable()
        {
            if (_trueOrFalse != null)
            {
                // boolean schemas do not admit type keywords.
                return;
            }

            if (Type != KdlSchemaType.Any)
            {
                Type |= KdlSchemaType.Null;
            }
        }

        public KdlNode ToKdlNode(KdlSchemaExporterOptions options)
        {
            if (_trueOrFalse is { } boolSchema)
            {
                return CompleteSchema((KdlNode)boolSchema);
            }

            var objSchema = new KdlObject();

            if (Ref != null)
            {
                objSchema.Add(RefPropertyName, Ref);
            }

            if (Comment != null)
            {
                objSchema.Add(CommentPropertyName, Comment);
            }

            if (MapSchemaType(Type) is KdlNode type)
            {
                objSchema.Add(TypePropertyName, type);
            }

            if (Format != null)
            {
                objSchema.Add(FormatPropertyName, Format);
            }

            if (Pattern != null)
            {
                objSchema.Add(PatternPropertyName, Pattern);
            }

            if (Constant != null)
            {
                objSchema.Add(ConstPropertyName, Constant);
            }

            if (Properties != null)
            {
                var properties = new KdlObject();
                foreach (KeyValuePair<string, KdlSchema> property in Properties)
                {
                    properties.Add(property.Key, property.Value.ToKdlNode(options));
                }

                objSchema.Add(PropertiesPropertyName, properties);
            }

            if (Required != null)
            {
                var requiredArray = new KdlArray();
                foreach (string requiredProperty in Required)
                {
                    requiredArray.Add((KdlNode)requiredProperty);
                }

                objSchema.Add(RequiredPropertyName, requiredArray);
            }

            if (Items != null)
            {
                objSchema.Add(ItemsPropertyName, Items.ToKdlNode(options));
            }

            if (AdditionalProperties != null)
            {
                objSchema.Add(AdditionalPropertiesPropertyName, AdditionalProperties.ToKdlNode(options));
            }

            if (Enum != null)
            {
                objSchema.Add(EnumPropertyName, Enum);
            }

            if (Not != null)
            {
                objSchema.Add(NotPropertyName, Not.ToKdlNode(options));
            }

            if (AnyOf != null)
            {
                KdlArray anyOfArray = [];
                foreach (KdlSchema schema in AnyOf)
                {
                    anyOfArray.Add(schema.ToKdlNode(options));
                }

                objSchema.Add(AnyOfPropertyName, anyOfArray);
            }

            if (HasDefaultValue)
            {
                objSchema.Add(DefaultPropertyName, DefaultValue);
            }

            if (MinLength is int minLength)
            {
                objSchema.Add(MinLengthPropertyName, (KdlNode)minLength);
            }

            if (MaxLength is int maxLength)
            {
                objSchema.Add(MaxLengthPropertyName, (KdlNode)maxLength);
            }

            return CompleteSchema(objSchema);

            KdlNode CompleteSchema(KdlNode schema)
            {
                if (ExporterContext is { } context)
                {
                    Debug.Assert(options.TransformSchemaNode != null, "context should only be populated if a callback is present.");
                    // Apply any user-defined transformations to the schema.
                    return options.TransformSchemaNode(context, schema);
                }

                return schema;
            }
        }

        /// <summary>
        /// If the schema is boolean, replaces it with a semantically
        /// equivalent object schema that allows appending keywords.
        /// </summary>
        public static void EnsureMutable(ref KdlSchema schema)
        {
            switch (schema._trueOrFalse)
            {
                case false:
                    schema = new KdlSchema { Not = CreateTrueSchema() };
                    break;
                case true:
                    schema = new KdlSchema();
                    break;
            }
        }

        private static ReadOnlySpan<KdlSchemaType> s_schemaValues =>
        [
            // NB the order of these values influences order of types in the rendered schema
            KdlSchemaType.String,
            KdlSchemaType.Integer,
            KdlSchemaType.Number,
            KdlSchemaType.Boolean,
            KdlSchemaType.Array,
            KdlSchemaType.Object,
            KdlSchemaType.Null,
        ];

        private void VerifyMutable()
        {
            Debug.Assert(_trueOrFalse is null, "Schema is not mutable");
            if (_trueOrFalse is not null)
            {
                Throw();
                static void Throw() => throw new InvalidOperationException();
            }
        }

        public static KdlNode? MapSchemaType(KdlSchemaType schemaType)
        {
            if (schemaType is KdlSchemaType.Any)
            {
                return null;
            }

            if (ToIdentifier(schemaType) is string identifier)
            {
                return identifier;
            }

            var array = new KdlArray();
            foreach (KdlSchemaType type in s_schemaValues)
            {
                if ((schemaType & type) != 0)
                {
                    array.Add((KdlNode)ToIdentifier(type)!);
                }
            }

            return array;

            static string? ToIdentifier(KdlSchemaType schemaType)
            {
                return schemaType switch
                {
                    KdlSchemaType.Null => "null",
                    KdlSchemaType.Boolean => "boolean",
                    KdlSchemaType.Integer => "integer",
                    KdlSchemaType.Number => "number",
                    KdlSchemaType.String => "string",
                    KdlSchemaType.Array => "array",
                    KdlSchemaType.Object => "object",
                    _ => null,
                };
            }
        }
    }
}
