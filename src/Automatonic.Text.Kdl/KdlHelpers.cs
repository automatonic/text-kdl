using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Automatonic.Text.Kdl
{
    internal static partial class KdlHelpers
    {
        /// <summary>
        /// Returns the unescaped span for the given reader.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetUnescapedSpan(this scoped ref KdlReader reader)
        {
            Debug.Assert(reader.TokenType is KdlTokenType.String or KdlTokenType.PropertyName);
            ReadOnlySpan<byte> span = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan;
            return reader.ValueIsEscaped ? KdlReaderHelper.GetUnescapedSpan(span) : span;
        }

        /// <summary>
        /// Attempts to perform a Read() operation and optionally checks that the full KDL value has been buffered.
        /// The reader will be reset if the operation fails.
        /// </summary>
        /// <param name="reader">The reader to advance.</param>
        /// <param name="requiresReadAhead">If reading a partial payload, read ahead to ensure that the full KDL value has been buffered.</param>
        /// <returns>True if the reader has been buffered with all required data.</returns>
        // AggressiveInlining used since this method is on a hot path and short. The AdvanceWithReadAhead method should not be inlined.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdvanceWithOptionalReadAhead(
            this scoped ref KdlReader reader,
            bool requiresReadAhead
        )
        {
            // No read-ahead necessary if we're at the final block of KDL data.
            bool readAhead = requiresReadAhead && !reader.IsFinalBlock;
            return readAhead ? TryAdvanceWithReadAhead(ref reader) : reader.Read();
        }

        /// <summary>
        /// Attempts to read ahead to the next root-level KDL value, if it exists.
        /// </summary>
        public static bool TryAdvanceToNextRootLevelValueWithOptionalReadAhead(
            this scoped ref KdlReader reader,
            bool requiresReadAhead,
            out bool isAtEndOfStream
        )
        {
            Debug.Assert(reader.CurrentDepth == 0, "should only invoked for top-level values.");

            KdlReader checkpoint = reader;
            if (!reader.Read())
            {
                // If the reader didn't return any tokens and it's the final block,
                // then there are no other KDL values to be read.
                isAtEndOfStream = reader.IsFinalBlock;
                reader = checkpoint;
                return false;
            }

            // We found another KDL value, read ahead accordingly.
            isAtEndOfStream = false;
            if (requiresReadAhead && !reader.IsFinalBlock)
            {
                // Perform full read-ahead to ensure the full KDL value has been buffered.
                reader = checkpoint;
                return TryAdvanceWithReadAhead(ref reader);
            }

            return true;
        }

        private static bool TryAdvanceWithReadAhead(scoped ref KdlReader reader)
        {
            // When we're reading ahead we always have to save the state
            // as we don't know if the next token is a start object or array.
            KdlReader restore = reader;

            if (!reader.Read())
            {
                return false;
            }

            // Perform the actual read-ahead.
            KdlTokenType tokenType = reader.TokenType;
            if (tokenType is KdlTokenType.StartChildrenBlock or KdlTokenType.StartChildrenBlock)
            {
                // Attempt to skip to make sure we have all the data we need.
                bool complete = reader.TrySkipPartial();

                // We need to restore the state in all cases as we need to be positioned back before
                // the current token to either attempt to skip again or to actually read the value.
                reader = restore;

                if (!complete)
                {
                    // Couldn't read to the end of the object, exit out to get more data in the buffer.
                    return false;
                }

                // Success, requeue the reader to the start token.
                reader.ReadWithVerify();
                Debug.Assert(tokenType == reader.TokenType);
            }

            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is between
        /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(uint value, uint lowerBound, uint upperBound) =>
            (value - lowerBound) <= (upperBound - lowerBound);

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is between
        /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(int value, int lowerBound, int upperBound) =>
            (uint)(value - lowerBound) <= (uint)(upperBound - lowerBound);

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is between
        /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(long value, long lowerBound, long upperBound) =>
            (ulong)(value - lowerBound) <= (ulong)(upperBound - lowerBound);

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is between
        /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>, inclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(
            KdlTokenType value,
            KdlTokenType lowerBound,
            KdlTokenType upperBound
        ) => (value - lowerBound) <= (upperBound - lowerBound);

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is in the range [0..9].
        /// Otherwise, returns <see langword="false"/>.
        /// </summary>
        public static bool IsDigit(byte value) => (uint)(value - '0') <= '9' - '0';

        /// <summary>
        /// Perform a Read() with a Debug.Assert verifying the reader did not return false.
        /// This should be called when the Read() return value is not used, such as non-Stream cases where there is only one buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadWithVerify(this ref KdlReader reader)
        {
            bool result = reader.Read();
            Debug.Assert(result);
        }

        /// <summary>
        /// Performs a TrySkip() with a Debug.Assert verifying the reader did not return false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipWithVerify(this ref KdlReader reader)
        {
            bool success = reader.TrySkipPartial(reader.CurrentDepth);
            Debug.Assert(success, "The skipped value should have already been buffered.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySkipPartial(this ref KdlReader reader)
        {
            return reader.TrySkipPartial(reader.CurrentDepth);
        }

        /// <summary>
        /// Calls Encoding.UTF8.GetString that supports netstandard.
        /// </summary>
        /// <param name="bytes">The utf8 bytes to convert.</param>
        /// <returns></returns>
        public static string Utf8GetString(ReadOnlySpan<byte> bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static bool TryLookupUtf8Key<TValue>(
            this Dictionary<string, TValue> dictionary,
            ReadOnlySpan<byte> utf8Key,
            [MaybeNullWhen(false)] out TValue result
        )
        {
            Debug.Assert(
                dictionary.Comparer is IAlternateEqualityComparer<ReadOnlySpan<char>, string>
            );

            Dictionary<string, TValue>.AlternateLookup<ReadOnlySpan<char>> spanLookup =
                dictionary.GetAlternateLookup<ReadOnlySpan<char>>();

            char[]? rentedBuffer = null;

            Span<char> charBuffer =
                utf8Key.Length <= KdlConstants.StackallocCharThreshold
                    ? stackalloc char[KdlConstants.StackallocCharThreshold]
                    : (rentedBuffer = ArrayPool<char>.Shared.Rent(utf8Key.Length));

            int charsWritten = Encoding.UTF8.GetChars(utf8Key, charBuffer);
            Span<char> decodedKey = charBuffer[0..charsWritten];

            bool success = spanLookup.TryGetValue(decodedKey, out result);

            if (rentedBuffer != null)
            {
                decodedKey.Clear();
                ArrayPool<char>.Shared.Return(rentedBuffer);
            }

            return success;
        }

        /// <summary>
        /// Emulates Dictionary(IEnumerable{KeyValuePair}) on netstandard.
        /// </summary>
        public static Dictionary<TKey, TValue> CreateDictionaryFromCollection<TKey, TValue>(
            IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer
        )
            where TKey : notnull
        {
            return new Dictionary<TKey, TValue>(collection: collection, comparer);
        }

        public static bool IsFinite(double value)
        {
            return double.IsFinite(value);
        }

        public static bool IsFinite(float value)
        {
            return float.IsFinite(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateInt32MaxArrayLength(uint length)
        {
            if (length > 0X7FEFFFFF) // prior to .NET 6, max array length for sizeof(T) != 1 (size == 1 is larger)
            {
                ThrowHelper.ThrowOutOfMemoryException(length);
            }
        }

        public static bool HasAllSet(this BitArray bitArray)
        {
            for (int i = 0; i < bitArray.Count; i++)
            {
                if (!bitArray[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a Regex instance for recognizing integer representations of enums.
        /// </summary>
        public static readonly Regex IntegerRegex = CreateIntegerRegex();
        private const string IntegerRegexPattern = @"^\s*(?:\+|\-)?[0-9]+\s*$";
        private const int IntegerRegexTimeoutMs = 200;

        [GeneratedRegex(
            IntegerRegexPattern,
            RegexOptions.None,
            matchTimeoutMilliseconds: IntegerRegexTimeoutMs
        )]
        private static partial Regex CreateIntegerRegex();

        /// <summary>
        /// Compares two valid UTF-8 encoded KDL numbers for decimal equality.
        /// </summary>
        public static bool AreEqualKdlNumbers(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            Debug.Assert(left.Length > 0 && right.Length > 0);

            ParseNumber(
                left,
                out bool leftIsNegative,
                out ReadOnlySpan<byte> leftIntegral,
                out ReadOnlySpan<byte> leftFractional,
                out int leftExponent
            );

            ParseNumber(
                right,
                out bool rightIsNegative,
                out ReadOnlySpan<byte> rightIntegral,
                out ReadOnlySpan<byte> rightFractional,
                out int rightExponent
            );

            int nDigits;
            if (
                leftIsNegative != rightIsNegative
                || leftExponent != rightExponent
                || (nDigits = leftIntegral.Length + leftFractional.Length)
                    != rightIntegral.Length + rightFractional.Length
            )
            {
                return false;
            }

            // Need to check that the concatenated integral and fractional parts are equal;
            // break each representation into three parts such that their lengths exactly match.
            ReadOnlySpan<byte> leftFirst;
            ReadOnlySpan<byte> leftMiddle;
            ReadOnlySpan<byte> leftLast;

            ReadOnlySpan<byte> rightFirst;
            ReadOnlySpan<byte> rightMiddle;
            ReadOnlySpan<byte> rightLast;

            int diff = leftIntegral.Length - rightIntegral.Length;
            switch (diff)
            {
                case < 0:
                    leftFirst = leftIntegral;
                    leftMiddle = leftFractional[..-diff];
                    leftLast = leftFractional[-diff..];
                    int rightOffset = rightIntegral.Length + diff;
                    rightFirst = rightIntegral[..rightOffset];
                    rightMiddle = rightIntegral[rightOffset..];
                    rightLast = rightFractional;
                    break;

                case 0:
                    leftFirst = leftIntegral;
                    leftMiddle = default;
                    leftLast = leftFractional;
                    rightFirst = rightIntegral;
                    rightMiddle = default;
                    rightLast = rightFractional;
                    break;

                case > 0:
                    int leftOffset = leftIntegral.Length - diff;
                    leftFirst = leftIntegral[..leftOffset];
                    leftMiddle = leftIntegral[leftOffset..];
                    leftLast = leftFractional;
                    rightFirst = rightIntegral;
                    rightMiddle = rightFractional[..diff];
                    rightLast = rightFractional[diff..];
                    break;
            }

            Debug.Assert(leftFirst.Length == rightFirst.Length);
            Debug.Assert(leftMiddle.Length == rightMiddle.Length);
            Debug.Assert(leftLast.Length == rightLast.Length);
            return leftFirst.SequenceEqual(rightFirst)
                && leftMiddle.SequenceEqual(rightMiddle)
                && leftLast.SequenceEqual(rightLast);

            static void ParseNumber(
                ReadOnlySpan<byte> span,
                out bool isNegative,
                out ReadOnlySpan<byte> integral,
                out ReadOnlySpan<byte> fractional,
                out int exponent
            )
            {
                // Parses a KDL number into its integral, fractional, and exponent parts.
                // The returned components use a normal-form decimal representation:
                //
                //   Number := sign * <integral + fractional> * 10^exponent
                //
                // where integral and fractional are sequences of digits whose concatenation
                // represents the significand of the number without leading or trailing zeros.
                // Two such normal-form numbers are treated as equal if and only if they have
                // equal signs, significands, and exponents.

                bool neg;
                ReadOnlySpan<byte> intg;
                ReadOnlySpan<byte> frac;
                int exp;

                Debug.Assert(span.Length > 0);

                if (span[0] == '-')
                {
                    neg = true;
                    span = span[1..];
                }
                else
                {
                    Debug.Assert(
                        char.IsDigit((char)span[0]),
                        "leading plus not allowed in valid KDL numbers."
                    );
                    neg = false;
                }

                int i = span.IndexOfAny((byte)'.', (byte)'e', (byte)'E');
                if (i < 0)
                {
                    intg = span;
                    frac = default;
                    exp = 0;
                    goto Normalize;
                }

                intg = span[..i];

                if (span[i] == '.')
                {
                    span = span[(i + 1)..];
                    i = span.IndexOfAny((byte)'e', (byte)'E');
                    if (i < 0)
                    {
                        frac = span;
                        exp = 0;
                        goto Normalize;
                    }

                    frac = span[..i];
                }
                else
                {
                    frac = default;
                }

                Debug.Assert(span[i] is (byte)'e' or (byte)'E');
                if (!Utf8Parser.TryParse(span[(i + 1)..], out exp, out _))
                {
                    Debug.Assert(span.Length >= 10);
                    ThrowHelper.ThrowArgumentOutOfRangeException_KdlNumberExponentTooLarge(
                        nameof(exponent)
                    );
                }

                Normalize: // Calculates the normal form of the number.

                if (IndexOfFirstTrailingZero(frac) is >= 0 and int iz)
                {
                    // Trim trailing zeros from the fractional part.
                    // e.g. 3.1400 -> 3.14
                    frac = frac[..iz];
                }

                if (intg[0] == '0')
                {
                    Debug.Assert(intg.Length == 1, "Leading zeros not permitted in KDL numbers.");

                    if (IndexOfLastLeadingZero(frac) is >= 0 and int lz)
                    {
                        // Trim leading zeros from the fractional part
                        // and update the exponent accordingly.
                        // e.g. 0.000123 -> 0.123e-3
                        frac = frac[(lz + 1)..];
                        exp -= lz + 1;
                    }

                    // Normalize "0" to the empty span.
                    intg = default;
                }

                if (frac.IsEmpty && IndexOfFirstTrailingZero(intg) is >= 0 and int fz)
                {
                    // There is no fractional part, trim trailing zeros from
                    // the integral part and increase the exponent accordingly.
                    // e.g. 1000 -> 1e3
                    exp += intg.Length - fz;
                    intg = intg[..fz];
                }

                // Normalize the exponent by subtracting the length of the fractional part.
                // e.g. 3.14 -> 314e-2
                exp -= frac.Length;

                if (intg.IsEmpty && frac.IsEmpty)
                {
                    // Normalize zero representations.
                    neg = false;
                    exp = 0;
                }

                // Copy to out parameters.
                isNegative = neg;
                integral = intg;
                fractional = frac;
                exponent = exp;

                static int IndexOfLastLeadingZero(ReadOnlySpan<byte> span)
                {
                    int firstNonZero = span.IndexOfAnyExcept((byte)'0');
                    return firstNonZero < 0 ? span.Length - 1 : firstNonZero - 1;
                }

                static int IndexOfFirstTrailingZero(ReadOnlySpan<byte> span)
                {
                    int lastNonZero = span.LastIndexOfAnyExcept((byte)'0');
                    return lastNonZero == span.Length - 1 ? -1 : lastNonZero + 1;
                }
            }
        }
    }
}
