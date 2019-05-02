using System.Data.Entity;
using MySql.Data.EntityFramework;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context(string connectionString) : base(connectionString)
        {
        }
    }
}