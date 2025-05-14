namespace Automatonic.Text.Kdl.Serialization
{
    public partial class KdlConverter<T>
    {
        internal bool WriteCore(
            KdlWriter writer,
            in T? value,
            KdlSerializerOptions options,
            ref WriteStack state)
        {
            try
            {
                return TryWrite(writer, value, options, ref state);
            }
            catch (Exception ex)
            {
                if (!state.SupportAsync)
                {
                    // Async serializers should dispose sync and
                    // async disposables from the async root method.
                    state.DisposePendingDisposablesOnException();
                }

                switch (ex)
                {
                    case InvalidOperationException when ex.Source == ThrowHelper.ExceptionSourceValueToRethrowAsKdlException:
                        ThrowHelper.ReThrowWithPath(ref state, ex);
                        break;

                    case KdlException { Path: null } kdlException:
                        // KdlExceptions where the Path property is already set
                        // typically originate from nested calls to KdlSerializer;
                        // treat these cases as any other exception type and do not
                        // overwrite any exception information.
                        ThrowHelper.AddKdlExceptionInformation(ref state, kdlException);
                        break;

                    case NotSupportedException when !ex.Message.Contains(" Path: "):
                        // If the message already contains Path, just re-throw. This could occur in serializer re-entry cases.
                        // To get proper Path semantics in re-entry cases, APIs that take 'state' need to be used.
                        ThrowHelper.ThrowNotSupportedException(ref state, ex);
                        break;
                }

                throw;
            }
        }
    }
}
