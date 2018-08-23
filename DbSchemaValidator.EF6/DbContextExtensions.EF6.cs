using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace DbSchemaValidator.EF6
{
    public static partial class DbContextExtensions
    {
        private static IDictionary<(string schema, string tableName), IReadOnlyCollection<string>> GetDbModel(this DbContext context)
        {
            var model = new Dictionary<(string schema, string tableName), IReadOnlyCollection<string>>();
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
                var columnNames = fragmentMapping.PropertyMappings.OfType<ScalarPropertyMapping>().Select(e => e.Column.Name);
                model.Add((schema: schema, tableName: tableName), columnNames.ToList());
            }
            return model;
        }

        private static DbConnection GetDbConnection(this DbContext context)
        {
            return context.Database.Connection;
        }
    }
}