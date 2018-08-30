namespace DbSchemaValidator.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.SQLite;
         
         public static readonly string Host = null;
         public static readonly string Port = null;
         public static readonly string Database = "../../../../DbSchemaValidator.Tests/DbSchemaValidator.db";
         public static readonly string User = null;
         public static readonly string Password = null;

         public static readonly string ConnectionString = $"Data Source={Database}";
     }
 }