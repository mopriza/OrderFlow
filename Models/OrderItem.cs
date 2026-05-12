using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int OrderId { get; set; }
    [JsonIgnore] [XmlIgnore]
    public Order Order { get; set; }

    [JsonIgnore] [XmlIgnore]
    public decimal TotalPrice => Quantity * (Product?.Price ?? 0);
}