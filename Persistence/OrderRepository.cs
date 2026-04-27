using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public class OrderRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task SaveToJsonAsync(IEnumerable<Order> orders, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, orders, _jsonOptions);
    }

    public async Task<List<Order>> LoadFromJsonAsync(string path)
    {
        if (!File.Exists(path)) return new List<Order>();
        
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await JsonSerializer.DeserializeAsync<List<Order>>(stream, _jsonOptions) ?? new List<Order>();
    }

    public async Task SaveToXmlAsync(IEnumerable<Order> orders, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        var serializer = new XmlSerializer(typeof(List<Order>));
        serializer.Serialize(stream, orders.ToList()); // W XML niestety SerializeAsync nie jest wbudowane bezpośrednio dla XmlSerializer, używamy standardu.
    }

    public async Task<List<Order>> LoadFromXmlAsync(string path)
    {
        if (!File.Exists(path)) return new List<Order>();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var serializer = new XmlSerializer(typeof(List<Order>));
        return await Task.Run(() => (List<Order>)serializer.Deserialize(stream)!) ?? new List<Order>();
    }
}