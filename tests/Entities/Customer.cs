﻿using System.Collections.Generic;

namespace DbContextValidation.Tests
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}