using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace DbSchemaValidator
{
    public class InvalidMappingException : Exception
    {
        public InvalidMappingException(Type entityType, string query, string message, Exception innerException) : base(message, innerException)
        {
            EntityType = entityType;
            Query = query;
        }
        
        public Type EntityType { get; }
        public string Query { get; }
    }
    
    public static class DbContextExtensions
    {
        public static async Task ValidateSchema(this DbContext context)
        {
            await context.Database.Connection.OpenAsync();
            var contextAssembly = context.GetType().Assembly;
            foreach (var type in ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace))
            {
                var entityType = contextAssembly.GetType(type.FullName, throwOnError: true);
                var query = ((IQueryable<object>)context.Set(entityType)).Take(1);
                try
                {
                    await query.ToListAsync();
                }
                catch (Exception exception)
                {
                    var innerException = (exception as EntityCommandExecutionException)?.InnerException ?? exception;
                    var message = $"Invalid mapping for DbSet<{entityType?.FullName}> in {context.GetType().FullName}. See the inner exception for details.";
                    throw new InvalidMappingException(entityType, query.ToString(), message, innerException);
                }
            }
        }
    }
}