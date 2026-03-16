# 🎨 dotnet-openrouter

> A .NET 10 global tool and MCP server for AI-powered image and audio generation via [OpenRouter](https://openrouter.ai/)

[![NuGet](https://img.shields.io/nuget/v/OpenRouterMcp.svg?label=NuGet&logo=nuget)](https://www.nuget.org/packages/OpenRouterMcp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/OpenRouterMcp.svg?label=Downloads)](https://www.nuget.org/packages/OpenRouterMcp)
[![Build](https://github.com/brendankowitz/dotnet-openrouter/actions/workflows/ci.yml/badge.svg)](https://github.com/brendankowitz/dotnet-openrouter/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![MCP Enabled](https://img.shields.io/badge/MCP-Enabled-blue.svg)](https://modelcontextprotocol.io)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## 📖 Overview

`dotnet-openrouter` is a dual-mode .NET global tool that brings OpenRouter's media generation capabilities to your terminal and AI assistant workflow. It operates in two modes:

- **CLI mode** — generate images and audio directly from the command line with a simple, intuitive interface
- **MCP server mode** — expose generation capabilities as [Model Context Protocol](https://modelcontextprotocol.io) tools so AI assistants like Claude can call them autonomously

Both modes share the same service layer, so behaviour is identical whether you drive it from the shell or from an AI assistant.

OpenRouter provides a unified API endpoint (`/api/v1/chat/completions`) that routes to best-in-class models for each modality — Gemini for images, GPT-4o for audio — without requiring separate API accounts for each provider.

---

## ✨ Features

- 🖼️ **Image generation** — create images from text prompts with configurable aspect ratios and resolutions (up to 4K)
- 🔊 **Audio generation** — synthesise speech and audio from text with six voice options and multiple output formats
- 🤖 **MCP server** — expose all capabilities as MCP tools for seamless AI assistant integration
- 🔑 **Simple authentication** — store your OpenRouter API key once with a single command
- 📁 **Flexible output** — control output directory, filename, format, and dimensions
- 🔄 **Resilient HTTP** — automatic retry with exponential back-off for transient API errors
- 📦 **Zero-dependency install** — single `dotnet tool install` command, no runtime dependencies
- 🛠️ **Scriptable** — `--robot` flag outputs JSON for shell scripting and automation
- 🌐 **Model-agnostic** — specify any OpenRouter-compatible model by ID

---

## ⚡ Quick Start

```bash
# 1. Install the tool
dotnet tool install -g OpenRouterMcp

# 2. Save your OpenRouter API key
dotnet-openrouter auth --key sk-or-v1-...

# 3. Generate your first image
dotnet-openrouter image -d "A golden sunset over a mountain lake"
```

---

## 📦 Installation

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- An [OpenRouter account](https://openrouter.ai/) with API access

### Install the Global Tool

```bash
dotnet tool install -g OpenRouterMcp
```

To update to the latest version:

```bash
dotnet tool update -g OpenRouterMcp
```

To uninstall:

```bash
dotnet tool uninstall -g OpenRouterMcp
```

### Authenticate

Get your API key from [openrouter.ai/keys](https://openrouter.ai/keys), then save it:

```bash
dotnet-openrouter auth --key sk-or-v1-YOUR_API_KEY_HERE
```

The key is stored in `~/.openrouter-mcp/config.json` with restricted file permissions (chmod 600 on Unix).

---

## 🚀 Usage

### CLI Mode

#### Image Generation

Generate an image from a text description:

```bash
dotnet-openrouter image -d "A cat wearing a top hat, watercolour style"
```

All options:

```bash
dotnet-openrouter image \
  --description "A futuristic city skyline at night" \
  --model "google/gemini-2.5-flash-image-preview" \
  --aspect-ratio "16:9" \
  --size "2K" \
  --output "./images" \
  --filename "city-skyline.png"
```

| Option | Alias | Required | Default | Description |
|--------|-------|:--------:|---------|-------------|
| `--description` | `-d` | Yes | — | Text prompt for image generation |
| `--model` | `-m` | No | `google/gemini-2.5-flash-image-preview` | OpenRouter model ID |
| `--output` | `-o` | No | `./` | Output directory |
| `--filename` | `-f` | No | `generated-{timestamp}.png` | Output filename |
| `--aspect-ratio` | `-a` | No | `1:1` | Aspect ratio (`1:1`, `16:9`, `4:3`, `2:3`, `3:2`) |
| `--size` | `-s` | No | `1K` | Image resolution (`0.5K`, `1K`, `2K`, `4K`) |
| `--robot` | `-r` | No | `false` | Output JSON for scripting |

Example output:

```
Image saved to ./generated-20260313-143022.png (1920x1080, 2.3 MB)
```

With `--robot`:

```json
{
  "path": "./generated-20260313-143022.png",
  "width": 1920,
  "height": 1080,
  "bytes": 2411724
}
```

#### Audio Generation

Generate audio from a text description:

```bash
dotnet-openrouter audio -d "Welcome to our podcast. Today we explore the future of AI."
```

All options:

```bash
dotnet-openrouter audio \
  --description "Say hello in a warm, friendly tone" \
  --model "openai/gpt-4o-audio-preview" \
  --voice shimmer \
  --format wav \
  --output "./audio" \
  --filename "greeting.wav"
```

| Option | Alias | Required | Default | Description |
|--------|-------|:--------:|---------|-------------|
| `--description` | `-d` | Yes | — | Text prompt or content to speak |
| `--model` | `-m` | No | `openai/gpt-4o-audio-preview` | OpenRouter model ID |
| `--output` | `-o` | No | `./` | Output directory |
| `--filename` | `-f` | No | `generated-{timestamp}.mp3` | Output filename |
| `--voice` | `-v` | No | `alloy` | Voice character (see below) |
| `--format` | — | No | `mp3` | Audio format (`wav`, `mp3`, `flac`, `opus`, `pcm16`) |
| `--robot` | `-r` | No | `false` | Output JSON for scripting |

Available voices:

| Voice | Character |
|-------|-----------|
| `alloy` | Neutral, balanced |
| `echo` | Deep, resonant |
| `fable` | Warm, narrative |
| `onyx` | Authoritative |
| `nova` | Energetic, bright |
| `shimmer` | Soft, expressive |

Example output:

```
Audio saved to ./generated-20260313-143055.wav (4.2s, 1.1 MB)
```

#### Authentication Management

Store your API key:

```bash
dotnet-openrouter auth --key sk-or-v1-abc123...
```

Output:

```
API key saved to ~/.openrouter-mcp/config.json
```

---

### MCP Server Mode

Start the MCP server on stdio transport (used by AI assistants):

```bash
dotnet-openrouter mcp
```

All logging is written to stderr; stdout carries only the JSON-RPC protocol stream, so MCP hosts can safely parse it.

#### Claude Desktop Configuration

Add the server to your Claude Desktop configuration file.

**macOS** — `~/Library/Application Support/Claude/claude_desktop_config.json`

**Windows** — `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "openrouter": {
      "command": "dotnet-openrouter",
      "args": ["mcp"],
      "env": {
        "OPENROUTER_API_KEY": "sk-or-v1-YOUR_KEY_HERE"
      }
    }
  }
}
```

Alternatively, if you have authenticated via `dotnet-openrouter auth`, you can omit the `env` block — the tool reads the key from `~/.openrouter-mcp/config.json` automatically.

#### VS Code / GitHub Copilot Configuration

Add to your VS Code `settings.json` or workspace `.vscode/mcp.json`:

```json
{
  "mcp": {
    "servers": {
      "openrouter": {
        "command": "dotnet-openrouter",
        "args": ["mcp"],
        "env": {
          "OPENROUTER_API_KEY": "sk-or-v1-YOUR_KEY_HERE"
        }
      }
    }
  }
}
```

Once configured, ask your AI assistant to generate images or audio naturally:

> "Generate a landscape image of a misty forest at dawn in a 16:9 format and save it to my Downloads folder."

> "Create a short audio clip of a friendly welcome message using the shimmer voice."

See [docs/MCP-CONFIGURATION.md](docs/MCP-CONFIGURATION.md) for a complete setup guide covering all MCP clients.

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────┐
│           OpenRouter MCP Server                 │
│                                                 │
│  ┌──────────────┐         ┌─────────────────┐  │
│  │   CLI Mode   │         │   MCP Mode      │  │
│  │              │         │                 │  │
│  │  • auth      │         │  • stdio server │  │
│  │  • image     │         │  • tool calls   │  │
│  │  • audio     │         │  • JSON-RPC     │  │
│  └──────┬───────┘         └────────┬────────┘  │
│         │                          │           │
│         └──────────┬───────────────┘           │
│                    │                           │
│         ┌──────────▼──────────┐                │
│         │  Shared Services    │                │
│         │                     │                │
│         │  • ConfigService    │                │
│         │  • OpenRouterService│                │
│         └──────────┬──────────┘                │
│                    │                           │
│         ┌──────────▼──────────┐                │
│         │  OpenRouter API v1  │                │
│         │  (chat/completions) │                │
│         └─────────────────────┘                │
└─────────────────────────────────────────────────┘
```

The CLI commands and MCP tools are thin wrappers over a shared service layer. Both paths call identical service methods, so behaviour is consistent regardless of how the tool is invoked. The `OpenRouterService` handles all HTTP communication, retry logic, and response parsing; `ConfigService` manages API key storage and configuration.

---

## 🔐 Authentication & Security

### API Key Storage

The API key is stored at `~/.openrouter-mcp/config.json`:

```json
{
  "apiKey": "sk-or-v1-..."
}
```

On Unix/macOS the config directory and file are created with `chmod 700` and `chmod 600` respectively, preventing other users from reading the key.

### Environment Variable

As an alternative to file-based storage, set the `OPENROUTER_API_KEY` environment variable. This takes precedence over the config file, making it suitable for CI/CD pipelines:

```bash
export OPENROUTER_API_KEY=sk-or-v1-...
dotnet-openrouter image -d "A test image"
```

### Best Practices

- Do not commit `~/.openrouter-mcp/config.json` to version control
- In production or CI environments, prefer `OPENROUTER_API_KEY` over file storage
- Rotate your OpenRouter API key if it is ever exposed
- OpenRouter API keys are scoped per account — create a dedicated key for each tool or deployment

---

## 🎛️ Available Models

OpenRouter aggregates models from multiple providers. Browse the full catalogue at [openrouter.ai/models](https://openrouter.ai/models).

Filter by modality:

- **Image models** — [openrouter.ai/models?output_modalities=image](https://openrouter.ai/models?output_modalities=image)
- **Audio models** — [openrouter.ai/models?output_modalities=audio](https://openrouter.ai/models?output_modalities=audio)

### Notable Image Models

| Model ID | Provider | Notes |
|----------|----------|-------|
| `google/gemini-2.5-flash-image-preview` | Google | Default — fast, high quality |
| `google/gemini-2.0-flash-exp:image` | Google | Experimental variant |

### Notable Audio Models

| Model ID | Provider | Notes |
|----------|----------|-------|
| `openai/gpt-4o-audio-preview` | OpenAI | Default — six voices, multiple formats |
| `openai/gpt-4o-mini-audio-preview` | OpenAI | Faster, lower cost |

---

## 🛠️ MCP Tools Reference

These tools are exposed when running in MCP server mode (`dotnet-openrouter mcp`).

### `GenerateImage`

Generate an image from a text description and save it to disk.

| Parameter | Type | Required | Default | Description |
|-----------|------|:--------:|---------|-------------|
| `description` | `string` | Yes | — | Text prompt describing the image to generate |
| `model` | `string` | No | `google/gemini-2.5-flash-image-preview` | OpenRouter model ID |
| `aspectRatio` | `string` | No | `1:1` | Aspect ratio (`1:1`, `16:9`, `4:3`, `2:3`, `3:2`) |
| `imageSize` | `string` | No | `1K` | Output resolution (`0.5K`, `1K`, `2K`, `4K`) |
| `outputPath` | `string` | No | `./` | Directory or full path for the saved file |

Example tool call (JSON-RPC):

```json
{
  "tool": "GenerateImage",
  "arguments": {
    "description": "A serene Japanese garden in spring, cherry blossoms falling",
    "model": "google/gemini-2.5-flash-image-preview",
    "aspectRatio": "16:9",
    "imageSize": "2K",
    "outputPath": "/Users/alice/Pictures"
  }
}
```

### `GenerateAudio`

Generate audio from a text description and save it to disk.

| Parameter | Type | Required | Default | Description |
|-----------|------|:--------:|---------|-------------|
| `description` | `string` | Yes | — | Text prompt or content to synthesise |
| `model` | `string` | No | `openai/gpt-4o-audio-preview` | OpenRouter model ID |
| `voice` | `string` | No | `alloy` | Voice character (`alloy`, `echo`, `fable`, `onyx`, `nova`, `shimmer`) |
| `format` | `string` | No | `mp3` | Output format (`wav`, `mp3`, `flac`, `opus`, `pcm16`) |
| `outputPath` | `string` | No | `./` | Directory or full path for the saved file |

Example tool call (JSON-RPC):

```json
{
  "tool": "GenerateAudio",
  "arguments": {
    "description": "Narrate a short introduction: Welcome to the OpenRouter podcast.",
    "model": "openai/gpt-4o-audio-preview",
    "voice": "shimmer",
    "format": "wav",
    "outputPath": "/Users/alice/Audio"
  }
}
```

---

## 💻 Development

### Clone and Build

```bash
git clone https://github.com/brendankowitz/dotnet-openrouter.git
cd dotnet-openrouter

# Build
dotnet build

# Run tests
dotnet test

# Pack NuGet package
dotnet pack src/OpenRouterMcp/OpenRouterMcp.csproj --configuration Release --output ./packages
```

### Install from Local Package

```bash
dotnet tool install -g OpenRouterMcp --add-source ./packages
```

### Run Without Installing

```bash
dotnet run --project src/OpenRouterMcp -- image -d "A test image"
dotnet run --project src/OpenRouterMcp -- mcp
```

### Project Structure

```
dotnet-openrouter/
├── src/
│   └── OpenRouterMcp/
│       ├── Commands/           # CLI command implementations
│       │   ├── AuthCommand.cs
│       │   ├── ImageCommand.cs
│       │   ├── AudioCommand.cs
│       │   └── McpCommand.cs
│       ├── Services/           # Business logic layer
│       │   ├── IConfigService.cs
│       │   ├── ConfigService.cs
│       │   ├── IOpenRouterService.cs
│       │   └── OpenRouterService.cs
│       ├── Models/             # Data structures
│       │   ├── OpenRouterRequest.cs
│       │   ├── OpenRouterResponse.cs
│       │   ├── ImageConfig.cs
│       │   ├── ImageResult.cs
│       │   ├── AudioConfig.cs
│       │   └── AudioResult.cs
│       ├── Mcp/                # MCP server and tools
│       │   ├── OpenRouterMcpServer.cs
│       │   └── Tools/
│       │       ├── GenerateImageTool.cs
│       │       └── GenerateAudioTool.cs
│       ├── Program.cs          # Entry point and DI setup
│       └── OpenRouterMcp.csproj
├── docs/
│   ├── MCP-CONFIGURATION.md   # MCP setup guide
│   └── features/              # Feature investigations
├── .github/
│   └── workflows/
│       ├── build.yml           # Reusable build template
│       ├── ci.yml              # CI/CD pipeline
│       └── pr.yml              # PR validation
├── .mcp/
│   └── server.json             # MCP server metadata
├── packages/                   # NuGet output (gitignored)
├── OpenRouterMcp.slnx
├── build.ps1
├── build.sh
├── GitVersion.yml
├── LICENSE
└── README.md
```

### Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `System.CommandLine` | 2.0.0-beta4 | CLI framework |
| `ModelContextProtocol` | 0.8.0-preview.1 | MCP server support |
| `Microsoft.Extensions.Hosting` | 9.0.0 | Dependency injection and hosting |
| `Microsoft.Extensions.Http` | 9.0.0 | HttpClientFactory |
| `Microsoft.Extensions.Http.Resilience` | 9.0.0 | Retry policies |
| `GitVersion.MsBuild` | 6.0.5 | Semantic versioning |

---

## 🤝 Contributing

Contributions are welcome. Please follow these steps:

1. Fork the repository at [github.com/brendankowitz/dotnet-openrouter](https://github.com/brendankowitz/dotnet-openrouter)
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes, ensuring all existing tests pass
4. Add tests for new functionality
5. Submit a pull request with a clear description of the change

For significant changes, please open an issue first to discuss the approach.

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## 🔗 Related Projects

- [dotnet-gmail-mcp](https://github.com/brendankowitz/dotnet-gmail-mcp) — the sibling project that inspired this architecture; provides Gmail integration as a .NET global tool and MCP server
- [OpenRouter](https://openrouter.ai/) — the unified API gateway this tool connects to
- [Model Context Protocol](https://modelcontextprotocol.io) — the open standard for AI assistant tool integration
- [ModelContextProtocol .NET SDK](https://github.com/modelcontextprotocol/csharp-sdk) — the SDK used to implement the MCP server

---

<p align="center">
  Built with .NET 10 · Powered by <a href="https://openrouter.ai">OpenRouter</a> · MCP Enabled
</p>
