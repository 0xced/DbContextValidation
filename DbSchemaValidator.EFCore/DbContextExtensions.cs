using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.EFCore
{
    public class InvalidMappingException : Exception
    {
        public InvalidMappingException(Type entityType, string message, Exception innerException) : base(message, innerException)
        {
            EntityType = entityType;
        }

        public Type EntityType { get; }
    }

    public static class DbContextExtensions
    {
        private static readonly Lazy<MethodInfo> Set = new Lazy<MethodInfo>(() => typeof(DbContext).GetMethod(typeof(DbSet<>), nameof(DbContext.Set)));
        private static readonly Lazy<MethodInfo> Take = new Lazy<MethodInfo>(() => typeof(Queryable).GetMethod(typeof(IQueryable<>), nameof(Queryable.Take), typeof(IQueryable<>), typeof(int)));
        private static readonly Lazy<MethodInfo> ToListAsync = new Lazy<MethodInfo>(() => typeof(EntityFrameworkQueryableExtensions).GetMethod(typeof(Task<List<object>>), nameof(EntityFrameworkQueryableExtensions.ToListAsync), typeof(IQueryable<>), typeof(CancellationToken)));
        
        public static async Task ValidateSchema(this DbContext context)
        {
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var type = entityType.ClrType;
                var dbSet = Set.Value.MakeGenericMethod(type).Invoke(context, new object[] {});
                var query = Take.Value.MakeGenericMethod(type).Invoke(null, new[] {dbSet, 1});
                try
                {
                    // runtime generic version of `await context.Set<type>().Take(1).ToListAsync()`
                    await (Task)ToListAsync.Value.MakeGenericMethod(type).Invoke(null, new[] {query, default(CancellationToken)});
                }
                catch (DbException exception)
                {
                    var message = $"The mapping for {type.FullName} is invalid. See the inner exception for details.";
                    throw new InvalidMappingException(type, message, exception);
                }
            }
        }
    }
}