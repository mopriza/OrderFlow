using System;
using System.Threading.Tasks;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderPipeline
{
    public event EventHandler<OrderStatusChangedEventArgs> StatusChanged;
    public event EventHandler<OrderValidationEventArgs> ValidationCompleted;

    // Вот наш старый метод из Лабы 2
    public void ProcessOrder(Order order)
    {
        ChangeStatus(order, OrderStatus.Validated);
        ValidationCompleted?.Invoke(this, new OrderValidationEventArgs { Order = order, IsValid = true });
        ChangeStatus(order, OrderStatus.Processing);
        ChangeStatus(order, OrderStatus.Completed);
    }

    // А ВОТ ЭТУ ШТУКУ ПРОСИТ ПРЕПОД ДЛЯ ЛАБЫ 3 (мы просто добавили её сюда)
    public Task ProcessOrderAsync(Order order)
    {
        return Task.Run(() => ProcessOrder(order));
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