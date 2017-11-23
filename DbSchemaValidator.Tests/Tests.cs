using System.Threading.Tasks;
using DbSchemaValidator.Tests.DB;
using Xunit;

namespace DbSchemaValidator.Tests
{
    public class Tests
    {
        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                await context.ValidateSchema();
            }
        }
        
        [Fact]
        public async Task MisspelledTable()
        {
            using (var context = new MisspelledTableContext())
            {
                var exception = await Assert.ThrowsAsync<InvalidMappingException>(() => context.ValidateSchema());
                Assert.Equal(typeof(Customer), exception.EntityType);
                Assert.Contains("Kustomers", exception.Query);
                var innerException = exception.InnerException;
                Assert.NotNull(innerException);
                Assert.Contains("Kustomers", innerException.Message);
            }
        }
        
        [Fact]
        public async Task MisspelledColumn()
        {
            using (var context = new MisspelledColumnContext())
            {
                var exception = await Assert.ThrowsAsync<InvalidMappingException>(() => context.ValidateSchema());
                Assert.Equal(typeof(Order), exception.EntityType);
                Assert.Contains("OrderFate", exception.Query);
                var innerException = exception.InnerException;
                Assert.NotNull(innerException);
                Assert.Contains("OrderFate", innerException.Message);
            }
        }
    }
}