using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
#if NETFRAMEWORK
using DbContextValidation.EF6;
using System.Data.Entity;
#else
using DbContextValidation.EFCore;
using Microsoft.EntityFrameworkCore;
#endif
using Xunit;

namespace DbContextValidation.Tests
{
    public enum Provider
    {
        MySQL,
        Npgsql,
        SQLite,
        SqlServer,
    }
    
    public class Tests : IClassFixture<DockerFixture>
    {
        private readonly DbContextValidator _defaultValidator;
        private readonly DbContextValidator _caseInsensitiveValidator;
        private readonly DbContextValidator _caseSensitiveValidator;

        static Tests()
        {
#if NETFRAMEWORK
            // Disable migrations
            Database.SetInitializer<ValidContext>(null);
            Database.SetInitializer<ContextWithExplicitSchema>(null);
            Database.SetInitializer<ContextWithUnknownSchema>(null);
            Database.SetInitializer<ContextWithMisspelledCustomersTable>(null);
            Database.SetInitializer<ContextWithMisspelledOrderDateColumn>(null);
            Database.SetInitializer<ContextWithMixedCaseColumns>(null);
#endif
        }

        public Tests()
        {
            _defaultValidator = new DbContextValidator();
            _caseInsensitiveValidator = new DbContextValidator(StringComparer.InvariantCultureIgnoreCase);
            _caseSensitiveValidator = new DbContextValidator(StringComparer.InvariantCulture);
        }

        [Fact]
        public async Task ValidMapping()
        {
            using (var context = new ValidContext())
            {
                var invalidMappings = await _defaultValidator.ValidateContextAsync(context);
                invalidMappings.Should().BeEmpty();
                // ReSharper disable AccessToDisposedClosure
                Func<Task> customersTask = async () => { await context.Customers.ToListAsync(); };
                Func<Task> ordersTask = async () => { await context.Orders.ToListAsync(); };
                // ReSharper restore AccessToDisposedClosure
                customersTask.Should().NotThrow();
                ordersTask.Should().NotThrow();
            }
        }
        
        [Fact]
        public async Task ValidMappingWithExplicitSchema()
        {
            using (var context = new ContextWithExplicitSchema())
            {
                var invalidMappings = await _defaultValidator.ValidateContextAsync(context);
                invalidMappings.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task ValidMappingWithProgress()
        {
            var fractions = new List<float>();
            var progress = new Progress<float>(fractions.Add);
            
            using (var context = new ValidContext())
            {
                await _defaultValidator.ValidateContextAsync(context, progress: progress);
                await Task.Yield();
                fractions.Should().HaveCount(2);
                fractions.Should().Contain(new []{0.5f, 1.0f});
            }
        }
        
        [Fact]
        public void ValidMappingWithCancellation()
        {
            using (var context = new ValidContext())
            {
                var validationTask = _defaultValidator.ValidateContextAsync(context, cancellationToken: new CancellationToken(true));
                validationTask.Status.Should().Be(TaskStatus.Canceled);
            }
        }

        [Fact]
        public async Task UnknownSchema()
        {
            using (var context = new ContextWithUnknownSchema())
            {
                var invalidMappings = await _defaultValidator.ValidateContextAsync(context);
                invalidMappings.Should().HaveCount(2);
                invalidMappings.Should().OnlyContain(e => e.Schema == "unknown");
                invalidMappings.Select(e => e.TableName).Should().Contain("tCustomers", "tOrders");
            }
        }
        
        [Fact]
        public async Task MisspelledCustomersTable()
        {
            using (var context = new ContextWithMisspelledCustomersTable())
            {
                var invalidMappings = await _defaultValidator.ValidateContextAsync(context);
                var invalidMapping = invalidMappings.Should().ContainSingle().Subject;
                invalidMapping.TableName.Should().Be("Customers");
                invalidMapping.MissingColumns.Should().BeNull();
                invalidMapping.MissingTableException.Should().NotBeNull();
            }
        }
        
        [Fact]
        public async Task MisspelledOrderDateColumn()
        {
            using (var context = new ContextWithMisspelledOrderDateColumn())
            {
                var invalidMappings = await _defaultValidator.ValidateContextAsync(context);
                var invalidMapping = invalidMappings.Should().ContainSingle().Subject;
                invalidMapping.TableName.Should().Be("tOrders");
                var missingColumn = invalidMapping.MissingColumns.Should().ContainSingle().Subject;
                missingColumn.Should().Be("OrderFate");
            }
        }
        
        [Fact]
        public async Task CaseInsensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await _caseInsensitiveValidator.ValidateContextAsync(context);
                invalidMappings.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task CaseSensitiveColumnNameComparison()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var invalidMappings = await _caseSensitiveValidator.ValidateContextAsync(context);
                var invalidMapping = invalidMappings.Should().ContainSingle().Subject; 
                invalidMapping.TableName.Should().Be("tOrders");
                invalidMapping.MissingColumns.Should().HaveCount(2);
                invalidMapping.MissingColumns.Should().Contain("oRdErDaTe", "cUsToMeRiD");
            }
        }
    }
}