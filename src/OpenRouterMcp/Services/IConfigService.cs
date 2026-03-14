namespace OpenRouterMcp.Services;

public interface IConfigService
{
    void EnsureConfigDirectory();
    Task SaveApiKeyAsync(string apiKey, CancellationToken ct = default);
    Task<string?> GetApiKeyAsync(CancellationToken ct = default);
    bool HasApiKey();
    string GetConfigDirectory();
}
