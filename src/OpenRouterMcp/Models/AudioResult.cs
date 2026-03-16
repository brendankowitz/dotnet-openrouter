namespace OpenRouterMcp.Models;

public record AudioResult(
    byte[] Data,
    string? Transcript,
    string Format);
