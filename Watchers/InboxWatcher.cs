using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Console.Watchers;

public class InboxWatcher : IDisposable
{
    private readonly string _inboxPath;
    private readonly OrderPipeline _pipeline;
    private readonly FileSystemWatcher _watcher;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2);

    public InboxWatcher(string inboxPath, OrderPipeline pipeline)
    {
        _inboxPath = inboxPath;
        _pipeline = pipeline;

        Directory.CreateDirectory(_inboxPath);
        Directory.CreateDirectory(Path.Combine(_inboxPath, "processed"));
        Directory.CreateDirectory(Path.Combine(_inboxPath, "failed"));

        _watcher = new FileSystemWatcher(_inboxPath, "*.json")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };
        _watcher.Created += OnFileCreated;
    }

    public void Start() => _watcher.EnableRaisingEvents = true;

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        await _semaphore.WaitAsync();
        try
        {
            await ProcessFileAsync(e.FullPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ProcessFileAsync(string filePath)
    {
        // ЗАЩИТА ОТ МАКА: Если файла уже нет (дубликат события), просто уходим
        if (!File.Exists(filePath)) return;

        System.Console.WriteLine($"[Watcher] Wykryto nowy plik: {Path.GetFileName(filePath)}");
        
        int retries = 5;
        string fileContent = null;
        
        while (retries > 0)
        {
            try
            {
                if (!File.Exists(filePath)) return; // Еще одна проверка на всякий случай

                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                using var reader = new StreamReader(stream);
                fileContent = await reader.ReadToEndAsync();
                break;
            }
            catch (FileNotFoundException)
            {
                return; // Файл исчез - тихо выходим
            }
            catch (IOException)
            {
                retries--;
                await Task.Delay(300);
            }
        }

        if (fileContent == null)
        {
            MoveToFailed(filePath, "Nie można uzyskać dostępu do pliku.");
            return;
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var orders = JsonSerializer.Deserialize<List<Order>>(fileContent, options);

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    await _pipeline.ProcessOrderAsync(order);
                }
            }

            MoveToProcessed(filePath);
        }
        catch (Exception ex)
        {
            MoveToFailed(filePath, ex.Message);
        }
    }

    private void MoveToProcessed(string filePath)
    {
        // Проверяем, существует ли еще файл
        if (!File.Exists(filePath)) return;
        
        var dest = Path.Combine(_inboxPath, "processed", Path.GetFileName(filePath));
        if (File.Exists(dest)) File.Delete(dest);
        File.Move(filePath, dest);
        System.Console.WriteLine($"[Watcher] Sukces! Plik przeniesiony do /processed.");
    }

    private void MoveToFailed(string filePath, string errorMsg)
    {
        // Проверяем, существует ли еще файл
        if (!File.Exists(filePath)) return;
        
        var fileName = Path.GetFileName(filePath);
        var dest = Path.Combine(_inboxPath, "failed", fileName);
        if (File.Exists(dest)) File.Delete(dest);
        File.Move(filePath, dest);
        File.WriteAllText(dest + ".error.txt", errorMsg);
        System.Console.WriteLine($"[Watcher] Błąd! Plik przeniesiony do /failed. Błąd: {errorMsg}");
    }

    public void Dispose()
    {
        _watcher.Created -= OnFileCreated;
        _watcher?.Dispose();
        _semaphore?.Dispose();
    }
}