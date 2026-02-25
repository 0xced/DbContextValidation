using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
#if EFCORE
using DbContextValidation.EFCore;
using Microsoft.EntityFrameworkCore;
#else
using DbContextValidation.EF6;
using System.Data.Entity;
#endif
using Xunit;

#if PROVIDER_FIREBIRD
namespace DbContextValidation.Tests.Firebird;
#elif PROVIDER_MYSQL
namespace DbContextValidation.Tests.MySQL;
#elif PROVIDER_MYSQL_POMELO
namespace DbContextValidation.Tests.MySQL.Pomelo;
#elif PROVIDER_NPGSQL
namespace DbContextValidation.Tests.Npgsql;
#elif PROVIDER_ORACLE
namespace DbContextValidation.Tests.Oracle;
#elif PROVIDER_SQLITE
namespace DbContextValidation.Tests.SQLite;
#elif PROVIDER_SQLSERVER
namespace DbContextValidation.Tests.SqlServer;
#else
#error Make sure to define a PROVIDER_* constant in the tests project
namespace DbContextValidation.Tests;
#endif

[SuppressMessage("ReSharper", "VSTHRD200", Justification = "Naming all tests methods with the Async suffix feels weird")]
public class ValidatorTests(DbFixture dbFixture) : IClassFixture<DbFixture>
{
    private class AccumulatorProgress<T> : IProgress<T>
    {
        private readonly List<T> _items = [];

        public IReadOnlyList<T> Items => _items;

        public void Report(T value)
        {
            _items.Add(value);
        }
    }

    private readonly DbContextValidator _defaultValidator = new(StringComparer.InvariantCulture);

    [Fact]
    public async Task Validator_ValidContext_ReturnNoErrors()
    {
        // Arrange
        var context = new ValidContext(dbFixture.ConnectionString);

        // Act
        var errors = await _defaultValidator.ValidateContextAsync(context);

        // Assert
        errors.Should().BeEmpty();
        _ = context.Products;
        // ReSharper disable AccessToDisposedClosure
        var customersTask = async () => { await context.Customers.ToListAsync(); };
        var ordersTask = async () => { await context.Orders.ToListAsync(); };
        // ReSharper restore AccessToDisposedClosure
        await customersTask.Should().NotThrowAsync();
        await ordersTask.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validator_ValidContextWithExplicitSchema_ReturnNoErrors()
    {
        // Arrange
        var context = new ValidContextWithExplicitSchema(dbFixture.ConnectionString, dbFixture.Schema);

        // Act
        var errors = await _defaultValidator.ValidateContextAsync(context);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_ValidContext_ReportsProgress()
    {
        // Arrange
        var progress = new AccumulatorProgress<Table>();
        var context = new ValidContext(dbFixture.ConnectionString);

        // Act
        await _defaultValidator.ValidateContextAsync(context, progress);

        // Assert
        progress.Items.Select(e => e.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
    }

    [Fact]
    public void Validator_ValidContext_SupportsCancellation()
    {
        // Arrange
        var context = new ValidContext(dbFixture.ConnectionString);

        // Act
        var validationTask = _defaultValidator.ValidateContextAsync(context, cancellationToken: new CancellationToken(true));

        // Assert
        validationTask.Status.Should().Be(TaskStatus.Canceled);
    }

    [Fact]
    public async Task Validator_UnknownSchema_ReturnsMissingTableErrors()
    {
        // Arrange
        var context = new ContextWithUnknownSchema(dbFixture.ConnectionString);

        // Act
        var errors = await _defaultValidator.ValidateContextAsync(context);

        // Assert
        errors.Should().OnlyContain(e => e.Table.Schema == "unknown");
        errors.Should().OnlyContain(e => e.GetType() == typeof(MissingTableError));
        errors.Select(e => e.Table.TableName).Should().BeEquivalentTo("tCustomers", "tOrders");
        var exceptions = errors.OfType<MissingTableError>().Select(e => e.MissingTableException).ToList();
#if !PROVIDER_MYSQL && !PROVIDER_MYSQL_POMELO // The message for MySQL is this: "SELECT command denied to user 'mysql'@'192.168.215.1' for table 'tableName'"
            exceptions.Select(e => e.DbException.Message).Should().AllSatisfy(message => message.Should().Contain("unknown"));
#endif
        exceptions.Select(e => e.SelectStatement).Should().AllSatisfy(message => message.Should().StartWith("SELECT *"));
    }

    [Fact]
    public async Task Validator_MisspelledCustomersTable_ReturnsMissingTableError()
    {
        // Arrange
        var context = new ContextWithMisspelledCustomersTable(dbFixture.ConnectionString);

        // Act
        var errors = await _defaultValidator.ValidateContextAsync(context);

        // Assert
        var error = errors.Should().ContainSingle().Subject;
        error.Table.TableName.Should().Be("Customers");
        error.Should().BeOfType<MissingTableError>();
    }

    [Fact]
    public async Task Validator_MisspelledOrderDateColumn_ReturnsMissingColumnsError()
    {
        // Arrange
        var context = new ContextWithMisspelledOrderDateColumn(dbFixture.ConnectionString);

        // Act
        var errors = await _defaultValidator.ValidateContextAsync(context);

        // Assert
        var error = errors.Should().ContainSingle().Subject;
        error.Table.TableName.Should().Be("tOrders");
        error.Should().BeOfType<MissingColumnsError>()
            .Which.ColumnNames.Should().ContainSingle()
            .Which.Should().Be("OrderFate");
    }

    [Fact]
    public async Task Validator_CaseInsensitiveColumnNameComparison_ReturnNoErrors()
    {
        // Arrange
        var context = new ContextWithMixedCaseColumns(dbFixture.ConnectionString);
        var caseInsensitiveValidator = new DbContextValidator(StringComparer.InvariantCultureIgnoreCase);

        // Act
        var errors = await caseInsensitiveValidator.ValidateContextAsync(context);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_CaseSensitiveColumnNameComparison_ReturnsMissingColumnsError()
    {
        // Arrange
        var context = new ContextWithMixedCaseColumns(dbFixture.ConnectionString);

        // Act
        var errors = await _defaultValidator.ValidateContextAsync(context);

        // Assert
        var error = errors.Should().ContainSingle().Subject;
        error.Table.TableName.Should().Be("tOrders");
        error.Should().BeOfType<MissingColumnsError>()
            .Which.ColumnNames.Should().BeEquivalentTo("oRdErDaTe", "cUsToMeRiD");
    }
}