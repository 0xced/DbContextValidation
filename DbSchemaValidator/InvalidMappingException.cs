using System;

namespace DbSchemaValidator
{
    public class InvalidMappingException : Exception
    {
        internal InvalidMappingException(Type entityType, string message, Exception innerException) : base(message, innerException)
        {
            EntityType = entityType;
        }

        public Type EntityType { get; }
    }
}