# Investigation: OpenRouter API CLI & MCP Server

**Feature**: dotnet-openrouter-cli
**Status**: In Progress
**Created**: 2026-03-13

## Approach

Build a .NET 10 global tool (`dotnet-openrouter`) that mirrors the architecture of [`dotnet-gmail-mcp`](https://github.com/brendankowitz/dotnet-gmail-mcp) — a dual-mode CLI + MCP server with clean separation of commands, services, and MCP tools. The tool connects to the [OpenRouter API](https://openrouter.ai/) to provide image generation and audio generation via a single unified endpoint (`/api/v1/chat/completions`) with modality selection.

### Project Structure

```
dotnet-openrouter/
├── .github/
│   └── workflows/
│       ├── build.yml              # Reusable build template
│       ├── ci.yml                 # Main CI/CD pipeline
│       └── pr.yml                 # PR validation
├── .mcp/
│   └── server.json               # MCP server metadata
├── docs/
│   ├── features/                  # Feature investigations
│   └── MCP-CONFIGURATION.md      # MCP setup guide
├── src/
│   └── OpenRouterMcp/
│       ├── Commands/
│       │   ├── AuthCommand.cs     # API key storage
│       │   ├── ImageCommand.cs    # Image generation
│       │   ├── AudioCommand.cs    # Audio generation
│       │   └── McpCommand.cs      # Start MCP server
│       ├── Services/
│       │   ├── IConfigService.cs
│       │   ├── ConfigService.cs   # Key storage & config
│       │   ├── IOpenRouterService.cs
│       │   └── OpenRouterService.cs  # API client
│       ├── Models/
│       │   ├── OpenRouterRequest.cs
│       │   ├── OpenRouterResponse.cs
│       │   ├── ImageConfig.cs
│       │   └── AudioConfig.cs
│       ├── Mcp/
│       │   ├── OpenRouterMcpServer.cs
│       │   └── Tools/
│       │       ├── GenerateImageTool.cs
│       │       └── GenerateAudioTool.cs
│       ├── Program.cs
│       └── OpenRouterMcp.csproj
├── packages/                      # NuGet output
├── OpenRouterMcp.slnx
├── build.ps1
├── build.sh
├── GitVersion.yml
├── LICENSE
└── README.md
```

### OpenRouter API Integration

All requests go through a single endpoint. Modality selection determines output type.

**Base URL**: `https://openrouter.ai/api/v1/chat/completions`

**Authentication**: `Authorization: Bearer {API_KEY}` header

#### Image Generation Request

```json
{
  "model": "google/gemini-2.5-flash-image-preview",
  "messages": [{ "role": "user", "content": "A sunset over mountains" }],
  "modalities": ["image", "text"],
  "stream": false,
  "image_config": {
    "aspect_ratio": "16:9",
    "image_size": "2K"
  }
}
```

**Response** — images arrive as base64 data URLs in `choices[0].message.images[]`:

```json
{
  "choices": [{
    "message": {
      "role": "assistant",
      "content": "descriptive text",
      "images": [{
        "type": "image_url",
        "image_url": { "url": "data:image/png;base64,iVBORw0KGgo..." }
      }]
    }
  }]
}
```

#### Audio Generation Request

```json
{
  "model": "openai/gpt-4o-audio-preview",
  "messages": [{ "role": "user", "content": "Say hello in a friendly tone" }],
  "modalities": ["text", "audio"],
  "stream": true,
  "audio": {
    "voice": "alloy",
    "format": "mp3"
  }
}
```

**Response** — audio arrives as streamed SSE chunks in `choices[0].delta.audio`:

```json
{
  "choices": [{
    "delta": {
      "audio": {
        "data": "<base64-encoded audio chunk>",
        "transcript": "text content"
      }
    }
  }]
}
```

Audio output **requires streaming** (`stream: true`).

### CLI Commands

#### `dotnet-openrouter auth --key <api-key>`

Stores the API key in `~/.openrouter-mcp/config.json`. Simple key-based auth (no OAuth flow needed — OpenRouter uses bearer tokens).

```
$ dotnet-openrouter auth --key sk-or-v1-abc123...
API key saved to ~/.openrouter-mcp/config.json
```

#### `dotnet-openrouter image`

| Option | Alias | Required | Default | Description |
|--------|-------|----------|---------|-------------|
| `--description` | `-d` | Yes | — | Text prompt for image generation |
| `--model` | `-m` | No | `google/gemini-2.5-flash-image-preview` | OpenRouter model ID |
| `--output` | `-o` | No | `./` | Output directory |
| `--filename` | `-f` | No | `generated-{timestamp}.png` | Output filename |
| `--aspect-ratio` | `-a` | No | `1:1` | Aspect ratio (1:1, 16:9, 4:3, etc.) |
| `--size` | `-s` | No | `1K` | Image size (0.5K, 1K, 2K, 4K) |
| `--robot` | `-r` | No | `false` | JSON output for scripting |

```
$ dotnet-openrouter image -d "A cat wearing a top hat" -m "google/gemini-2.5-flash-image-preview" -a "16:9" -s "2K"
Image saved to ./generated-20260313-143022.png (1920x1080, 2.3 MB)
```

#### `dotnet-openrouter audio`

| Option | Alias | Required | Default | Description |
|--------|-------|----------|---------|-------------|
| `--description` | `-d` | Yes | — | Text prompt or content to speak |
| `--model` | `-m` | No | `openai/gpt-4o-audio-preview` | OpenRouter model ID |
| `--output` | `-o` | No | `./` | Output directory |
| `--filename` | `-f` | No | `generated-{timestamp}.mp3` | Output filename |
| `--voice` | `-v` | No | `alloy` | Voice (alloy, echo, fable, onyx, nova, shimmer) |
| `--format` | | No | `mp3` | Audio format (wav, mp3, flac, opus, pcm16) |
| `--robot` | `-r` | No | `false` | JSON output for scripting |

```
$ dotnet-openrouter audio -d "Welcome to our podcast" -v shimmer --format wav
Audio saved to ./generated-20260313-143055.wav (4.2s, 1.1 MB)
```

#### `dotnet-openrouter mcp`

Starts the MCP server on stdio transport (identical pattern to `dotnet-gmail mcp`). All logging goes to stderr to avoid corrupting the JSON-RPC protocol on stdout.

### MCP Tools

| Tool | Parameters | Description |
|------|-----------|-------------|
| `GenerateImage` | `description`, `model?`, `aspectRatio?`, `imageSize?`, `outputPath?` | Generate image and save to disk |
| `GenerateAudio` | `description`, `model?`, `voice?`, `format?`, `outputPath?` | Generate audio and save to disk |

### Key Services

#### `ConfigService`

- Config directory: `~/.openrouter-mcp/`
- Config file: `config.json` containing `{ "apiKey": "sk-or-v1-..." }`
- Unix: chmod 600 on config file
- Methods: `SaveApiKey(string key)`, `GetApiKey()`, `GetConfigPath()`

#### `OpenRouterService`

- Single `HttpClient` with base address `https://openrouter.ai/api/v1/`
- Bearer token from ConfigService
- Polly retry policy (exponential backoff for 429, 500, 503)
- Methods:
  - `GenerateImageAsync(string prompt, string model, ImageConfig? config)` → returns `byte[]` image data + metadata
  - `GenerateAudioAsync(string prompt, string model, AudioConfig? config)` → returns `byte[]` audio data + transcript

### NuGet Package Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>net10.0</TargetFrameworks>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>dotnet-openrouter</ToolCommandName>
    <PackageId>OpenRouterMcp</PackageId>
    <PackageType>McpServer</PackageType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
</Project>
```

### Dependencies

| Package | Purpose |
|---------|---------|
| `System.CommandLine` (2.0.0-beta4) | CLI framework |
| `ModelContextProtocol` (latest preview) | MCP server support |
| `Microsoft.Extensions.Hosting` | DI & hosting |
| `Microsoft.Extensions.Http` | HttpClientFactory |
| `Polly` | Resilience / retry policies |
| `System.Text.Json` | JSON serialization |
| `GitVersion.MsBuild` | Semantic versioning |

### README Structure

The root README (also packed into the NuGet package) should follow the dotnet-gmail-mcp README style:

1. **Hero section** — project name, one-line description, badges (NuGet, build, license)
2. **Features** — bullet list of capabilities
3. **Quick Start** — 3-step install + auth + first command
4. **Installation** — `dotnet tool install -g OpenRouterMcp`
5. **Authentication** — `dotnet-openrouter auth --key` with instructions to get an API key from openrouter.ai
6. **Usage** — subsections for each command with examples
   - Image generation with all options
   - Audio generation with all options
7. **MCP Setup** — Claude Desktop / VS Code configuration snippets
8. **Architecture** — brief diagram of CLI ↔ Services ↔ OpenRouter API
9. **Available Models** — link to OpenRouter model discovery + examples
10. **Development** — build from source instructions
11. **License** — MIT

## Tradeoffs

| Pros | Cons |
|------|------|
| Follows proven dotnet-gmail-mcp patterns — fast to build, consistent UX | Coupled to OpenRouter API; if their modalities spec changes, we adapt |
| Single endpoint for all modalities — simple HTTP client | Audio requires streaming which adds complexity vs. image (non-streamed) |
| .NET global tool = easy install via `dotnet tool install` | System.CommandLine is still in beta (2.0.0-beta4) |
| Key-based auth is simpler than OAuth (no browser flow needed) | API key stored in plaintext JSON (mitigated by file permissions) |
| MCP server enables AI assistant integration out of the box | MCP SDK is preview; API may change |
| Dual-mode (CLI + MCP) from a single binary | Two code paths to maintain (commands + MCP tools) but shared services layer minimizes duplication |
| Multi-target net10.0 aligns with latest .NET | net10.0 is preview as of early 2026 |

## Alignment

- [x] Follows architectural layering rules — Commands → Services → HTTP Client
- [x] Developer Experience — `dotnet tool install` + single auth command + immediate use
- [x] Specification compliance — uses OpenRouter's documented modalities API
- [x] Consistent with existing patterns — mirrors dotnet-gmail-mcp structure

## Evidence

### OpenRouter API Research

- **Unified endpoint**: All generation goes through `/api/v1/chat/completions` with `modalities` parameter controlling output type
- **Image generation**: Non-streamed, response includes `images[]` array with base64 data URLs
- **Audio generation**: Requires `stream: true`, response chunks include `delta.audio.data` (base64) and `delta.audio.transcript`
- **Image config options**: `aspect_ratio` (1:1, 16:9, 4:3, 2:3, 3:2, etc.), `image_size` (0.5K, 1K, 2K, 4K)
- **Audio config options**: `voice` (alloy, echo, fable, onyx, nova, shimmer), `format` (wav, mp3, flac, opus, pcm16)
- **Model discovery**: `GET /api/v1/models?output_modalities=image` or `?output_modalities=audio`
- **Authentication**: Simple bearer token in Authorization header

### dotnet-gmail-mcp Patterns (Prior Art)

- Dual-mode CLI + MCP from single binary — proven architecture
- `System.CommandLine` for CLI with `RootCommand` and subcommands
- `ModelContextProtocol` SDK with `[McpServerToolType]` / `[McpServerTool]` attributes
- Service layer with DI (ServiceCollection → singletons)
- Polly retry policies for HTTP resilience
- Config stored in `~/.{tool-name}/` directory
- stderr-only logging in MCP mode
- NuGet packaging as global tool with `PackageType=McpServer`
- GitVersion for semantic versioning
- GitHub Actions CI/CD with reusable build template

## Alternative Approaches Worth Investigating

1. **openai-sdk-wrapper** — Use the official OpenAI .NET SDK (which supports custom base URLs) instead of raw HttpClient. Simpler HTTP handling but less control over OpenRouter-specific features like `image_config`.

2. **plugin-based-modalities** — Instead of hardcoding image/audio commands, build a plugin system where new modalities can be added via configuration. More flexible but over-engineered for the current scope.

3. **docker-sidecar** — Package as a Docker container instead of a .NET global tool, with the MCP server exposed via SSE transport rather than stdio. Better for server deployments but worse for developer experience.

## Verdict

**Recommended approach.** This investigation mirrors the proven dotnet-gmail-mcp architecture with minimal changes — swapping OAuth for simple key auth, and Gmail API calls for OpenRouter HTTP requests. The unified OpenRouter endpoint simplifies the service layer. The main complexity is handling streamed audio responses, which is well-supported by .NET's `HttpClient` streaming capabilities. This approach provides the fastest path to a working tool with excellent developer experience.
