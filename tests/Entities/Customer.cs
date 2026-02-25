using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DbContextValidation.Tests;

[SuppressMessage("ReSharper", "All")]
public class Customer
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}