using System;
using System.Collections.Generic;
using System.Linq;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderProcessor
{
    private readonly List<Order> _orders;
    public OrderProcessor(List<Order> orders) => _orders = orders;

    // Predicate 
    public IEnumerable<Order> FilterOrders(Predicate<Order> filter) => _orders.Where(o => filter(o));

    // Action 
    public void ProcessOrders(Action<Order> action) => _orders.ForEach(action);

    // Func 
    public IEnumerable<T> ProjectOrders<T>(Func<Order, T> projection) => _orders.Select(projection);

    // Agregacja
    public decimal AggregateOrders(Func<IEnumerable<Order>, decimal> aggregator) => aggregator(_orders);
}