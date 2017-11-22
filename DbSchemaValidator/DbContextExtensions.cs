using System;
using System.Collections.Generic;
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

            var set = typeof(DbContext).GetMethod(typeof(DbSet<>), nameof(DbContext.Set));
            var take = typeof(Queryable).GetMethod(typeof(IQueryable<>), nameof(Queryable.Take), typeof(IQueryable<>), typeof(int));
            var toListAsync = typeof(QueryableExtensions).GetMethod(typeof(Task<List<object>>), nameof(QueryableExtensions.ToListAsync), typeof(IQueryable));
            
            var contextAssembly = context.GetType().Assembly;
            foreach (var type in ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace))
            {
                var entityType = contextAssembly.GetType(type.FullName, throwOnError: true);
                var dbSet = set.MakeGenericMethod(entityType).Invoke(context, new object[] {});
                var query = take.MakeGenericMethod(entityType).Invoke(null, new[] {dbSet, 1});
                try
                {
                    // runtime generic version of `await context.Set<entityType>().Take(1).ToListAsync()`
                    await (Task)toListAsync.Invoke(null, new[] {query});
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