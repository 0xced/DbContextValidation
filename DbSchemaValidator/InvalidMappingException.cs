using System;

namespace DbSchemaValidator
{
    /// <inheritdoc />
    /// <summary>
    /// The exception thrown when the context does not match the actual database schema. Has a <exception cref="System.Data.Common.DbException">DbException</exception> inner exception.
    /// </summary>
    public class InvalidMappingException : Exception
    {
        internal InvalidMappingException(Type entityType, string message, Exception innerException) : base(message, innerException)
        {
            EntityType = entityType;
        }

        /// <summary>
        /// The type of the entity which does not match the actual database schema.
        /// </summary>
        public Type EntityType { get; }
    }
}