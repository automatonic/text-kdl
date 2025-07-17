namespace Automatonic.Text.Kdl.Serialization
{
    public partial class KdlConverter<T>
    {
        internal bool ReadCore(
            ref KdlReader reader,
            out T? value,
            KdlSerializerOptions options,
            ref ReadStack state
        )
        {
            try
            {
                if (!state.IsContinuation && !IsRootLevelMultiContentStreamingConverter)
                {
                    // This is first call to the converter -- advance the reader
                    // to the first KDL token and perform a read-ahead if necessary.
                    if (!reader.TryAdvanceWithOptionalReadAhead(RequiresReadAhead))
                    {
                        if (state.SupportContinuation && state.Current.ReturnValue is object result)
                        {
                            // This branch is hit when deserialization has completed in an earlier call
                            // but we're still processing trailing whitespace. Return the result stored in the state machine.
                            value = (T)result;
                        }
                        else
                        {
                            value = default;
                        }

                        return reader.IsFinalBlock;
                    }
                }

                bool success = TryRead(
                    ref reader,
                    state.Current.KdlTypeInfo.Type,
                    options,
                    ref state,
                    out value,
                    out _
                );
                return success;
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case KdlReaderException kdlReaderEx:
                        ThrowHelper.ReThrowWithPath(ref state, kdlReaderEx);
                        break;

                    case FormatException
                        when ex.Source == ThrowHelper.ExceptionSourceValueToRethrowAsKdlException:
                        ThrowHelper.ReThrowWithPath(ref state, reader, ex);
                        break;

                    case InvalidOperationException
                        when ex.Source == ThrowHelper.ExceptionSourceValueToRethrowAsKdlException:
                        ThrowHelper.ReThrowWithPath(ref state, reader, ex);
                        break;

                    case KdlException kdlEx when kdlEx.Path is null:
                        // KdlExceptions where the Path property is already set
                        // typically originate from nested calls to KdlSerializer;
                        // treat these cases as any other exception type and do not
                        // overwrite any exception information.
                        ThrowHelper.AddKdlExceptionInformation(ref state, reader, kdlEx);
                        break;

                    case NotSupportedException when !ex.Message.Contains(" Path: "):
                        // If the message already contains Path, just re-throw. This could occur in serializer re-entry cases.
                        // To get proper Path semantics in re-entry cases, APIs that take 'state' need to be used.
                        ThrowHelper.ThrowNotSupportedException(ref state, reader, ex);
                        break;
                }

                throw;
            }
        }
    }
}
