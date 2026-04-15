using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderStatistics
{
    public int BuggyTotalProcessed = 0;
    public decimal BuggyTotalRevenue = 0m;
    public Dictionary<OrderStatus, int> BuggyOrdersPerStatus = new Dictionary<OrderStatus, int>();
    public List<string> BuggyProcessingErrors = new List<string>();

    public void ProcessWithBug(Order order, bool isValid)
    {
        BuggyTotalProcessed++;
        BuggyTotalRevenue += 100m; 

        if (!BuggyOrdersPerStatus.ContainsKey(order.Status))
            BuggyOrdersPerStatus[order.Status] = 0;
        BuggyOrdersPerStatus[order.Status]++;

        if (!isValid)
            BuggyProcessingErrors.Add($"Błąd walidacji zamówienia {order.Id}");
    }

    public int SafeTotalProcessed = 0;
    public decimal SafeTotalRevenue = 0m;
    public ConcurrentDictionary<OrderStatus, int> SafeOrdersPerStatus = new ConcurrentDictionary<OrderStatus, int>();
    public List<string> SafeProcessingErrors = new List<string>();

    private readonly object _revenueLock = new object();
    private readonly object _errorsLock = new object();

    public void ProcessSafely(Order order, bool isValid)
    {
        Interlocked.Increment(ref SafeTotalProcessed);

        lock (_revenueLock)
        {
            SafeTotalRevenue += 100m;
        }

        SafeOrdersPerStatus.AddOrUpdate(order.Status, 1, (key, oldValue) => oldValue + 1);

        if (!isValid)
        {
            lock (_errorsLock)
            {
                SafeProcessingErrors.Add($"Błąd walidacji zamówienia {order.Id}");
            }
        }
    }
}