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
    /// TODO
    /// </summary>
    public interface IDbContextValidator
    {
        /// <summary>
        /// Validates all the table and column names of entities defined in the model associated with the context against the actual database schema.
        /// </summary>
        /// <param name="context">The context you want to validate against its actual database connection.</param>
        /// <param name="progress">A progress reporting numbers between 0.0 and 1.0 representing the completed fraction of the validation process. If <code>null</code>, no progress is reported.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that you can use to abort the validation process.</param>
        /// <returns>A collection of <see cref="InvalidMapping"/>s, i.e. when an entity defined in the model does not have a matching table and column names in the database. If the context model exactly matches the database schema then an empty collection is returned.</returns>
        Task<IReadOnlyCollection<InvalidMapping>> ValidateSchemaAsync(DbContext context, IProgress<float> progress = null, CancellationToken cancellationToken = default);
    }
}