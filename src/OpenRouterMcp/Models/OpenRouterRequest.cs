using System.Text.Json.Serialization;

namespace OpenRouterMcp.Models;

public class OpenRouterRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required OpenRouterMessage[] Messages { get; init; }

    [JsonPropertyName("modalities")]
    public required string[] Modalities { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("image_config")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenRouterImageConfig? ImageConfig { get; init; }

    [JsonPropertyName("audio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenRouterAudioConfig? Audio { get; init; }
}

public class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}

public class OpenRouterImageConfig
{
    [JsonPropertyName("aspect_ratio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AspectRatio { get; init; }

    [JsonPropertyName("image_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageSize { get; init; }
}

public class OpenRouterAudioConfig
{
    [JsonPropertyName("voice")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Voice { get; init; }

    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; init; }
}
