﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Kdl.Nodes;
using System.Text.Kdl.Schema;

namespace System.Text.Kdl.Serialization.Converters
{
    internal sealed class VersionConverter : KdlPrimitiveConverter<Version?>
    {
#if NET
        private const int MinimumVersionLength = 3; // 0.0

        private const int MaximumVersionLength = 43; // 2147483647.2147483647.2147483647.2147483647

        private const int MaximumEscapedVersionLength = KdlConstants.MaxExpansionFactorWhileEscaping * MaximumVersionLength;
#endif

        public override Version? Read(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            if (reader.TokenType is KdlTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != KdlTokenType.String)
            {
                ThrowHelper.ThrowInvalidOperationException_ExpectedString(reader.TokenType);
            }

            return ReadCore(ref reader);
        }

        private static Version ReadCore(ref KdlReader reader)
        {
            Debug.Assert(reader.TokenType is KdlTokenType.PropertyName or KdlTokenType.String);

#if NET
            if (!KdlHelpers.IsInRangeInclusive(reader.ValueLength, MinimumVersionLength, MaximumEscapedVersionLength))
            {
                ThrowHelper.ThrowFormatException(DataType.Version);
            }

            Span<char> charBuffer = stackalloc char[MaximumEscapedVersionLength];
            int bytesWritten = reader.CopyString(charBuffer);
            ReadOnlySpan<char> source = charBuffer.Slice(0, bytesWritten);

            if (!char.IsDigit(source[0]) || !char.IsDigit(source[^1]))
            {
                // Since leading and trailing whitespaces are forbidden throughout System.Text.Kdl converters
                // we need to make sure that our input doesn't have them,
                // and if it has - we need to throw, to match behaviour of other converters
                // since Version.TryParse allows them and silently parses input to Version
                ThrowHelper.ThrowFormatException(DataType.Version);
            }

            if (Version.TryParse(source, out Version? result))
            {
                return result;
            }
#else
            string? versionString = reader.GetString();
            if (!string.IsNullOrEmpty(versionString) && (!char.IsDigit(versionString[0]) || !char.IsDigit(versionString[versionString.Length - 1])))
            {
                // Since leading and trailing whitespaces are forbidden throughout System.Text.Kdl converters
                // we need to make sure that our input doesn't have them,
                // and if it has - we need to throw, to match behaviour of other converters
                // since Version.TryParse allows them and silently parses input to Version
                ThrowHelper.ThrowFormatException(DataType.Version);
            }
            if (Version.TryParse(versionString, out Version? result))
            {
                return result;
            }
#endif
            ThrowHelper.ThrowKdlException();
            return null;
        }

        public override void Write(KdlWriter writer, Version? value, KdlSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

#if NET
#if NET8_0_OR_GREATER
            Span<byte> span = stackalloc byte[MaximumVersionLength];
#else
            Span<char> span = stackalloc char[MaximumVersionLength];
#endif
            bool formattedSuccessfully = value.TryFormat(span, out int charsWritten);
            Debug.Assert(formattedSuccessfully && charsWritten >= MinimumVersionLength);
            writer.WriteStringValue(span.Slice(0, charsWritten));
#else
            writer.WriteStringValue(value.ToString());
#endif
        }

        internal override Version ReadAsPropertyNameCore(ref KdlReader reader, Type typeToConvert, KdlSerializerOptions options)
        {
            return ReadCore(ref reader);
        }

        internal override void WriteAsPropertyNameCore(KdlWriter writer, Version value, KdlSerializerOptions options, bool isWritingExtensionDataProperty)
        {
            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(value));
            }

#if NET
#if NET8_0_OR_GREATER
            Span<byte> span = stackalloc byte[MaximumVersionLength];
#else
            Span<char> span = stackalloc char[MaximumVersionLength];
#endif
            bool formattedSuccessfully = value.TryFormat(span, out int charsWritten);
            Debug.Assert(formattedSuccessfully && charsWritten >= MinimumVersionLength);
            writer.WritePropertyName(span.Slice(0, charsWritten));
#else
            writer.WritePropertyName(value.ToString());
#endif
        }

        internal override KdlSchema? GetSchema(KdlNumberHandling _) =>
            new()
            {
                Type = KdlSchemaType.String,
                Comment = "Represents a version string.",
                Pattern = @"^\d+(\.\d+){1,3}$",
            };
    }
}
