using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Console.Models;
using OrderFlow.Console.Persistence;

class Program
{
    static async Task Main(string[] args)
    {
        using var db = new OrderFlowContext();
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);

        System.Console.WriteLine("\n zadanie 2: CRUD ");
        
        // CREATE
        var newOrder = new Order { CustomerId = 1, Notes = "Pilne!", Status = OrderStatus.New };
        newOrder.Items.Add(new OrderItem { ProductId = 2, Quantity = 2 }); 
        newOrder.Items.Add(new OrderItem { ProductId = 5, Quantity = 1 }); 
        db.Orders.Add(newOrder);
        await db.SaveChangesAsync();
        System.Console.WriteLine($"[CREATE] Dodano zamówienie #{newOrder.Id} z 2 pozycjami.");

        // READ
        var readOrder = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == newOrder.Id);
        System.Console.WriteLine($"[READ] Wczytano zamówienie: Klient: {readOrder.Customer.Name}, Kwota: {readOrder.TotalAmount}");

        // UPDATE
        readOrder.Status = OrderStatus.Processing;
        readOrder.Notes = "Już nie takie pilne";
        await db.SaveChangesAsync();
        System.Console.WriteLine($"[UPDATE] Zaktualizowano status na: {readOrder.Status}");

        // DELETE
        var orderToDelete = await db.Orders.FirstOrDefaultAsync(o => o.Status == OrderStatus.Cancelled);
        if (orderToDelete != null)
        {
            db.Orders.Remove(orderToDelete);
            await db.SaveChangesAsync();
            System.Console.WriteLine($"[DELETE] Usunięto anulowane zamówienie #{orderToDelete.Id}");
        }


        System.Console.WriteLine("\n zadanie 3: ZAPYTANIA LINQ ");
        
        // 1. Zamówienia VIP > progu (Używamy rzutowania na double dla SQLite!)
        var vipOrders = await db.Orders.Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Customer.IsVip && o.Items.Sum(i => (double)(i.Quantity * i.Product.Price)) > 1000).ToListAsync();
        System.Console.WriteLine($"1. Zamówienia VIP > 1000 PLN: Znaleziono {vipOrders.Count}");

        // 2. Ranking klientów
        var ranking = await db.Orders.Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product)
            .GroupBy(o => o.Customer.Name)
            .Select(g => new { Name = g.Key, Total = g.Sum(o => o.Items.Sum(i => (double)(i.Quantity * i.Product.Price))) })
            .OrderByDescending(x => x.Total).ToListAsync();
        System.Console.WriteLine($"2. Najlepszy klient: {ranking.First().Name} ({ranking.First().Total} PLN)");

        // 3. Średnia wartość per miasto
        var avgPerCity = await db.Orders.Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product)
            .GroupBy(o => o.Customer.City)
            .Select(g => new { City = g.Key, Avg = g.Average(o => o.Items.Sum(i => (double)(i.Quantity * i.Product.Price))) }).ToListAsync();
        System.Console.WriteLine($"3. Średnia dla Warszawy: {avgPerCity.FirstOrDefault(x => x.City == "Warszawa")?.Avg:F2} PLN");

        // 4. Produkty nigdy nie zamówione (Anti-join)
        var unusedProducts = await db.Products.Where(p => !p.OrderItems.Any()).ToListAsync();
        System.Console.WriteLine($"4. Nigdy nie zamówione produkty: {string.Join(", ", unusedProducts.Select(p => p.Name))}");

        // 5. Dynamiczne zapytanie
        OrderStatus? filterStatus = OrderStatus.Processing;
        decimal minAmount = 500m;
        IQueryable<Order> query = db.Orders.Include(o => o.Items).ThenInclude(i => i.Product);
        if (filterStatus.HasValue) query = query.Where(o => o.Status == filterStatus.Value);
        var dynamicResult = await query.ToListAsync();
        var finalFiltered = dynamicResult.Where(o => o.TotalAmount > minAmount).ToList();
        System.Console.WriteLine($"5. Dynamiczne (Processing, >500 PLN): {finalFiltered.Count} zamówień");


        System.Console.WriteLine("\n zadanie 3: TRANSAKCJE ");
        
        // Sukces (zamówienie 102 - ma dużo towaru)
        await ProcessOrderTransactionAsync(db, 2); 
        
        // Błąd (zamówienie 101 - ma laptopa, a daliśmy mu Stock = 1, zróbmy próbę kupienia 2 sztuk ręcznie)
        var failOrder = new Order { CustomerId = 1, Status = OrderStatus.New };
        failOrder.Items.Add(new OrderItem { ProductId = 1, Quantity = 5 }); // Chcemy 5 laptopów (jest 1)
        db.Orders.Add(failOrder);
        await db.SaveChangesAsync();
        
        await ProcessOrderTransactionAsync(db, failOrder.Id);
    }

    static async Task ProcessOrderTransactionAsync(OrderFlowContext db, int orderId)
    {
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            order.Status = OrderStatus.Processing;

            foreach (var item in order.Items)
            {
                if (item.Product.Stock < item.Quantity)
                {
                    throw new Exception($"Brak towaru na magazynie: {item.Product.Name}");
                }
                item.Product.Stock -= item.Quantity;
            }

            order.Status = OrderStatus.Completed;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            System.Console.WriteLine($"[Transakcja] SUKCES: Zamówienie #{orderId} zrealizowane.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            System.Console.WriteLine($"[Transakcja] ROLLBACK: Zamówienie #{orderId} anulowane. Błąd: {ex.Message}");
        }
    }
}