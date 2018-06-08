using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace DbSchemaValidator.Tests.EFCore
{
    public abstract class Context : DbContext
    {
        private static readonly LoggerFactory ConsoleLoggerFactory = new LoggerFactory(new[] {new ConsoleLoggerProvider((name, level) => true, true)});

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=../../../../DbSchemaValidator.Tests/DbSchemaValidator.db")
                .UseLoggerFactory(ConsoleLoggerFactory);
        }
    }
}