using OpenRouterMcp.Models;

namespace OpenRouterMcp.Services;

public interface IOpenRouterService
{
    Task<ImageResult> GenerateImageAsync(
        string prompt,
        string model = "google/gemini-2.5-flash-image-preview",
        ImageConfig? config = null,
        CancellationToken ct = default);

    Task<AudioResult> GenerateAudioAsync(
        string prompt,
        string model = "openai/gpt-4o-audio-preview",
        AudioConfig? config = null,
        CancellationToken ct = default);
}
