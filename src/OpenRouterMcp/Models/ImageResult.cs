namespace OpenRouterMcp.Models;

public record ImageResult(
    byte[] Data,
    string Description,
    string MimeType = "image/png");
