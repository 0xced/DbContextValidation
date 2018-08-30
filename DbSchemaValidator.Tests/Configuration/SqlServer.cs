namespace DbSchemaValidator.Tests
 {
     public static class Config
     {
         public static readonly Provider Provider = Provider.SqlServer;
         
         public static readonly string Host = "localhost";
         public static readonly string Port = "1433";
         public static readonly string Database = "tempdb";
         public static readonly string User = "sa";
         public static readonly string Password = "SqlServer-doc4er";
 
         public static readonly string ConnectionString = $"Server={Host};Database={Database};User Id={User};Password={Password}";
     }
 }