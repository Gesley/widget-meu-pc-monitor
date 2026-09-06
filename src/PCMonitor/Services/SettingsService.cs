using System.IO;
using System.Text.Json;
using PCMonitor.Models;

namespace PCMonitor.Services;

public static class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PCMonitor",
        "settings.json");

    private static readonly object Sync = new();

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("interactionMode", out _))
            {
                settings.InteractionMode = true;
            }

            return settings;
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao ler configurações.", ex);
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            lock (Sync)
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, JsonOptions));
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao salvar configurações.", ex);
        }
    }
}
