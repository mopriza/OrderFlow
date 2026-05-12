using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public class OrderFlowContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=orderflow.db")
                      .LogTo(System.Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Relacje: Customer -> Order (1:N, Restrict)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // 2. Relacje: Order -> OrderItem (1:N, Cascade)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Relacje: OrderItem -> Product (N:1, Restrict)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Wykluczenia
        modelBuilder.Entity<Order>().Ignore(o => o.TotalAmount);
        modelBuilder.Entity<OrderItem>().Ignore(oi => oi.TotalPrice);

        // HasPrecision
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

        // Indeksy
        modelBuilder.Entity<Customer>().HasIndex(c => c.Name);
        modelBuilder.Entity<Order>().HasIndex(o => o.Status);
    }
}