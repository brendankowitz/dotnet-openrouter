using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Commands;

public class AuthCommand : Command
{
    private AuthCommand() : base("auth", "Configure OpenRouter API key authentication")
    {
    }

    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new AuthCommand();

        var keyOption = new Option<string>(
            ["--key", "-k"],
            "OpenRouter API key")
        { IsRequired = true };

        command.AddOption(keyOption);

        command.SetHandler(async (string key) =>
        {
            var configService = serviceProvider.GetRequiredService<IConfigService>();
            await ExecuteAsync(configService, key);
        }, keyOption);

        return command;
    }

    private static async Task ExecuteAsync(IConfigService configService, string apiKey)
    {
        try
        {
            await configService.SaveApiKeyAsync(apiKey);
            var configDir = configService.GetConfigDirectory();
            Console.WriteLine($"API key saved to {configDir}");
            Console.WriteLine("You can now use image and audio generation commands.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to save API key: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
