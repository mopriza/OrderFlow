using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrderFlow.Console.Data;
using OrderFlow.Console.Persistence;
using OrderFlow.Console.Services;
using OrderFlow.Console.Watchers;

class Program
{
    static async Task Main(string[] args)
    {
        var orders = SampleData.Orders;
        var repo = new OrderRepository();
        
        System.Console.WriteLine(" Zapis i odczyt JSON / XML ");
        string jsonPath = "data/orders.json";
        string xmlPath = "data/orders.xml";

        await repo.SaveToJsonAsync(orders, jsonPath);
        await repo.SaveToXmlAsync(orders, xmlPath);
        System.Console.WriteLine("Zapisano dane do plików!");

        var loadedJson = await repo.LoadFromJsonAsync(jsonPath);
        var loadedXml = await repo.LoadFromXmlAsync(xmlPath);
        
        System.Console.WriteLine($"Oryginał: {orders.Count} zamówień, suma: {orders.Sum(o => o.TotalAmount)}");
        System.Console.WriteLine($"Wczytane JSON: {loadedJson.Count} zamówień, suma: {loadedJson.Sum(o => o.TotalAmount)}");
        System.Console.WriteLine($"Wczytane XML: {loadedXml.Count} zamówień, suma: {loadedXml.Sum(o => o.TotalAmount)}\n");


        System.Console.WriteLine("LINQ to XML Raport");
        var builder = new XmlReportBuilder();
        string reportPath = "data/report.xml";
        
        var reportDoc = builder.BuildReport(orders);
        await builder.SaveReportAsync(reportDoc, reportPath);
        System.Console.WriteLine("Raport XML wygenerowany i zapisany.");

        var highValueIds = await builder.FindHighValueOrderIdsAsync(reportPath, 1000m);
        System.Console.WriteLine($"Id zamówień powyżej 1000: {string.Join(", ", highValueIds)}\n");


        System.Console.WriteLine("Inbox Watcher");
        var pipeline = new OrderPipeline();
        pipeline.StatusChanged += (s, e) => System.Console.WriteLine($"   [Pipeline] Zamówienie {e.Order.Id} zmieniło status: {e.NewStatus}");

        string inboxPath = "inbox";
        using var watcher = new InboxWatcher(inboxPath, pipeline);
        watcher.Start();
        System.Console.WriteLine("Watcher uruchomiony. Generowanie testowych plików JSON...");

        
        for (int i = 1; i <= 2; i++)
        {
            await Task.Delay(2000); 
            string testFilePath = Path.Combine(inboxPath, $"test_import_{i}.json");
            
            // saving some orders
            var testOrders = orders.Take(2).ToList();
            testOrders[0].Id = 900 + i; // id changing
            await repo.SaveToJsonAsync(testOrders, testFilePath);
        }

        
        await Task.Delay(3000);
        System.Console.WriteLine("\ndone");
    }
}