using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Console.Data;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public class DatabaseSeeder
{
    public static async Task SeedAsync(OrderFlowContext db)
    {
        // Sprawdza, czy baza jest pusta
        if (await db.Products.AnyAsync()) return;

        var products = SampleData.Products;
        // Dodajemy Stock dla transakcji
        foreach (var p in products) p.Stock = 50; 
        products[0].Stock = 1; // Mało sztuk laptopa dla testu błędu!

        var customers = SampleData.Customers;
        var orders = SampleData.Orders;

        // Wyciągamy Id, żeby EF Core sam je nadał bez błędów w SQLite
        foreach(var p in products) p.Id = 0;
        foreach(var c in customers) c.Id = 0;
        foreach(var o in orders) o.Id = 0;

        db.Products.AddRange(products);
        db.Customers.AddRange(customers);
        db.Orders.AddRange(orders);

        await db.SaveChangesAsync();
    }
}