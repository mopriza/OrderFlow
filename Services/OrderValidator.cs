using System;
using System.Collections.Generic;
using System.Linq;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public delegate bool ValidationRule(Order order, out string errorMessage);

public class OrderValidator
{
    // 3 reguły jako named methods
    public bool HasItems(Order order, out string errorMessage)
    {
        errorMessage = "The order is empty";
        return order.Items.Any();
    }

    public bool ValidAmount(Order order, out string errorMessage)
    {
        errorMessage = "Too expensive (limit 10000)!";
        return order.TotalAmount <= 10000;
    }

    public bool PositiveQuantity(Order order, out string errorMessage)
    {
        errorMessage = "The quantity of the product must be greater than zero.";
        return order.Items.All(i => i.Quantity > 0);
    }

    public bool ValidateAll(Order order)
    {
        var errors = new List<string>();

        var customRules = new List<ValidationRule> { HasItems, ValidAmount, PositiveQuantity };
        foreach (var rule in customRules)
        {
            if (!rule(order, out string err)) errors.Add(err);
        }

        var lambdaRules = new List<Func<Order, bool>>
        {
            o => o.OrderDate <= DateTime.Now,        
            o => o.Status != OrderStatus.Cancelled   
        };

        foreach (var func in lambdaRules)
        {
            if (!func(order)) errors.Add("Lambda check failed");
        }

        // result
        if (errors.Any())
        {
            System.Console.WriteLine($"\n[ERROR] Order {order.Id} is defective:");
            foreach (var e in errors) System.Console.WriteLine($" - {e}");
            return false;
        }

        System.Console.WriteLine($"\n[ОК] Order {order.Id} passed the test.");
        return true;
    }
}