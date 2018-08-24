#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public struct Validation
    {
        public Validation(float fractionCompleted, string selectStatement, InvalidMapping invalidMapping)
        {
            FractionCompleted = fractionCompleted;
            SelectStatement = selectStatement;
            InvalidMapping = invalidMapping;
        }

        public float FractionCompleted { get; }
        public string SelectStatement { get; }
        public InvalidMapping InvalidMapping { get; }
    }
}