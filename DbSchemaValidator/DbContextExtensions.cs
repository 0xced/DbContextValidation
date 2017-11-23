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
            var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var itemCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);
            foreach (var entityType in workspace.GetItems<EntityType>(DataSpace.CSpace))
            {
                var type = itemCollection.GetClrType(workspace.GetObjectSpaceType(entityType));
                var query = ((IQueryable<object>)context.Set(type)).Take(1);
                try
                {
                    await query.ToListAsync();
                }
                catch (Exception exception)
                {
                    var innerException = (exception as EntityCommandExecutionException)?.InnerException ?? exception;
                    var message = $"Invalid mapping for DbSet<{type?.FullName}> in {context.GetType().FullName}. See the inner exception for details.";
                    throw new InvalidMappingException(type, query.ToString(), message, innerException);
                }
            }
        }
    }
}