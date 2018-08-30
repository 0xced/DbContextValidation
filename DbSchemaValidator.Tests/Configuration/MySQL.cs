namespace DbSchemaValidator.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.MySQL;
         public static readonly string Schema = null;
         
         public static readonly string Host = "localhost";
         public static readonly string Port = "3306";
         public static readonly string Database = "DbSchemaValidator";
         public static readonly string User = "root";
         public static readonly string Password = "docker";

         public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
     }
 }