using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpenRouterMcp.Models;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Commands;

public class ImageCommand : Command
{
    private ImageCommand() : base("image", "Generate an image using OpenRouter")
    {
    }

    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new ImageCommand();

        var descriptionOption = new Option<string>(
            ["--description", "-d"],
            "Text prompt describing the image to generate")
        { IsRequired = true };

        var modelOption = new Option<string>(
            ["--model", "-m"],
            () => "google/gemini-2.5-flash-image-preview",
            "OpenRouter model ID for image generation");

        var outputOption = new Option<string>(
            ["--output", "-o"],
            () => ".",
            "Output directory for the generated image");

        var filenameOption = new Option<string?>(
            ["--filename", "-f"],
            "Output filename (default: generated-{timestamp}.png)");

        var aspectRatioOption = new Option<string>(
            ["--aspect-ratio", "-a"],
            () => "1:1",
            "Aspect ratio (1:1, 16:9, 4:3, 2:3, 3:2)");

        var sizeOption = new Option<string>(
            ["--size", "-s"],
            () => "1K",
            "Image size (0.5K, 1K, 2K, 4K)");

        var robotOption = new Option<bool>(
            ["--robot", "-r"],
            () => false,
            "Output as JSON for scripting");

        command.AddOption(descriptionOption);
        command.AddOption(modelOption);
        command.AddOption(outputOption);
        command.AddOption(filenameOption);
        command.AddOption(aspectRatioOption);
        command.AddOption(sizeOption);
        command.AddOption(robotOption);

        command.SetHandler(async (string description, string model, string output, string? filename,
            string aspectRatio, string size, bool robot) =>
        {
            var openRouterService = serviceProvider.GetRequiredService<IOpenRouterService>();
            await ExecuteAsync(openRouterService, description, model, output, filename, aspectRatio, size, robot);
        }, descriptionOption, modelOption, outputOption, filenameOption, aspectRatioOption, sizeOption, robotOption);

        return command;
    }

    private static async Task ExecuteAsync(
        IOpenRouterService openRouterService,
        string description,
        string model,
        string outputDir,
        string? filename,
        string aspectRatio,
        string imageSize,
        bool robot)
    {
        try
        {
            if (!robot)
                Console.WriteLine($"Generating image with {model}...");

            var config = new ImageConfig(aspectRatio, imageSize);
            var result = await openRouterService.GenerateImageAsync(description, model, config);

            // Determine file extension from mime type
            var extension = result.MimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/webp" => ".webp",
                _ => ".png"
            };

            filename ??= $"generated-{DateTime.Now:yyyyMMdd-HHmmss}{extension}";
            var outputPath = Path.GetFullPath(Path.Combine(outputDir, filename));

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(outputPath, result.Data);

            if (robot)
            {
                var output = new
                {
                    filePath = outputPath,
                    filename,
                    size = result.Data.Length,
                    mimeType = result.MimeType,
                    description = result.Description
                };
                Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                var sizeKb = result.Data.Length / 1024.0;
                Console.WriteLine($"Image saved to {outputPath} ({sizeKb:F1} KB)");
                if (!string.IsNullOrEmpty(result.Description))
                    Console.WriteLine($"Description: {result.Description}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Image generation failed: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
