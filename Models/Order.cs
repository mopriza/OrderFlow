using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class Order
{
    [XmlAttribute("orderId")] // XML 
    [JsonPropertyName("identyfikator")] // JSON 
    public int Id { get; set; }
    
    [XmlElement("klient")] // XML 
    public Customer Customer { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    
    public List<OrderItem> Items { get; set; } = new();

    [JsonIgnore] // JSON 
    [XmlIgnore] // XML 
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
}