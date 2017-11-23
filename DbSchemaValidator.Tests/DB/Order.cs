using System;

namespace DbSchemaValidator.Tests.DB
{
    public class Order
    {
        public long OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public long CustomerNumber { get; set; }
        public virtual Customer Customer { get; set; }
    }
}