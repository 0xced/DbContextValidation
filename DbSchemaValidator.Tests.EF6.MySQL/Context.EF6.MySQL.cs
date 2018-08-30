using System.Data.Entity;
using MySql.Data.EntityFramework;

// ReSharper disable once CheckNamespace
namespace DbSchemaValidator.Tests
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base(Config.ConnectionString)
        {
        }
    }
}