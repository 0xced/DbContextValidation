using System;
using System.Threading.Tasks;
using DbSchemaValidator.EFCore.Tests.DB;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DbSchemaValidator.EFCore.Tests
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
        [InlineData(typeof(MisspelledTableContext), typeof(Customer), "no such table: Kustomers")]
        [InlineData(typeof(MisspelledColumnContext), typeof(Order), "no such column: o.OrderFate")]
        public async Task Misspelling(Type contextType, Type invalidEntityType, string errorMessage)
        {
            using (var context = (DbContext)Activator.CreateInstance(contextType))
            {
                var exception = await Assert.ThrowsAsync<InvalidMappingException>(() => context.ValidateSchema());
                Assert.Equal(invalidEntityType, exception.EntityType);
                var innerException = exception.InnerException;
                Assert.NotNull(innerException);
                Assert.Contains(errorMessage, innerException.Message);
            }
        }
    }
}