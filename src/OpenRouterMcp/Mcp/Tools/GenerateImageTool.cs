using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OpenRouterMcp.Models;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Mcp.Tools;

[McpServerToolType]
public static class GenerateImageTool
{
    [McpServerTool, Description("Generate an image using OpenRouter. Returns the file path and metadata of the generated image.")]
    public static async Task<string> GenerateImage(
        [Description("Text prompt describing the image to generate")] string description,
        IOpenRouterService openRouterService,
        [Description("OpenRouter model ID (default: google/gemini-2.5-flash-image-preview)")] string model = "google/gemini-2.5-flash-image-preview",
        [Description("Aspect ratio: 1:1, 16:9, 4:3, 2:3, 3:2")] string aspectRatio = "1:1",
        [Description("Image size: 0.5K, 1K, 2K, 4K")] string imageSize = "1K",
        [Description("Output file path (default: auto-generated in current directory)")] string? outputPath = null)
    {
        try
        {
            var config = new ImageConfig(aspectRatio, imageSize);
            var result = await openRouterService.GenerateImageAsync(description, model, config);

            var extension = result.MimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/webp" => ".webp",
                _ => ".png"
            };

            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                $"generated-{DateTime.Now:yyyyMMdd-HHmmss}{extension}");

            var resolvedPath = Path.GetFullPath(outputPath ?? defaultPath);
            var allowedBases = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.CurrentDirectory
            };
            if (!allowedBases.Any(b => resolvedPath.StartsWith(b, StringComparison.OrdinalIgnoreCase)))
                throw new UnauthorizedAccessException($"Output path must be within the user profile or current directory. Got: {resolvedPath}");

            outputPath = resolvedPath;

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(outputPath, result.Data);

            return JsonSerializer.Serialize(new
            {
                success = true,
                filePath = outputPath,
                size = result.Data.Length,
                mimeType = result.MimeType,
                description = result.Description
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }
}
