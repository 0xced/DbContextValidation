namespace DbContextValidation.Tests
{
    public class Configuration
    {
        public const string Schema = null;
    }

    public class DbFixture
    {
        public string ConnectionString => $"Data Source={SqlInitializationHelper.SqlFile("DbContextValidation.sqlite3")}";
    }
}