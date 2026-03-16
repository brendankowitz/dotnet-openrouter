using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using OpenRouterMcp.Commands;
using OpenRouterMcp.Services;

namespace OpenRouterMcp;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        await using var serviceProvider = services.BuildServiceProvider();

        var configService = serviceProvider.GetRequiredService<IConfigService>();
        configService.EnsureConfigDirectory();

        var rootCommand = new RootCommand("OpenRouter MCP Server - Dual-mode CLI and MCP integration for image and audio generation")
        {
            AuthCommand.Create(serviceProvider),
            ImageCommand.Create(serviceProvider),
            AudioCommand.Create(serviceProvider),
            McpCommand.Create(serviceProvider)
        };

        return await rootCommand.InvokeAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddHttpClient<IOpenRouterService, OpenRouterService>(client =>
        {
            client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
            client.Timeout = TimeSpan.FromMinutes(5);
        })
        .AddStandardResilienceHandler();
    }
}
