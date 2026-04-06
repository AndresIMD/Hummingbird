using System.Text.Json;
using Hummingbird.Models;

namespace Hummingbird.Services;

public class DataService
{
    private readonly string _readingsPath;
    private readonly string _configPath;
    private List<GlucoseReading>? _readings;
    private AppConfig? _config;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true
    };

    public DataService()
    {
        var folder = FileSystem.AppDataDirectory;
        _readingsPath = Path.Combine(folder, "readings.json");
        _configPath = Path.Combine(folder, "config.json");
    }

    public async Task<List<GlucoseReading>> GetReadingsAsync()
    {
        if (_readings is not null)
            return _readings;

        if (File.Exists(_readingsPath))
        {
            var json = await File.ReadAllTextAsync(_readingsPath);
            _readings = JsonSerializer.Deserialize<List<GlucoseReading>>(json, s_jsonOptions) ?? [];
        }
        else
        {
            _readings = [];
        }

        return _readings;
    }

    public async Task SaveReadingAsync(GlucoseReading reading)
    {
        var readings = await GetReadingsAsync();
        readings.Add(reading);
        await PersistReadingsAsync();
    }

    public async Task DeleteReadingAsync(string id)
    {
        var readings = await GetReadingsAsync();
        readings.RemoveAll(r => r.Id == id);
        await PersistReadingsAsync();
    }

    private async Task PersistReadingsAsync()
    {
        var json = JsonSerializer.Serialize(_readings, s_jsonOptions);
        await File.WriteAllTextAsync(_readingsPath, json);
    }

    public async Task<AppConfig> GetConfigAsync()
    {
        if (_config is not null)
            return _config;

        if (File.Exists(_configPath))
        {
            var json = await File.ReadAllTextAsync(_configPath);
            _config = JsonSerializer.Deserialize<AppConfig>(json, s_jsonOptions) ?? new AppConfig();
        }
        else
        {
            _config = new AppConfig();
        }

        return _config;
    }

    public async Task SaveConfigAsync(AppConfig config)
    {
        _config = config;
        var json = JsonSerializer.Serialize(_config, s_jsonOptions);
        await File.WriteAllTextAsync(_configPath, json);
    }
}
