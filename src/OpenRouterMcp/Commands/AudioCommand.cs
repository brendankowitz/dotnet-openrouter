using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpenRouterMcp.Models;
using OpenRouterMcp.Services;

namespace OpenRouterMcp.Commands;

public class AudioCommand : Command
{
    private AudioCommand() : base("audio", "Generate audio using OpenRouter")
    {
    }

    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new AudioCommand();

        var descriptionOption = new Option<string>(
            ["--description", "-d"],
            "Text prompt or content to convert to audio")
        { IsRequired = true };

        var modelOption = new Option<string>(
            ["--model", "-m"],
            () => "openai/gpt-4o-audio-preview",
            "OpenRouter model ID for audio generation");

        var outputOption = new Option<string>(
            ["--output", "-o"],
            () => ".",
            "Output directory for the generated audio");

        var filenameOption = new Option<string?>(
            ["--filename", "-f"],
            "Output filename (default: generated-{timestamp}.{format})");

        var voiceOption = new Option<string>(
            ["--voice", "-v"],
            () => "alloy",
            "Voice selection (alloy, echo, fable, onyx, nova, shimmer)");

        var formatOption = new Option<string>(
            "--format",
            () => "mp3",
            "Audio format (wav, mp3, flac, opus, pcm16)");

        var robotOption = new Option<bool>(
            ["--robot", "-r"],
            () => false,
            "Output as JSON for scripting");

        command.AddOption(descriptionOption);
        command.AddOption(modelOption);
        command.AddOption(outputOption);
        command.AddOption(filenameOption);
        command.AddOption(voiceOption);
        command.AddOption(formatOption);
        command.AddOption(robotOption);

        command.SetHandler(async (string description, string model, string output, string? filename,
            string voice, string format, bool robot) =>
        {
            var openRouterService = serviceProvider.GetRequiredService<IOpenRouterService>();
            await ExecuteAsync(openRouterService, description, model, output, filename, voice, format, robot);
        }, descriptionOption, modelOption, outputOption, filenameOption, voiceOption, formatOption, robotOption);

        return command;
    }

    private static async Task ExecuteAsync(
        IOpenRouterService openRouterService,
        string description,
        string model,
        string outputDir,
        string? filename,
        string voice,
        string format,
        bool robot)
    {
        try
        {
            if (!robot)
                Console.WriteLine($"Generating audio with {model} (voice: {voice})...");

            var config = new AudioConfig(voice, format);
            var result = await openRouterService.GenerateAudioAsync(description, model, config);

            filename ??= $"generated-{DateTime.Now:yyyyMMdd-HHmmss}.{format}";
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
                    format,
                    transcript = result.Transcript
                };
                Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                var sizeKb = result.Data.Length / 1024.0;
                Console.WriteLine($"Audio saved to {outputPath} ({sizeKb:F1} KB)");
                if (!string.IsNullOrEmpty(result.Transcript))
                    Console.WriteLine($"Transcript: {result.Transcript}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Audio generation failed: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
