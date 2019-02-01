using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "VSTHRD200", Justification = "Naming all tests methods with the Async suffix feels weird")]
    public class ValidatorTests : IClassFixture<DatabaseFixture>
    {
        private class AccumulatorProgress<T> : IProgress<T>
        {
            private readonly List<T> _items = new List<T>();

            public IReadOnlyList<T> Items => _items;

            public void Report(T value)
            {
                _items.Add(value);
            }
        }
        
        private readonly DbContextValidator _defaultValidator;

        public ValidatorTests()
        {
            _defaultValidator = new DbContextValidator(StringComparer.InvariantCulture);
        }

        [Fact]
        public async Task Validator_ValidContext_ReturnNoErrors()
        {
            using (var context = new ValidContext())
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                errors.Should().BeEmpty();
                // ReSharper disable AccessToDisposedClosure
                Func<Task> customersTask = async () => { await context.Customers.ToListAsync(); };
                Func<Task> ordersTask = async () => { await context.Orders.ToListAsync(); };
                // ReSharper restore AccessToDisposedClosure
                await customersTask.Should().NotThrowAsync();
                await ordersTask.Should().NotThrowAsync();
            }
        }
        
        [Fact]
        public async Task Validator_ValidContextWithExplicitSchema_ReturnNoErrors()
        {
            using (var context = new ValidContextWithExplicitSchema())
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                errors.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task Validator_ValidContext_ReportsProgress()
        {
            var progress = new AccumulatorProgress<float>();
            using (var context = new ValidContext())
            {
                await _defaultValidator.ValidateContextAsync(context, progress);
                progress.Items.Should().BeEquivalentTo(new []{ 0.5f, 1.0f }, options => options.WithStrictOrdering());
            }
        }
        
        [Fact]
        public void Validator_ValidContext_SupportsCancellation()
        {
            using (var context = new ValidContext())
            {
                var validationTask = _defaultValidator.ValidateContextAsync(context, cancellationToken: new CancellationToken(true));
                validationTask.Status.Should().Be(TaskStatus.Canceled);
            }
        }

        [Fact]
        public async Task Validator_UnknownSchema_ReturnsMissingTableErrors()
        {
            using (var context = new ContextWithUnknownSchema())
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                errors.Should().OnlyContain(e => e.Schema == "unknown");
                errors.Should().OnlyContain(e => e.GetType() == typeof(MissingTableError));
                errors.Select(e => e.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
            }
        }
        
        [Fact]
        public async Task Validator_MisspelledCustomersTable_ReturnsMissingTableError()
        {
            using (var context = new ContextWithMisspelledCustomersTable())
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                var error = errors.Should().ContainSingle().Subject;
                error.TableName.Should().Be("Customers");
                error.Should().BeOfType<MissingTableError>();
            }
        }
        
        [Fact]
        public async Task Validator_MisspelledOrderDateColumn_ReturnsMissingColumnsError()
        {
            using (var context = new ContextWithMisspelledOrderDateColumn())
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                var error = errors.Should().ContainSingle().Subject;
                error.TableName.Should().Be("tOrders");
                error.Should().BeOfType<MissingColumnsError>()
                    .Which.ColumnNames.Should().ContainSingle()
                    .Which.Should().Be("OrderFate");
            }
        }
        
        [Fact]
        public async Task Validator_CaseInsensitiveColumnNameComparison_ReturnNoErrors()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var caseInsensitiveValidator = new DbContextValidator(StringComparer.InvariantCultureIgnoreCase);
                var errors = await caseInsensitiveValidator.ValidateContextAsync(context);
                errors.Should().BeEmpty();
            }
        }
        
        [Fact]
        public async Task Validator_CaseSensitiveColumnNameComparison_ReturnsMissingColumnsError()
        {
            using (var context = new ContextWithMixedCaseColumns())
            {
                var errors = await _defaultValidator.ValidateContextAsync(context);
                var error = errors.Should().ContainSingle().Subject; 
                error.TableName.Should().Be("tOrders");
                error.Should().BeOfType<MissingColumnsError>()
                    .Which.ColumnNames.Should().BeEquivalentTo("oRdErDaTe", "cUsToMeRiD");
            }
        }
    }
}