using System.Runtime.InteropServices;
using System.Text.Json;

namespace OpenRouterMcp.Services;

public sealed class ConfigService : IConfigService
{
    private static readonly string ConfigDirectoryName = ".openrouter-mcp";
    private static readonly string ConfigFileName = "config.json";

    private string ConfigDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ConfigDirectoryName);

    private string ConfigFilePath => Path.Combine(ConfigDirectory, ConfigFileName);

    public void EnsureConfigDirectory()
    {
        Directory.CreateDirectory(ConfigDirectory);
    }

    public async Task SaveApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        EnsureConfigDirectory();

        var config = new Dictionary<string, string> { ["apiKey"] = apiKey };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

        var tmpPath = ConfigFilePath + ".tmp";
        await File.WriteAllTextAsync(tmpPath, json, ct);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.SetUnixFileMode(tmpPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }

        File.Move(tmpPath, ConfigFilePath, overwrite: true);
    }

    public async Task<string?> GetApiKeyAsync(CancellationToken ct = default)
    {
        if (!File.Exists(ConfigFilePath))
            return null;

        var json = await File.ReadAllTextAsync(ConfigFilePath, ct);
        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        return config?.GetValueOrDefault("apiKey");
    }

    public bool HasApiKey()
    {
        if (!File.Exists(ConfigFilePath)) return false;
        try
        {
            var json = File.ReadAllText(ConfigFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return !string.IsNullOrWhiteSpace(config?.GetValueOrDefault("apiKey"));
        }
        catch
        {
            return false;
        }
    }

    public string GetConfigDirectory() => ConfigDirectory;
}
