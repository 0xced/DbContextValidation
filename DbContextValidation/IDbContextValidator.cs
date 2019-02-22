using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

#if EFCORE
namespace DbContextValidation.EFCore
#else
namespace DbContextValidation.EF6
#endif
{
    /// <summary>
    /// A service for validating a DbContext model
    /// </summary>
    public interface IDbContextValidator
    {
        /// <summary>
        /// Validates all the table and column names of entities defined in the DbContext model against the actual database associated to the context.
        /// </summary>
        /// <param name="context">The context you want to validate against its actual database connection.</param>
        /// <param name="progress">A progress reporting the validated <see cref="Table"/>s. If <code>null</code>, no progress is reported.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that you can use to abort the validation process.</param>
        /// <returns>A collection of <see cref="ValidationError"/>s, i.e. when an entity defined in the model does not have a matching table and column names in the database. If the context model exactly matches the actual database then an empty collection is returned.</returns>
        Task<IReadOnlyCollection<ValidationError>> ValidateContextAsync(DbContext context, IProgress<Table> progress = null, CancellationToken cancellationToken = default);
    }
}