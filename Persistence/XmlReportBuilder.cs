using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public class XmlReportBuilder
{
    public XDocument BuildReport(IEnumerable<Order> orders)
    {
        var doc = new XDocument(
            new XElement("report",
                new XAttribute("generated", DateTime.Now.ToString("s")),
                
                new XElement("summary",
                    new XAttribute("totalOrders", orders.Count()),
                    new XAttribute("totalRevenue", orders.Sum(o => o.TotalAmount))
                ),
                
                new XElement("byStatus",
                    orders.GroupBy(o => o.Status).Select(g => 
                        new XElement("status",
                            new XAttribute("name", g.Key.ToString()),
                            new XAttribute("count", g.Count()),
                            new XAttribute("revenue", g.Sum(o => o.TotalAmount))
                        )
                    )
                ),
                
                new XElement("byCustomer",
                    orders.GroupBy(o => o.Customer).Select(g => 
                        new XElement("customer",
                            new XAttribute("id", g.Key?.Id ?? 0),
                            new XAttribute("name", g.Key?.Name ?? "Unknown"),
                            new XAttribute("isVip", g.Key?.IsVip.ToString().ToLower() ?? "false"),
                            new XElement("orderCount", g.Count()),
                            new XElement("totalSpent", g.Sum(o => o.TotalAmount)),
                            new XElement("orders",
                                g.Select(o => 
                                    new XElement("orderRef",
                                        new XAttribute("id", o.Id),
                                        new XAttribute("total", o.TotalAmount)
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );
        return doc;
    }

    public async Task SaveReportAsync(XDocument report, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await report.SaveAsync(stream, SaveOptions.None, default);
    }

    // Чтение прямо из XML файла
    public async Task<IEnumerable<int>> FindHighValueOrderIdsAsync(string reportPath, decimal threshold)
    {
        if (!File.Exists(reportPath)) return Enumerable.Empty<int>();

        await using var stream = new FileStream(reportPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, default);

        var ids = doc.Descendants("orderRef")
                     .Where(x => decimal.Parse(x.Attribute("total")!.Value) > threshold)
                     .Select(x => int.Parse(x.Attribute("id")!.Value));
                     
        return ids;
    }
}