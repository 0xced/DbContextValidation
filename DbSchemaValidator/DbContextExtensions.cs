﻿using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace DbSchemaValidator
{
    public class InvalidMappingException : Exception
    {
        public InvalidMappingException(Type entityType, string sqlQuery, string message, Exception innerException) : base(message, innerException)
        {
            EntityType = entityType;
            SqlQuery = sqlQuery;
        }

        public Type EntityType { get; }
        public string SqlQuery { get; }
    }

    public static class DbContextExtensions
    {
        public static async Task ValidateSchema(this DbContext context)
        {
            var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var itemCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);
            foreach (var entityType in workspace.GetItems<EntityType>(DataSpace.CSpace))
            {
                var type = itemCollection.GetClrType(workspace.GetObjectSpaceType(entityType));
                var validationQuery = ((IQueryable<object>)context.Set(type)).Take(1);
                var sqlQuery = validationQuery.ToString();
                try
                {
                    await context.Database.SqlQuery<object>(sqlQuery).ToListAsync();
                }
                catch (DbException exception)
                {
                    var message = $"The mapping for {type.FullName} is invalid. See the inner exception for details.";
                    throw new InvalidMappingException(type, sqlQuery, message, exception);
                }
            }
        }
    }
}