using System.Collections.Generic;

namespace DbSchemaValidator.Tests.DB
{
    public class Customer
    {
        public long CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}