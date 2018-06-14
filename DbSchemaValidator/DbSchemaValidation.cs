#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public class DbSchemaValidation
    {
        internal DbSchemaValidation(int table, int tableCount, string tableName, InvalidMapping invalidMapping)
        {
            Table = table;
            TableCount = tableCount;
            TableName = tableName;
            InvalidMapping = invalidMapping;
        }

        public int Table { get; }
        public int TableCount { get; }
        public string TableName { get; }
        public InvalidMapping InvalidMapping { get; }
    }
}