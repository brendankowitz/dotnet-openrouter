using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpenRouterMcp.Mcp;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Commands;

public class McpCommand : Command
{
    private McpCommand() : base("mcp", "Start MCP server for AI assistant integration via stdio")
    {
    }

    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new McpCommand();
        command.SetHandler(async () =>
        {
            var configService = serviceProvider.GetRequiredService<IConfigService>();
            var openRouterService = serviceProvider.GetRequiredService<IOpenRouterService>();
            await ExecuteAsync(configService, openRouterService);
        });
        return command;
    }

    private static async Task ExecuteAsync(
        IConfigService configService,
        IOpenRouterService openRouterService)
    {
        try
        {
            if (!configService.HasApiKey())
            {
                await Console.Error.WriteLineAsync("Error: No API key configured. Run 'dotnet-openrouter auth --key <your-key>' first.");
                Environment.ExitCode = 1;
                return;
            }

            await OpenRouterMcpServer.RunAsync(configService, openRouterService);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"MCP server error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
