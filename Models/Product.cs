using System.Collections.Generic;

namespace OrderFlow.Console.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    
    public int Stock { get; set; } 

    public List<OrderItem> OrderItems { get; set; } = new();
}