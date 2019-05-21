#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// Represents a validation error, i.e. when a table defined in the DbContext model does not match the corresponding table in the actual database.
    /// </summary>
    public abstract class ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="table">The table that is defined in the DbContext model.</param>
        protected ValidationError(Table table)
        {
            Table = table;
        }
        
        /// <summary>
        /// The table that is defined in the DbContext model.
        /// </summary>
        public Table Table { get; }
    }
}