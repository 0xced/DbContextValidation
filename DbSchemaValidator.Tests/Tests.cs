using System.Linq;
using System.Threading.Tasks;
#if NETFRAMEWORK
using DbSchemaValidator.EF6;
using DbSchemaValidator.Tests.EF6;
#else
using DbSchemaValidator.EFCore;
using DbSchemaValidator.Tests.EFCore;
#endif
using Xunit;

namespace DbSchemaValidator.Tests
{
    public class Tests
    {
        static Tests()
        {
#if NETFRAMEWORK
            // Disable migrations
            System.Data.Entity.Database.SetInitializer<ValidContext>(null);
            System.Data.Entity.Database.SetInitializer<MisspelledTableContext>(null);
            System.Data.Entity.Database.SetInitializer<MisspelledColumnContext>(null);
#endif
        }

        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                var invalidMappings = await context.ValidateSchema();
                Assert.Empty(invalidMappings);
            }
        }
        
        [Fact]
        public async Task MisspelledTable()
        {
            using (var context = new MisspelledTableContext())
            {
                var invalidMappings = await context.ValidateSchema();
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("Kustomers", invalidMapping.TableName);
                Assert.Null(invalidMapping.MissingColumns);
            }
        }
        
        [Fact]
        public async Task MisspelledColumn()
        {
            using (var context = new MisspelledColumnContext())
            {
                var invalidMappings = await context.ValidateSchema();
                Assert.Single(invalidMappings);
                var invalidMapping = invalidMappings.Single(); 
                Assert.Equal("Orders", invalidMapping.TableName);
                Assert.Single(invalidMapping.MissingColumns);
                Assert.Equal("OrderFate", invalidMapping.MissingColumns.Single());
            }
        }
    }
}