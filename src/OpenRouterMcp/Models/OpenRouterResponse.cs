using System.Text.Json.Serialization;

namespace OpenRouterMcp.Models;

public class OpenRouterResponse
{
    [JsonPropertyName("choices")]
    public OpenRouterChoice[] Choices { get; init; } = [];
}

public class OpenRouterChoice
{
    [JsonPropertyName("message")]
    public OpenRouterResponseMessage? Message { get; init; }
}

public class OpenRouterResponseMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = "assistant";

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("images")]
    public OpenRouterImageOutput[]? Images { get; init; }
}

public class OpenRouterImageOutput
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "image_url";

    [JsonPropertyName("image_url")]
    public OpenRouterImageUrl? ImageUrl { get; init; }
}

public class OpenRouterImageUrl
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

// Streaming response types for audio
public class OpenRouterStreamChunk
{
    [JsonPropertyName("choices")]
    public OpenRouterStreamChoice[] Choices { get; init; } = [];
}

public class OpenRouterStreamChoice
{
    [JsonPropertyName("delta")]
    public OpenRouterStreamDelta? Delta { get; init; }
}

public class OpenRouterStreamDelta
{
    [JsonPropertyName("audio")]
    public OpenRouterAudioOutput? Audio { get; init; }
}

public class OpenRouterAudioOutput
{
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    [JsonPropertyName("transcript")]
    public string? Transcript { get; init; }
}
