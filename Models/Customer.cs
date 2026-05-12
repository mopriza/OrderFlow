using System.Collections.Generic;

namespace OrderFlow.Console.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public bool IsVip { get; set; } 
    
    public string? Email { get; set; } 
    
    public List<Order> Orders { get; set; } = new();
}