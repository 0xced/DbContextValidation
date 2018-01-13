﻿using System;

namespace DbSchemaValidator.EFCore.Tests.DB
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public long CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}