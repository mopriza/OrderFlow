using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderFlow.Console.Models;

public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public List<OrderItem> Items { get; set; } = new();

    //  TotalAmount
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
}