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
        /// <param name="context">The context</param>
        /// <returns>An enumerable collection of the database tables defined in the given context.</returns>
        public static IEnumerable<Table> GetModelTables(this DbContext context)
        {
            var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
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
                var columns = fragmentMapping.PropertyMappings.OfType<ScalarPropertyMapping>().Select(e => new ModelColumn(e.Column));
                var table = new Table(schema, tableName, columns.ToList());
                yield return table;
            }
        }

        internal static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.Connection;
        }
    }
}