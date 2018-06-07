#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public class DbSchemaValidation
    {
        internal DbSchemaValidation(float fractionCompleted, string tableName)
        {
            FractionCompleted = fractionCompleted;
            TableName = tableName;
        }

        public float FractionCompleted { get; }
        public string TableName { get; }
    }
}