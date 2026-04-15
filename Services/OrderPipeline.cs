using System;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderPipeline
{
    public event EventHandler<OrderStatusChangedEventArgs> StatusChanged;
    public event EventHandler<OrderValidationEventArgs> ValidationCompleted;

    public void ProcessOrder(Order order)
    {
        ChangeStatus(order, OrderStatus.Validated);
        ValidationCompleted?.Invoke(this, new OrderValidationEventArgs { Order = order, IsValid = true });
        ChangeStatus(order, OrderStatus.Processing);
        ChangeStatus(order, OrderStatus.Completed);
    }

    private void ChangeStatus(Order order, OrderStatus newStatus)
    {
        var oldStatus = order.Status;
        order.Status = newStatus;

        StatusChanged?.Invoke(this, new OrderStatusChangedEventArgs
        {
            Order = order,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Timestamp = DateTime.Now
        });
    }
}