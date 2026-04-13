using System;
using System.Linq;
using OrderFlow.Console.Data;
using OrderFlow.Console.Services;
using OrderFlow.Console.Models;


Console.WriteLine("walidacja(2)");
var validator = new OrderValidator();
validator.ValidateAll(SampleData.Orders[0]); // good order
validator.ValidateAll(SampleData.Orders[5]); // bad order (Id: 666)



Console.WriteLine("\nprocessor(3)");
var processor = new OrderProcessor(SampleData.Orders);


Predicate<Order> isVip = o => o.Customer.IsVip;
Predicate<Order> isNew = o => o.Status == OrderStatus.New;
Predicate<Order> isExpensive = o => o.TotalAmount > 100;

Action<Order> printAction = o => Console.WriteLine($"Order: {o.Id}, Sum: {o.TotalAmount}");
Action<Order> validateAction = o => o.Status = OrderStatus.Validated;


var projected = processor.ProjectOrders(o => new { o.Id, o.Customer.Name });


var sum = processor.AggregateOrders(list => list.Sum(o => o.TotalAmount));
var max = processor.AggregateOrders(list => list.Max(o => o.TotalAmount));
var avg = processor.AggregateOrders(list => list.Average(o => o.TotalAmount));


Console.WriteLine("Call Chain (Top 2 VIP Orders):");
processor.FilterOrders(isVip)
         .OrderByDescending(o => o.TotalAmount)
         .Take(2)
         .ToList()
         .ForEach(printAction);



Console.WriteLine("\n=linq");


var query1 = from o in SampleData.Orders
             join c in SampleData.Customers on o.Customer.Id equals c.Id
             group o by c.City into cityGroup
             select new { City = cityGroup.Key, Count = cityGroup.Count() };
Console.WriteLine("\n1. Zamówienia po miastach:");
foreach (var q in query1) Console.WriteLine($"{q.City}: {q.Count}");

// 2. SelectMany (Składnia metod / Method syntax)
// Użyto method syntax, bo SelectMany idealnie spłaszcza kolekcje w jednym płynnym wywołaniu.
var query2 = SampleData.Orders
                       .SelectMany(o => o.Items)
                       .Select(i => i.Product.Name)
                       .Distinct();
Console.WriteLine($"\n2. Wszystkie sprzedane produkty: {string.Join(", ", query2)}");

var query3 = SampleData.Orders
                       .GroupBy(o => o.Customer.Name)
                       .Select(g => new { Name = g.Key, Total = g.Sum(o => o.TotalAmount) })
                       .OrderByDescending(x => x.Total);
Console.WriteLine("\n3. Top klienci wg kwoty:");
foreach (var q in query3) Console.WriteLine($"{q.Name}: {q.Total}");

var query4 = from c in SampleData.Customers
             join o in SampleData.Orders on c.Id equals o.Customer.Id into custOrders
             select new { c.Name, OrdersCount = custOrders.Count() };
Console.WriteLine("\n4. Klienci i liczba ich zamówień (Left Join):");
foreach (var q in query4) Console.WriteLine($"{q.Name}: {q.OrdersCount}");


var query5 = (from o in SampleData.Orders
              select new { o.Customer.Name, o.TotalAmount })
             .Where(x => x.TotalAmount > 500)
             .ToList();
Console.WriteLine("\n5. Zamówienia powyżej 500 (Mixed syntax):");
foreach (var q in query5) Console.WriteLine($"{q.Name}: {q.TotalAmount}");

var query6 = SampleData.Orders.Where(o => o.Status == OrderStatus.New);
Console.WriteLine($"\n6. Ilość nowych zamówień: {query6.Count()}");