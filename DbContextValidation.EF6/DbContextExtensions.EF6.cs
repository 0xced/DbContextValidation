﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace DbContextValidation.EF6
{
    /// <summary>
    /// Extends the <see cref="DbContext"/> class by providing a method for getting a simplified model of the tables.
    /// </summary>
    public static class DbContextExtensions
    {
        private static readonly ConcurrentDictionary<DbContext, IReadOnlyCollection<Table>> ModelTablesCache = new ConcurrentDictionary<DbContext, IReadOnlyCollection<Table>>();

        /// <param name="context">The context</param>
        /// <returns>A collection of the database tables defined in the given context.</returns>
        public static IReadOnlyCollection<Table> GetModelTables(this DbContext context)
        {
            return ModelTablesCache.GetOrAdd(context, ctx =>
            {
                var tables = new List<Table>();
                var workspace = ((IObjectContextAdapter)ctx).ObjectContext.MetadataWorkspace;
                var entitySets = workspace.GetItems<EntityContainer>(DataSpace.CSpace).Single().EntitySets;
                var entitySetMappings = workspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings.ToList();
                var entityTypes = workspace.GetItems<EntityType>(DataSpace.CSpace);
                foreach (var entityType in entityTypes)
                {
                    var entitySet = entitySets.Single(s => s.ElementType.Name == entityType.Name);
                    var entitySetMapping = entitySetMappings.Single(s => s.EntitySet == entitySet);
                    var fragmentMapping = entitySetMapping.EntityTypeMappings.Single().Fragments.Single();
                    var schema = fragmentMapping.StoreEntitySet.Schema;
                    var tableName = fragmentMapping.StoreEntitySet.Table;
                    var columnNames = fragmentMapping.PropertyMappings.OfType<ScalarPropertyMapping>().Select(e => e.Column.Name);
                    var table = new Table(schema, tableName, columnNames.ToList());
                    tables.Add(table);
                }
                return tables;
            });
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.Connection;
        }
    }
}