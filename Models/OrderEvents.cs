using System;
using System.Collections.Generic;

namespace OrderFlow.Console.Models;

public class OrderStatusChangedEventArgs : EventArgs
{
    public Order Order { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime Timestamp { get; set; }
}

public class OrderValidationEventArgs : EventArgs
{
    public Order Order { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}