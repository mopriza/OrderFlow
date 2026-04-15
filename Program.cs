using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("\n 1 Zdarzenia (Events)");
        var pipeline = new OrderPipeline();
        
        pipeline.StatusChanged += (sender, e) => System.Console.WriteLine($"[Logger] Zamówienie {e.Order.Id}: {e.OldStatus} -> {e.NewStatus}");
        pipeline.StatusChanged += (sender, e) => { if(e.NewStatus == OrderStatus.Completed) System.Console.WriteLine($"[Email] Wysłano potwierdzenie dla zamówienia {e.Order.Id}"); };
        pipeline.ValidationCompleted += (sender, e) => System.Console.WriteLine($"[Statystyki] Walidacja zamówienia {e.Order.Id} zakończona. Wynik: {e.IsValid}");

        pipeline.ProcessOrder(new Order { Id = 101, Status = OrderStatus.New });
        pipeline.ProcessOrder(new Order { Id = 102, Status = OrderStatus.New });

        System.Console.WriteLine("\n 2 Asynchroniczność ");
        var simulator = new ExternalServiceSimulator();
        var orders = new List<Order>();
        for (int i = 1; i <= 6; i++) orders.Add(new Order { Id = 200 + i });

        System.Console.WriteLine("Sekwencyjnie:");
        var swSeq = Stopwatch.StartNew();
        foreach (var o in orders) await simulator.ProcessOrderAsync(o);
        System.Console.WriteLine($"Czas sekwencyjny: {swSeq.ElapsedMilliseconds}ms\n");

        System.Console.WriteLine("Równolegle (z ograniczeniem do 3):");
        var swPar = Stopwatch.StartNew();
        await simulator.ProcessMultipleOrdersAsync(orders);
        System.Console.WriteLine($"Czas równoległy: {swPar.ElapsedMilliseconds}ms\n");

        System.Console.WriteLine("\n 3: Thread Safety ");
        var stats = new OrderStatistics();
        var massOrders = new List<Order>();
        for (int i = 0; i < 10000; i++) massOrders.Add(new Order { Id = i, Status = OrderStatus.Processing });

        Parallel.ForEach(massOrders, order => 
        {
            try { stats.ProcessWithBug(order, false); } catch { }
        });

        Parallel.ForEach(massOrders, order => 
        {
            stats.ProcessSafely(order, false);
        });

        System.Console.WriteLine("Oczekiwane: TotalProcessed = 10000, TotalRevenue = 1000000");
        System.Console.WriteLine($"Z BUGIEM: Processed = {stats.BuggyTotalProcessed}, Revenue = {stats.BuggyTotalRevenue}, Błędy = {stats.BuggyProcessingErrors.Count}");
        System.Console.WriteLine($"BEZPIECZNE: Processed = {stats.SafeTotalProcessed}, Revenue = {stats.SafeTotalRevenue}, Błędy = {stats.SafeProcessingErrors.Count}");
    }
}