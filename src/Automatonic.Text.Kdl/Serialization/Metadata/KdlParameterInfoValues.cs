﻿using System.ComponentModel;

namespace Automatonic.Text.Kdl.Serialization.Metadata
{
    /// <summary>
    /// Provides information about a constructor parameter required for KDL deserialization.
    /// </summary>
    /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class KdlParameterInfoValues
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public string Name { get; init; } = null!;

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public Type ParameterType { get; init; } = null!;

        /// <summary>
        /// The zero-based position of the parameter in the formal parameter list.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public int Position { get; init; }

        /// <summary>
        /// Whether a default value was specified for the parameter.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public bool HasDefaultValue { get; init; }

        /// <summary>
        /// The default value of the parameter.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public object? DefaultValue { get; init; }

        /// <summary>
        /// Whether the parameter allows <see langword="null"/> values.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public bool IsNullable { get; init; }

        /// <summary>
        /// Whether the parameter represents a required or init-only member initializer.
        /// </summary>
        /// <remarks>This API is for use by the output of the Automatonic.Text.Kdl source generator and should not be called directly.</remarks>
        public bool IsMemberInitializer { get; init; }
    }
}
