using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automatonic.Text.Kdl.RandomAccess;

namespace Automatonic.Text.Kdl
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentException_NodeValueNotAllowed(string paramName)
        {
            throw new ArgumentException(SR.NodeValueNotAllowed, paramName);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_DuplicateKey(
            string paramName,
            object? propertyName
        )
        {
            throw new ArgumentException(Format(SR.NodeDuplicateKey, propertyName), paramName);
        }

        [DoesNotReturn]
        public static void ThrowKeyNotFoundException()
        {
            throw new KeyNotFoundException();
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeAlreadyHasParent()
        {
            throw new InvalidOperationException(SR.NodeAlreadyHasParent);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeCycleDetected()
        {
            throw new InvalidOperationException(SR.NodeCycleDetected);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeElementCannotBeObjectOrArray()
        {
            throw new InvalidOperationException(SR.NodeElementCannotBeObjectOrArray);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_CollectionIsReadOnly()
        {
            throw GetNotSupportedException_CollectionIsReadOnly();
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ElementWrongType(
            params ReadOnlySpan<string> supportedTypeNames
        )
        {
            Debug.Assert(supportedTypeNames.Length > 0);
            string concatenatedNames =
                supportedTypeNames.Length == 1
                    ? supportedTypeNames[0]
                    : string.Join(", ", supportedTypeNames.ToArray());
            throw new InvalidOperationException(Format(SR.NodeWrongType, concatenatedNames));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeParentWrongType(string typeName)
        {
            throw new InvalidOperationException(Format(SR.NodeParentWrongType, typeName));
        }

        public static NotSupportedException GetNotSupportedException_CollectionIsReadOnly()
        {
            return new NotSupportedException(SR.CollectionIsReadOnly);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeUnableToConvert(
            Type sourceType,
            Type destinationType
        )
        {
            throw new InvalidOperationException(
                Format(SR.NodeUnableToConvert, sourceType, destinationType)
            );
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NodeUnableToConvertElement(
            KdlValueKind valueKind,
            Type destinationType
        )
        {
            throw new InvalidOperationException(
                Format(SR.NodeUnableToConvertElement, valueKind, destinationType)
            );
        }
    }
}
