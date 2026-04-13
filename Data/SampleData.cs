using System;
using System.Collections.Generic;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Data;

public static class SampleData
{
    public static List<Product> Products { get; }
    public static List<Customer> Customers { get; }
    public static List<Order> Orders { get; }

    static SampleData()
    {
        // 5 produktów, różne kategorie
        Products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Category = "IT", Price = 5000 },
            new Product { Id = 2, Name = "Mouse", Category = "IT", Price = 100 },
            new Product { Id = 3, Name = "Table", Category = "Furniture", Price = 800 },
            new Product { Id = 4, Name = "Chair", Category = "Furniture", Price = 300 },
            new Product { Id = 5, Name = "Cup", Category = "Kitchen", Price = 20 }
        };

        // 4 klientów (w tym min. 1 VIP)
        Customers = new List<Customer>
        {
            new Customer { Id = 1, Name = "Yan", City = "Warszawa", IsVip = true },
            new Customer { Id = 2, Name = "Anna", City = "Krakow", IsVip = false },
            new Customer { Id = 3, Name = "Tomasz", City = "Warszawa", IsVip = false },
            new Customer { Id = 4, Name = "Yeva", City = "Poznan", IsVip = false }
        };

        // 6 orders
        Orders = new List<Order>
        {
            // Good orders
            new Order { Id = 101, Customer = Customers[0], Status = OrderStatus.New, Items = { new OrderItem { Product = Products[0], Quantity = 1 } } },
            
            new Order { Id = 102, Customer = Customers[1], Status = OrderStatus.Completed, Items = { new OrderItem { Product = Products[2], Quantity = 2 } } },
            
            new Order { Id = 103, Customer = Customers[2], Status = OrderStatus.Processing, Items = { new OrderItem { Product = Products[4], Quantity = 10 } } },
            
            new Order { Id = 104, Customer = Customers[0], Status = OrderStatus.New, Items = { new OrderItem { Product = Products[1], Quantity = 1 } } },
            
            new Order { Id = 105, Customer = Customers[3], Status = OrderStatus.New, Items = { new OrderItem { Product = Products[3], Quantity = 4 } } },

            // Bad order (no products, future date, canceled) - especially for validation tests!
            new Order { Id = 666, Customer = Customers[1], Status = OrderStatus.Cancelled, OrderDate = DateTime.Now.AddDays(5), Items = new List<OrderItem>() }
        };
    }
}