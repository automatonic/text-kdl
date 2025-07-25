using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Automatonic.Text.Kdl.RandomAccess;

namespace Automatonic.Text.Kdl
{
    internal static partial class ThrowHelper
    {
        // If the exception source is this value, the serializer will re-throw as KdlException.
        public const string ExceptionSourceValueToRethrowAsKdlException =
            "Automatonic.Text.Kdl.Rethrowable";

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_NewLine(string parameterName)
        {
            throw GetArgumentOutOfRangeException(parameterName, SR.InvalidNewLine);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_IndentCharacter(string parameterName)
        {
            throw GetArgumentOutOfRangeException(parameterName, SR.InvalidIndentCharacter);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_IndentSize(
            string parameterName,
            int minimumSize,
            int maximumSize
        )
        {
            throw GetArgumentOutOfRangeException(
                parameterName,
                Format(SR.InvalidIndentSize, minimumSize, maximumSize)
            );
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_MaxDepthMustBePositive(
            string parameterName
        )
        {
            throw GetArgumentOutOfRangeException(parameterName, SR.MaxDepthMustBePositive);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_KdlNumberExponentTooLarge(
            string parameterName
        )
        {
            throw GetArgumentOutOfRangeException(parameterName, SR.KdlNumberExponentTooLarge);
        }

        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(
            string parameterName,
            string message
        )
        {
            return new ArgumentOutOfRangeException(parameterName, message);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_CommentEnumMustBeInRange(
            string parameterName
        )
        {
            throw GetArgumentOutOfRangeException(parameterName, SR.CommentHandlingMustBeValid);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_ArrayIndexNegative(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, SR.ArrayIndexNegative);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_KdlConverterFactory_TypeNotSupported(
            Type typeToConvert
        )
        {
            throw new ArgumentOutOfRangeException(
                nameof(typeToConvert),
                Format(SR.SerializerConverterFactoryInvalidArgument, typeToConvert.FullName)
            );
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException_NeedNonNegNum(string paramName)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                SR.ArgumentOutOfRange_Generic_MustBeNonNegative
            );
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_InvalidOffLen()
        {
            throw new ArgumentException(SR.Argument_InvalidOffLen);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_ArrayTooSmall(string paramName)
        {
            throw new ArgumentException(SR.ArrayTooSmall, paramName);
        }

        private static ArgumentException GetArgumentException(string message)
        {
            return new ArgumentException(message);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(string message)
        {
            throw GetArgumentException(message);
        }

        public static InvalidOperationException GetInvalidOperationException_CallFlushFirst(
            int _buffered
        )
        {
            return GetInvalidOperationException(Format(SR.CallFlushToAvoidDataLoss, _buffered));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_DestinationTooShort()
        {
            throw GetArgumentException(SR.DestinationTooShort);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_PropertyNameTooLarge(int tokenLength)
        {
            throw GetArgumentException(Format(SR.PropertyNameTooLarge, tokenLength));
        }
        [DoesNotReturn]
        public static void ThrowArgumentException_NodeNameTooLarge(int tokenLength)
        {
            throw GetArgumentException(Format(SR.NodeNameTooLarge, tokenLength));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_ValueTooLarge(long tokenLength)
        {
            throw GetArgumentException(Format(SR.ValueTooLarge, tokenLength));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_ValueNotSupported()
        {
            throw GetArgumentException(SR.SpecialNumberValuesNotSupported);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NeedLargerSpan()
        {
            throw GetInvalidOperationException(SR.FailedToGetLargerSpan);
        }

        [DoesNotReturn]
        public static void ThrowPropertyNameTooLargeArgumentException(int length)
        {
            throw GetArgumentException(Format(SR.PropertyNameTooLarge, length));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(
            ReadOnlySpan<byte> propertyName,
            ReadOnlySpan<byte> value
        )
        {
            if (propertyName.Length > KdlConstants.MaxUnescapedTokenSize)
            {
                ThrowArgumentException(Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > KdlConstants.MaxUnescapedTokenSize);
                ThrowArgumentException(Format(SR.ValueTooLarge, value.Length));
            }
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(
            ReadOnlySpan<byte> propertyName,
            ReadOnlySpan<char> value
        )
        {
            if (propertyName.Length > KdlConstants.MaxUnescapedTokenSize)
            {
                ThrowArgumentException(Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > KdlConstants.MaxCharacterTokenSize);
                ThrowArgumentException(Format(SR.ValueTooLarge, value.Length));
            }
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(
            ReadOnlySpan<char> propertyName,
            ReadOnlySpan<byte> value
        )
        {
            if (propertyName.Length > KdlConstants.MaxCharacterTokenSize)
            {
                ThrowArgumentException(Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > KdlConstants.MaxUnescapedTokenSize);
                ThrowArgumentException(Format(SR.ValueTooLarge, value.Length));
            }
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(
            ReadOnlySpan<char> propertyName,
            ReadOnlySpan<char> value
        )
        {
            if (propertyName.Length > KdlConstants.MaxCharacterTokenSize)
            {
                ThrowArgumentException(Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > KdlConstants.MaxCharacterTokenSize);
                ThrowArgumentException(Format(SR.ValueTooLarge, value.Length));
            }
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationOrArgumentException(
            ReadOnlySpan<byte> propertyName,
            int currentDepth,
            int maxDepth
        )
        {
            currentDepth &= KdlConstants.RemoveFlagsBitMask;
            if (currentDepth >= maxDepth)
            {
                ThrowInvalidOperationException(Format(SR.DepthTooLarge, currentDepth, maxDepth));
            }
            else
            {
                Debug.Assert(propertyName.Length > KdlConstants.MaxCharacterTokenSize);
                ThrowArgumentException(Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(int currentDepth, int maxDepth)
        {
            currentDepth &= KdlConstants.RemoveFlagsBitMask;
            Debug.Assert(currentDepth >= maxDepth);
            ThrowInvalidOperationException(Format(SR.DepthTooLarge, currentDepth, maxDepth));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(string message)
        {
            throw GetInvalidOperationException(message);
        }

        private static InvalidOperationException GetInvalidOperationException(string message)
        {
            return new InvalidOperationException(message)
            {
                Source = ExceptionSourceValueToRethrowAsKdlException,
            };
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DepthNonZeroOrEmptyKdl(int currentDepth)
        {
            throw GetInvalidOperationException(currentDepth);
        }

        private static InvalidOperationException GetInvalidOperationException(int currentDepth)
        {
            currentDepth &= KdlConstants.RemoveFlagsBitMask;
            if (currentDepth != 0)
            {
                return GetInvalidOperationException(Format(SR.ZeroDepthAtEnd, currentDepth));
            }
            else
            {
                return GetInvalidOperationException(SR.EmptyKdlIsInvalid);
            }
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationOrArgumentException(
            ReadOnlySpan<char> propertyName,
            int currentDepth,
            int maxDepth
        )
        {
            currentDepth &= KdlConstants.RemoveFlagsBitMask;
            if (currentDepth >= maxDepth)
            {
                ThrowInvalidOperationException(Format(SR.DepthTooLarge, currentDepth, maxDepth));
            }
            else
            {
                Debug.Assert(propertyName.Length > KdlConstants.MaxCharacterTokenSize);
                ThrowArgumentException(Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedArray(
            KdlTokenType tokenType
        )
        {
            return GetInvalidOperationException("array", tokenType);
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedObject(
            KdlTokenType tokenType
        )
        {
            return GetInvalidOperationException("object", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedNumber(KdlTokenType tokenType)
        {
            throw GetInvalidOperationException("number", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedBoolean(KdlTokenType tokenType)
        {
            throw GetInvalidOperationException("boolean", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedString(KdlTokenType tokenType)
        {
            throw GetInvalidOperationException("string", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedPropertyName(
            KdlTokenType tokenType
        )
        {
            throw GetInvalidOperationException("propertyName", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedStringComparison(
            KdlTokenType tokenType
        )
        {
            throw GetInvalidOperationException(tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedComment(KdlTokenType tokenType)
        {
            throw GetInvalidOperationException("comment", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_CannotSkipOnPartial()
        {
            throw GetInvalidOperationException(SR.CannotSkip);
        }

        private static InvalidOperationException GetInvalidOperationException(
            string message,
            KdlTokenType tokenType
        )
        {
            return GetInvalidOperationException(Format(SR.InvalidCast, tokenType, message));
        }

        private static InvalidOperationException GetInvalidOperationException(
            KdlTokenType tokenType
        )
        {
            return GetInvalidOperationException(Format(SR.InvalidComparison, tokenType));
        }

        [DoesNotReturn]
        internal static void ThrowKdlElementWrongTypeException(
            KdlTokenType expectedType,
            KdlTokenType actualType
        )
        {
            throw GetKdlElementWrongTypeException(
                expectedType.ToValueKind(),
                actualType.ToValueKind()
            );
        }

        internal static InvalidOperationException GetKdlElementWrongTypeException(
            KdlValueKind expectedType,
            KdlValueKind actualType
        )
        {
            return GetInvalidOperationException(
                Format(SR.KdlElementHasWrongType, expectedType, actualType)
            );
        }

        internal static InvalidOperationException GetKdlElementWrongTypeException(
            string expectedTypeName,
            KdlValueKind actualType
        )
        {
            return GetInvalidOperationException(
                Format(SR.KdlElementHasWrongType, expectedTypeName, actualType)
            );
        }

        [DoesNotReturn]
        public static void ThrowKdlReaderException(
            ref KdlReader kdl,
            ExceptionResource resource,
            byte nextByte = default,
            ReadOnlySpan<byte> bytes = default
        )
        {
            throw GetKdlReaderException(ref kdl, resource, nextByte, bytes);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static KdlException GetKdlReaderException(
            ref KdlReader kdl,
            ExceptionResource resource,
            byte nextByte,
            ReadOnlySpan<byte> bytes
        )
        {
            string message = GetResourceString(
                ref kdl,
                resource,
                nextByte,
                KdlHelpers.Utf8GetString(bytes)
            );

            long lineNumber = kdl.CurrentState._lineNumber;
            long bytePositionInLine = kdl.CurrentState._bytePositionInLine;

            message += $" LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";
            return new KdlReaderException(message, lineNumber, bytePositionInLine);
        }

        private static bool IsPrintable(byte value) => value is >= 0x20 and < 0x7F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetPrintableString(byte value)
        {
            return IsPrintable(value) ? ((char)value).ToString() : $"0x{value:X2}";
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(
            ref KdlReader kdl,
            ExceptionResource resource,
            byte nextByte,
            string characters
        )
        {
            string character = GetPrintableString(nextByte);

            string message = "";
            switch (resource)
            {
                case ExceptionResource.NodeDepthTooLarge:
                    message = Format(SR.NodeDepthTooLarge, kdl.CurrentState.Options.MaxDepth);
                    break;
                case ExceptionResource.MismatchedChildrenBlock:
                    message = Format(SR.MismatchedChildrenBlock, character);
                    break;
                case ExceptionResource.TrailingCommaNotAllowedBeforeArrayEnd:
                    message = SR.TrailingCommaNotAllowedBeforeArrayEnd;
                    break;
                case ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd:
                    message = SR.TrailingCommaNotAllowedBeforeObjectEnd;
                    break;
                case ExceptionResource.EndOfStringNotFound:
                    message = SR.EndOfStringNotFound;
                    break;
                case ExceptionResource.RequiredDigitNotFoundAfterSign:
                    message = Format(SR.RequiredDigitNotFoundAfterSign, character);
                    break;
                case ExceptionResource.RequiredDigitNotFoundAfterDecimal:
                    message = Format(SR.RequiredDigitNotFoundAfterDecimal, character);
                    break;
                case ExceptionResource.RequiredDigitNotFoundEndOfData:
                    message = SR.RequiredDigitNotFoundEndOfData;
                    break;
                case ExceptionResource.ExpectedEndOfDigitNotFound:
                    message = Format(SR.ExpectedEndOfDigitNotFound, character);
                    break;
                case ExceptionResource.ExpectedNextDigitEValueNotFound:
                    message = Format(SR.ExpectedNextDigitEValueNotFound, character);
                    break;
                case ExceptionResource.ExpectedSeparatorAfterPropertyNameNotFound:
                    message = Format(SR.ExpectedSeparatorAfterPropertyNameNotFound, character);
                    break;
                case ExceptionResource.ExpectedStartOfPropertyNotFound:
                    message = Format(SR.ExpectedStartOfPropertyNotFound, character);
                    break;
                case ExceptionResource.ExpectedStartOfPropertyOrValueNotFound:
                    message = SR.ExpectedStartOfPropertyOrValueNotFound;
                    break;
                case ExceptionResource.ExpectedStartOfPropertyOrValueAfterComment:
                    message = Format(SR.ExpectedStartOfPropertyOrValueAfterComment, character);
                    break;
                case ExceptionResource.ExpectedStartOfValueNotFound:
                    message = Format(SR.ExpectedStartOfValueNotFound, character);
                    break;
                case ExceptionResource.ExpectedValueAfterPropertyNameNotFound:
                    message = SR.ExpectedValueAfterPropertyNameNotFound;
                    break;
                case ExceptionResource.FoundInvalidCharacter:
                    message = Format(SR.FoundInvalidCharacter, character);
                    break;
                case ExceptionResource.InvalidEndOfKdlNonPrimitive:
                    message = Format(SR.InvalidEndOfKdlNonPrimitive, kdl.TokenType);
                    break;
                case ExceptionResource.ExpectedFalse:
                    message = Format(SR.ExpectedFalse, characters);
                    break;
                case ExceptionResource.ExpectedNull:
                    message = Format(SR.ExpectedNull, characters);
                    break;
                case ExceptionResource.ExpectedTrue:
                    message = Format(SR.ExpectedTrue, characters);
                    break;
                case ExceptionResource.InvalidCharacterWithinString:
                    message = Format(SR.InvalidCharacterWithinString, character);
                    break;
                case ExceptionResource.InvalidCharacterAfterEscapeWithinString:
                    message = Format(SR.InvalidCharacterAfterEscapeWithinString, character);
                    break;
                case ExceptionResource.InvalidHexCharacterWithinString:
                    message = Format(SR.InvalidHexCharacterWithinString, character);
                    break;
                case ExceptionResource.EndOfCommentNotFound:
                    message = SR.EndOfCommentNotFound;
                    break;
                case ExceptionResource.ZeroDepthAtEnd:
                    message = Format(SR.ZeroDepthAtEnd);
                    break;
                case ExceptionResource.ExpectedKdlTokens:
                    message = SR.ExpectedKdlTokens;
                    break;
                case ExceptionResource.NotEnoughData:
                    message = SR.NotEnoughData;
                    break;
                case ExceptionResource.ExpectedOneCompleteToken:
                    message = SR.ExpectedOneCompleteToken;
                    break;
                case ExceptionResource.InvalidCharacterAtStartOfComment:
                    message = Format(SR.InvalidCharacterAtStartOfComment, character);
                    break;
                case ExceptionResource.UnexpectedEndOfDataWhileReadingComment:
                    message = Format(SR.UnexpectedEndOfDataWhileReadingComment);
                    break;
                case ExceptionResource.UnexpectedEndOfLineSeparator:
                    message = Format(SR.UnexpectedEndOfLineSeparator);
                    break;
                case ExceptionResource.InvalidLeadingZeroInNumber:
                    message = Format(SR.InvalidLeadingZeroInNumber, character);
                    break;
                default:
                    Debug.Fail(
                        $"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message."
                    );
                    break;
            }

            return message;
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(
            ExceptionResource resource,
            int currentDepth,
            int maxDepth,
            byte token,
            KdlTokenType tokenType
        )
        {
            throw GetInvalidOperationException(resource, currentDepth, maxDepth, token, tokenType);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_InvalidCommentValue()
        {
            throw new ArgumentException(SR.CannotWriteCommentWithEmbeddedDelimiter);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_InvalidUTF8(ReadOnlySpan<byte> value)
        {
            var builder = new StringBuilder();

            int printFirst10 = Math.Min(value.Length, 10);

            for (int i = 0; i < printFirst10; i++)
            {
                byte nextByte = value[i];
                if (IsPrintable(nextByte))
                {
                    builder.Append((char)nextByte);
                }
                else
                {
                    builder.Append($"0x{nextByte:X2}");
                }
            }

            if (printFirst10 < value.Length)
            {
                builder.Append("...");
            }

            throw new ArgumentException(Format(SR.CannotEncodeInvalidUTF8, builder));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_InvalidUTF16(int charAsInt)
        {
            throw new ArgumentException(Format(SR.CannotEncodeInvalidUTF16, $"0x{charAsInt:X2}"));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ReadInvalidUTF16(int charAsInt)
        {
            throw GetInvalidOperationException(
                Format(SR.CannotReadInvalidUTF16, $"0x{charAsInt:X2}")
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ReadIncompleteUTF16()
        {
            throw GetInvalidOperationException(SR.CannotReadIncompleteUTF16);
        }

        public static InvalidOperationException GetInvalidOperationException_ReadInvalidUTF8(
            DecoderFallbackException? innerException = null
        )
        {
            return GetInvalidOperationException(SR.CannotTranscodeInvalidUtf8, innerException);
        }

        public static ArgumentException GetArgumentException_ReadInvalidUTF16(
            EncoderFallbackException innerException
        )
        {
            return new ArgumentException(SR.CannotTranscodeInvalidUtf16, innerException);
        }

        public static InvalidOperationException GetInvalidOperationException(
            string message,
            Exception? innerException
        )
        {
            InvalidOperationException ex = new InvalidOperationException(message, innerException)
            {
                Source = ExceptionSourceValueToRethrowAsKdlException,
            };
            return ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException GetInvalidOperationException(
            ExceptionResource resource,
            int currentDepth,
            int maxDepth,
            byte token,
            KdlTokenType tokenType
        )
        {
            string message = GetResourceString(resource, currentDepth, maxDepth, token, tokenType);
            InvalidOperationException ex = GetInvalidOperationException(message);
            ex.Source = ExceptionSourceValueToRethrowAsKdlException;
            return ex;
        }

        [DoesNotReturn]
        public static void ThrowOutOfMemoryException(uint capacity)
        {
            throw new OutOfMemoryException(Format(SR.BufferMaximumSizeExceeded, capacity));
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(
            ExceptionResource resource,
            int currentDepth,
            int maxDepth,
            byte token,
            KdlTokenType tokenType
        )
        {
            string message = "";
            switch (resource)
            {
                case ExceptionResource.MismatchedChildrenBlock:
                    Debug.Assert(token is KdlConstants.CloseBrace);
                    message =
                        (tokenType == KdlTokenType.PropertyName)
                            ? Format(SR.CannotWriteEndAfterProperty, (char)token)
                            : Format(SR.MismatchedChildrenBlock, (char)token);
                    break;
                case ExceptionResource.DepthTooLarge:
                    message = Format(
                        SR.DepthTooLarge,
                        currentDepth & KdlConstants.RemoveFlagsBitMask,
                        maxDepth
                    );
                    break;
                case ExceptionResource.CannotStartObjectArrayWithoutProperty:
                    message = Format(SR.CannotStartObjectArrayWithoutProperty, tokenType);
                    break;
                case ExceptionResource.CannotStartObjectArrayAfterPrimitiveOrClose:
                    message = Format(SR.CannotStartObjectArrayAfterPrimitiveOrClose, tokenType);
                    break;
                case ExceptionResource.CannotWriteValueWithinObject:
                    message = Format(SR.CannotWriteValueWithinObject, tokenType);
                    break;
                case ExceptionResource.CannotWritePropertyWithinArray:
                    message =
                        (tokenType == KdlTokenType.PropertyName)
                            ? Format(SR.CannotWritePropertyAfterProperty)
                            : Format(SR.CannotWritePropertyWithinArray, tokenType);
                    break;
                case ExceptionResource.CannotWriteValueAfterPrimitiveOrClose:
                    message = Format(SR.CannotWriteValueAfterPrimitiveOrClose, tokenType);
                    break;
                default:
                    Debug.Fail(
                        $"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message."
                    );
                    break;
            }

            return message;
        }

        [DoesNotReturn]
        public static void ThrowFormatException()
        {
            throw new FormatException { Source = ExceptionSourceValueToRethrowAsKdlException };
        }

        public static void ThrowFormatException(NumericType numericType)
        {
            string message = "";

            switch (numericType)
            {
                case NumericType.Byte:
                    message = SR.FormatByte;
                    break;
                case NumericType.SByte:
                    message = SR.FormatSByte;
                    break;
                case NumericType.Int16:
                    message = SR.FormatInt16;
                    break;
                case NumericType.Int32:
                    message = SR.FormatInt32;
                    break;
                case NumericType.Int64:
                    message = SR.FormatInt64;
                    break;
                case NumericType.Int128:
                    message = SR.FormatInt128;
                    break;
                case NumericType.UInt16:
                    message = SR.FormatUInt16;
                    break;
                case NumericType.UInt32:
                    message = SR.FormatUInt32;
                    break;
                case NumericType.UInt64:
                    message = SR.FormatUInt64;
                    break;
                case NumericType.UInt128:
                    message = SR.FormatUInt128;
                    break;
                case NumericType.Half:
                    message = SR.FormatHalf;
                    break;
                case NumericType.Single:
                    message = SR.FormatSingle;
                    break;
                case NumericType.Double:
                    message = SR.FormatDouble;
                    break;
                case NumericType.Decimal:
                    message = SR.FormatDecimal;
                    break;
                default:
                    Debug.Fail(
                        $"The NumericType enum value: {numericType} is not part of the switch. Add the appropriate case and exception message."
                    );
                    break;
            }

            throw new FormatException(message)
            {
                Source = ExceptionSourceValueToRethrowAsKdlException,
            };
        }

        [DoesNotReturn]
        public static void ThrowFormatException(DataType dataType)
        {
            string message = "";

            switch (dataType)
            {
                case DataType.Boolean:
                case DataType.DateOnly:
                case DataType.DateTime:
                case DataType.DateTimeOffset:
                case DataType.TimeOnly:
                case DataType.TimeSpan:
                case DataType.Guid:
                case DataType.Version:
                    message = Format(SR.UnsupportedFormat, dataType);
                    break;
                case DataType.Base64String:
                    message = SR.CannotDecodeInvalidBase64;
                    break;
                default:
                    Debug.Fail(
                        $"The DataType enum value: {dataType} is not part of the switch. Add the appropriate case and exception message."
                    );
                    break;
            }

            throw new FormatException(message)
            {
                Source = ExceptionSourceValueToRethrowAsKdlException,
            };
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExpectedChar(KdlTokenType tokenType)
        {
            throw GetInvalidOperationException("char", tokenType);
        }

        [DoesNotReturn]
        public static void ThrowObjectDisposedException_KdlWriter()
        {
            throw new ObjectDisposedException(nameof(KdlWriter));
        }

        [DoesNotReturn]
        public static void ThrowObjectDisposedException_KdlDocument()
        {
            throw new ObjectDisposedException(nameof(KdlReadOnlyDocument));
        }

        [DoesNotReturn]
        public static void ThrowInsufficientExecutionStackException_KdlElementDeepEqualsInsufficientExecutionStack()
        {
            throw new InsufficientExecutionStackException(
                SR.KdlElementDeepEqualsInsufficientExecutionStack
            );
        }
    }

    public enum ExceptionResource
    {
        EndOfCommentNotFound,
        EndOfStringNotFound,
        RequiredDigitNotFoundAfterDecimal,
        RequiredDigitNotFoundAfterSign,
        RequiredDigitNotFoundEndOfData,
        ExpectedEndOfDigitNotFound,
        ExpectedFalse,
        ExpectedNextDigitEValueNotFound,
        ExpectedNull,
        ExpectedSeparatorAfterPropertyNameNotFound,
        ExpectedStartOfPropertyNotFound,
        ExpectedStartOfPropertyOrValueNotFound,
        ExpectedStartOfPropertyOrValueAfterComment,
        ExpectedStartOfValueNotFound,
        ExpectedTrue,
        ExpectedValueAfterPropertyNameNotFound,
        FoundInvalidCharacter,
        InvalidCharacterWithinString,
        InvalidCharacterAfterEscapeWithinString,
        InvalidHexCharacterWithinString,
        InvalidEndOfKdlNonPrimitive,
        MismatchedChildrenBlock,
        NodeDepthTooLarge,
        ZeroDepthAtEnd,
        DepthTooLarge,
        CannotStartObjectArrayWithoutProperty,
        CannotStartObjectArrayAfterPrimitiveOrClose,
        CannotWriteValueWithinObject,
        CannotWriteValueAfterPrimitiveOrClose,
        CannotWritePropertyWithinArray,
        ExpectedKdlTokens,
        TrailingCommaNotAllowedBeforeArrayEnd,
        TrailingCommaNotAllowedBeforeObjectEnd,
        InvalidCharacterAtStartOfComment,
        UnexpectedEndOfDataWhileReadingComment,
        UnexpectedEndOfLineSeparator,
        ExpectedOneCompleteToken,
        NotEnoughData,
        InvalidLeadingZeroInNumber,
    }

    internal enum NumericType
    {
        Byte,
        SByte,
        Int16,
        Int32,
        Int64,
        Int128,
        UInt16,
        UInt32,
        UInt64,
        UInt128,
        Half,
        Single,
        Double,
        Decimal,
    }

    internal enum DataType
    {
        Boolean,
        DateOnly,
        DateTime,
        DateTimeOffset,
        TimeOnly,
        TimeSpan,
        Base64String,
        Guid,
        Version,
    }
}
