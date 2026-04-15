using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class ExternalServiceSimulator
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3);

    public async Task CheckInventoryAsync(Product product)
    {
        int delay = Random.Shared.Next(500, 1501);
        await Task.Delay(delay);
    }

    public async Task ValidatePaymentAsync(Order order)
    {
        int delay = Random.Shared.Next(1000, 2001);
        await Task.Delay(delay);
    }

    public async Task CalculateShippingAsync(Order order)
    {
        int delay = Random.Shared.Next(300, 801);
        await Task.Delay(delay);
    }

    public async Task ProcessOrderAsync(Order order)
    {
        var sw = Stopwatch.StartNew();
        
        var productTask = CheckInventoryAsync(new Product()); 
        var paymentTask = ValidatePaymentAsync(order);
        var shippingTask = CalculateShippingAsync(order);

        await Task.WhenAll(productTask, paymentTask, shippingTask);
        sw.Stop();

        System.Console.WriteLine($"Zamówienie {order.Id} przetworzone w {sw.ElapsedMilliseconds}ms");
    }

    public async Task ProcessMultipleOrdersAsync(List<Order> orders)
    {
        int processedCount = 0;
        int total = orders.Count;
        var tasks = new List<Task>();

        foreach (var order in orders)
        {
            await _semaphore.WaitAsync();

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await ProcessOrderAsync(order);
                    int current = Interlocked.Increment(ref processedCount);
                    System.Console.WriteLine($"Przetworzono {current}/{total} zamówień.");
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }
        await Task.WhenAll(tasks);
    }
}