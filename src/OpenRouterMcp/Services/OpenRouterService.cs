using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenRouterMcp.Models;

namespace OpenRouterMcp.Services;

public sealed class OpenRouterService(IConfigService configService, HttpClient httpClient) : IOpenRouterService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<ImageResult> GenerateImageAsync(
        string prompt,
        string model = "google/gemini-2.5-flash-image-preview",
        ImageConfig? config = null,
        CancellationToken ct = default)
    {
        await ConfigureAuthorizationAsync(ct);
        config ??= new ImageConfig();

        var request = new OpenRouterRequest
        {
            Model = model,
            Messages = [new OpenRouterMessage { Role = "user", Content = prompt }],
            Modalities = ["image", "text"],
            Stream = false,
            ImageConfig = new OpenRouterImageConfig
            {
                AspectRatio = config.AspectRatio,
                ImageSize = config.ImageSize
            }
        };

        var json = JsonSerializer.Serialize(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody)
            ?? throw new InvalidOperationException("Failed to deserialize OpenRouter response.");

        var choice = result.Choices.FirstOrDefault()
            ?? throw new InvalidOperationException("No choices returned from OpenRouter.");

        var imageOutput = choice.Message?.Images?.FirstOrDefault()
            ?? throw new InvalidOperationException("No image data returned from OpenRouter.");

        var dataUrl = imageOutput.ImageUrl?.Url
            ?? throw new InvalidOperationException("No image URL in response.");

        if (!dataUrl.StartsWith("data:", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected image URL format. Expected a data URL but got: {dataUrl[..Math.Min(80, dataUrl.Length)]}...");

        var semicolonIndex = dataUrl.IndexOf(';');
        var commaIndex = dataUrl.IndexOf(',');
        if (semicolonIndex < 0 || commaIndex < 0 || semicolonIndex >= commaIndex)
            throw new InvalidOperationException("Malformed data URL in OpenRouter image response.");

        var mimeType = dataUrl[5..semicolonIndex];
        var base64Data = dataUrl[(commaIndex + 1)..];
        var imageBytes = Convert.FromBase64String(base64Data);

        return new ImageResult(imageBytes, choice.Message?.Content ?? "", mimeType);
    }

    public async Task<AudioResult> GenerateAudioAsync(
        string prompt,
        string model = "openai/gpt-4o-audio-preview",
        AudioConfig? config = null,
        CancellationToken ct = default)
    {
        await ConfigureAuthorizationAsync(ct);
        config ??= new AudioConfig();

        var request = new OpenRouterRequest
        {
            Model = model,
            Messages = [new OpenRouterMessage { Role = "user", Content = prompt }],
            Modalities = ["text", "audio"],
            Stream = true,
            Audio = new OpenRouterAudioConfig
            {
                Voice = config.Voice,
                Format = config.Format
            }
        };

        var json = JsonSerializer.Serialize(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        var audioChunks = new List<byte[]>();
        var transcriptBuilder = new StringBuilder();

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var data = line[6..];
            if (data == "[DONE]")
                break;

            var chunk = JsonSerializer.Deserialize<OpenRouterStreamChunk>(data);
            var audioOutput = chunk?.Choices.FirstOrDefault()?.Delta?.Audio;

            if (audioOutput is null)
                continue;

            if (!string.IsNullOrEmpty(audioOutput.Data))
                audioChunks.Add(Convert.FromBase64String(audioOutput.Data));

            if (!string.IsNullOrEmpty(audioOutput.Transcript))
                transcriptBuilder.Append(audioOutput.Transcript);
        }

        var totalLength = audioChunks.Sum(c => c.Length);
        var audioData = new byte[totalLength];
        var offset = 0;
        foreach (var chunk in audioChunks)
        {
            Buffer.BlockCopy(chunk, 0, audioData, offset, chunk.Length);
            offset += chunk.Length;
        }

        return new AudioResult(
            audioData,
            transcriptBuilder.Length > 0 ? transcriptBuilder.ToString() : null,
            config.Format);
    }

    private async Task ConfigureAuthorizationAsync(CancellationToken ct)
    {
        var apiKey = await configService.GetApiKeyAsync(ct)
            ?? throw new InvalidOperationException(
                "No API key configured. Run 'dotnet-openrouter auth --key <your-key>' first.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }
}
