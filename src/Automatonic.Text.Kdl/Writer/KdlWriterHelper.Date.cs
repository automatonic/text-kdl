using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace Automatonic.Text.Kdl
{
    internal static partial class KdlWriterHelper
    {
        private static readonly StandardFormat s_dateTimeStandardFormat = new('O');

        public static void WriteDateTimeTrimmed(
            Span<byte> buffer,
            DateTime value,
            out int bytesWritten
        )
        {
            Span<byte> tempSpan = stackalloc byte[KdlConstants.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(
                value,
                tempSpan,
                out bytesWritten,
                s_dateTimeStandardFormat
            );
            Debug.Assert(result);
            TrimDateTimeOffset(tempSpan[..bytesWritten], out bytesWritten);
            tempSpan[..bytesWritten].CopyTo(buffer);
        }

        public static void WriteDateTimeOffsetTrimmed(
            Span<byte> buffer,
            DateTimeOffset value,
            out int bytesWritten
        )
        {
            Span<byte> tempSpan = stackalloc byte[KdlConstants.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(
                value,
                tempSpan,
                out bytesWritten,
                s_dateTimeStandardFormat
            );
            Debug.Assert(result);
            TrimDateTimeOffset(tempSpan[..bytesWritten], out bytesWritten);
            tempSpan[..bytesWritten].CopyTo(buffer);
        }

        //
        // Trims roundtrippable DateTime(Offset) input.
        // If the milliseconds part of the date is zero, we omit the fraction part of the date,
        // else we write the fraction up to 7 decimal places with no trailing zeros. i.e. the output format is
        // YYYY-MM-DDThh:mm:ss[.s]TZD where TZD = Z or +-hh:mm.
        // e.g.
        //   ---------------------------------
        //   2017-06-12T05:30:45.768-07:00
        //   2017-06-12T05:30:45.00768Z           (Z is short for "+00:00" but also distinguishes DateTimeKind.Utc from DateTimeKind.Local)
        //   2017-06-12T05:30:45                  (interpreted as local time wrt to current time zone)
        public static void TrimDateTimeOffset(Span<byte> buffer, out int bytesWritten)
        {
            const int maxDateTimeLength = KdlConstants.MaximumFormatDateTimeLength;

            // Assert buffer is the right length for:
            // YYYY-MM-DDThh:mm:ss.fffffff (KdlConstants.MaximumFormatDateTimeLength)
            // YYYY-MM-DDThh:mm:ss.fffffffZ (KdlConstants.MaximumFormatDateTimeLength + 1)
            // YYYY-MM-DDThh:mm:ss.fffffff(+|-)hh:mm (KdlConstants.MaximumFormatDateTimeOffsetLength)
            Debug.Assert(
                buffer.Length
                    is maxDateTimeLength
                        or (maxDateTimeLength + 1)
                        or KdlConstants.MaximumFormatDateTimeOffsetLength
            );

            // Find the last significant digit.
            int curIndex;
            if (buffer[maxDateTimeLength - 1] == '0')
            {
                if (buffer[maxDateTimeLength - 2] == '0')
                {
                    if (buffer[maxDateTimeLength - 3] == '0')
                    {
                        if (buffer[maxDateTimeLength - 4] == '0')
                        {
                            if (buffer[maxDateTimeLength - 5] == '0')
                            {
                                if (buffer[maxDateTimeLength - 6] == '0')
                                {
                                    if (buffer[maxDateTimeLength - 7] == '0')
                                    {
                                        // All decimal places are 0 so we can delete the decimal point too.
                                        curIndex = maxDateTimeLength - 7 - 1;
                                    }
                                    else
                                    {
                                        curIndex = maxDateTimeLength - 6;
                                    }
                                }
                                else
                                {
                                    curIndex = maxDateTimeLength - 5;
                                }
                            }
                            else
                            {
                                curIndex = maxDateTimeLength - 4;
                            }
                        }
                        else
                        {
                            curIndex = maxDateTimeLength - 3;
                        }
                    }
                    else
                    {
                        curIndex = maxDateTimeLength - 2;
                    }
                }
                else
                {
                    curIndex = maxDateTimeLength - 1;
                }
            }
            else
            {
                // There is nothing to trim.
                bytesWritten = buffer.Length;
                return;
            }

            // We are either trimming a DateTimeOffset, or a DateTime with
            // DateTimeKind.Local or DateTimeKind.Utc
            if (buffer.Length == maxDateTimeLength)
            {
                // There is no offset to copy.
                bytesWritten = curIndex;
            }
            else if (buffer.Length == KdlConstants.MaximumFormatDateTimeOffsetLength)
            {
                // We have a non-UTC offset (+|-)hh:mm that are 6 characters to copy.
                buffer[curIndex] = buffer[maxDateTimeLength];
                buffer[curIndex + 1] = buffer[maxDateTimeLength + 1];
                buffer[curIndex + 2] = buffer[maxDateTimeLength + 2];
                buffer[curIndex + 3] = buffer[maxDateTimeLength + 3];
                buffer[curIndex + 4] = buffer[maxDateTimeLength + 4];
                buffer[curIndex + 5] = buffer[maxDateTimeLength + 5];
                bytesWritten = curIndex + 6;
            }
            else
            {
                // There is a single 'Z'. Just write it at the current index.
                Debug.Assert(buffer[maxDateTimeLength] == 'Z');

                buffer[curIndex] = (byte)'Z';
                bytesWritten = curIndex + 1;
            }
        }
    }
}
