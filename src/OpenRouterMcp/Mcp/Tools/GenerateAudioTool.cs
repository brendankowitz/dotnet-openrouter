using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OpenRouterMcp.Models;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Mcp.Tools;

[McpServerToolType]
public static class GenerateAudioTool
{
    [McpServerTool, Description("Generate audio using OpenRouter. Returns the file path, metadata, and transcript of the generated audio.")]
    public static async Task<string> GenerateAudio(
        [Description("Text prompt or content to convert to audio")] string description,
        IOpenRouterService openRouterService,
        [Description("OpenRouter model ID (default: openai/gpt-4o-audio-preview)")] string model = "openai/gpt-4o-audio-preview",
        [Description("Voice: alloy, echo, fable, onyx, nova, shimmer")] string voice = "alloy",
        [Description("Audio format: wav, mp3, flac, opus, pcm16")] string format = "mp3",
        [Description("Output file path (default: auto-generated in Downloads)")] string? outputPath = null)
    {
        try
        {
            var config = new AudioConfig(voice, format);
            var result = await openRouterService.GenerateAudioAsync(description, model, config);

            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                $"generated-{DateTime.Now:yyyyMMdd-HHmmss}.{format}");

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
                format,
                transcript = result.Transcript
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
