using System.Data.Entity;
using MySql.Data.EntityFramework;

namespace DbSchemaValidator.Tests.EF6.MySQL
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base("Host=localhost;Database=DbSchemaValidator;UserName=root;Password=docker")
        {
        }
    }
}