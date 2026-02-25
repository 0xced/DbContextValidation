using System.Diagnostics.CodeAnalysis;

namespace DbContextValidation.Tests;

[SuppressMessage("ReSharper", "All")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class DbFixture
{
    public string? Schema => null;

    public string ConnectionString => "Data Source=DbContextValidation.sqlite3";
}