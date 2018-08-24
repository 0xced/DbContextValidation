using System;
using System.Collections.Generic;
using System.Linq;

#if EFCORE
namespace DbSchemaValidator.EFCore
#else
namespace DbSchemaValidator.EF6
#endif
{
    public class ValidationProgress : IProgress<Validation>
    {
        private readonly List<Validation> _validations = new List<Validation>();
        
        public void Report(Validation validation)
        {
            _validations.Add(validation);
        }

        public IReadOnlyCollection<InvalidMapping> InvalidMappings => _validations.Where(e => e.InvalidMapping != null).Select(e => e.InvalidMapping).ToList();
        
        public IReadOnlyCollection<float> Fractions => _validations.Select(e => e.FractionCompleted).ToList();
        
        public IReadOnlyCollection<string> SelectStatements => _validations.Select(e => e.SelectStatement).ToList();
    }
}