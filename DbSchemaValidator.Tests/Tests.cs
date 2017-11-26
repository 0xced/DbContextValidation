using System;
using System.Data.Entity;
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

        [Theory]
        [InlineData(typeof(MisspelledTableContext), typeof(Customer), "Kustomers")]
        [InlineData(typeof(MisspelledColumnContext), typeof(Order), "OrderFate")]
        public async Task Misspelling(Type contextType, Type invalidEntityType, string misspelledConfiguration)
        {
            using (var context = (DbContext)Activator.CreateInstance(contextType))
            {
                var exception = await Assert.ThrowsAsync<InvalidMappingException>(() => context.ValidateSchema());
                Assert.Equal(invalidEntityType, exception.EntityType);
                Assert.Contains(misspelledConfiguration, exception.SqlQuery);
                var innerException = exception.InnerException;
                Assert.NotNull(innerException);
                Assert.Contains(misspelledConfiguration, innerException.Message);
            }
        }
    }
}