using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Mcp;

public static class OpenRouterMcpServer
{
    public static async Task RunAsync(
        IConfigService configService,
        IOpenRouterService openRouterService,
        CancellationToken ct = default)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddSingleton(configService);
        builder.Services.AddSingleton(openRouterService);

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();
        await host.RunAsync(ct);
    }
}
