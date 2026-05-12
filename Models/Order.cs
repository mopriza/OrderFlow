using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class Order
{
    public int Id { get; set; }
    
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    
    public string? Notes { get; set; } // Новое поле
    
    public List<OrderItem> Items { get; set; } = new();

    [JsonIgnore] [XmlIgnore]
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
}