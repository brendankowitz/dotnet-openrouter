# MCP Configuration Guide

This guide covers everything you need to connect `dotnet-openrouter` to your AI assistant as a Model Context Protocol (MCP) server. Once configured, your assistant can generate images and audio on your behalf without you needing to run any commands manually.

---

## Table of Contents

1. [How It Works](#how-it-works)
2. [Prerequisites](#prerequisites)
3. [Claude Desktop Setup](#claude-desktop-setup)
   - [macOS](#macos)
   - [Windows](#windows)
4. [VS Code with GitHub Copilot](#vs-code-with-github-copilot)
5. [VS Code with Cline](#vs-code-with-cline)
6. [Automatic Configuration from NuGet](#automatic-configuration-from-nuget)
7. [Authentication Options](#authentication-options)
   - [File-based (recommended for local use)](#file-based-recommended-for-local-use)
   - [Environment variable (recommended for CI/CD)](#environment-variable-recommended-for-cicd)
8. [Troubleshooting](#troubleshooting)
9. [Example AI Assistant Interactions](#example-ai-assistant-interactions)

---

## How It Works

`dotnet-openrouter mcp` starts a JSON-RPC server over stdio. The MCP host (Claude Desktop, VS Code, etc.) launches the process and communicates with it by writing to its stdin and reading from its stdout. All diagnostic logging goes to stderr so it does not interfere with the protocol stream.

```
MCP Host (Claude Desktop / VS Code)
        │
        │  stdin  ──►  dotnet-openrouter mcp
        │  stdout ◄──  dotnet-openrouter mcp
        │  stderr       (logs, ignored by host)
```

The server exposes two MCP tools:

| Tool | What it does |
|------|-------------|
| `GenerateImage` | Generates an image from a text description and saves it to disk |
| `GenerateAudio` | Generates audio from a text description and saves it to disk |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- `OpenRouterMcp` global tool installed: `dotnet tool install -g OpenRouterMcp`
- An [OpenRouter API key](https://openrouter.ai/keys)

Verify your installation:

```bash
dotnet-openrouter --version
```

---

## Claude Desktop Setup

### macOS

The Claude Desktop configuration file is at:

```
~/Library/Application Support/Claude/claude_desktop_config.json
```

Open it (create it if it does not exist) and add the `openrouter` entry to `mcpServers`:

```json
{
  "mcpServers": {
    "openrouter": {
      "command": "dotnet-openrouter",
      "args": ["mcp"]
    }
  }
}
```

If you have not authenticated via `dotnet-openrouter auth`, pass your key as an environment variable instead:

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

After saving the file, restart Claude Desktop. You should see a tools icon appear in the chat input area indicating MCP tools are available.

### Windows

The Claude Desktop configuration file is at:

```
%APPDATA%\Claude\claude_desktop_config.json
```

Which typically resolves to:

```
C:\Users\YourName\AppData\Roaming\Claude\claude_desktop_config.json
```

The configuration format is identical to macOS:

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

> **Note for Windows users:** If `dotnet-openrouter` is not on your `PATH` when Claude Desktop launches, use the full path to the executable. The .NET global tools directory is typically `%USERPROFILE%\.dotnet\tools\dotnet-openrouter.exe`. Replace `"command": "dotnet-openrouter"` with the full path:
>
> ```json
> "command": "C:\\Users\\YourName\\.dotnet\\tools\\dotnet-openrouter.exe"
> ```

After saving the file, restart Claude Desktop.

---

## VS Code with GitHub Copilot

VS Code supports MCP servers through its built-in Copilot extension (VS Code 1.99+). You can configure the server at the user level (applies to all workspaces) or at the workspace level.

### User-level configuration

Open VS Code settings (`Ctrl+Shift+P` → "Open User Settings (JSON)") and add:

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

### Workspace-level configuration

Create or edit `.vscode/mcp.json` in your workspace root:

```json
{
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
```

> **Security note:** Do not commit `.vscode/mcp.json` containing your API key to version control. Add it to `.gitignore`, or omit the `env` block and rely on the file-based auth described in [Authentication Options](#authentication-options).

After saving, reload the VS Code window. MCP tools will be available in the Copilot chat panel under the tools icon.

---

## VS Code with Cline

[Cline](https://marketplace.visualstudio.com/items?itemName=saoudrizwan.claude-dev) is a popular open-source AI coding assistant for VS Code that supports MCP.

Open the Cline sidebar, click the MCP icon (plug symbol), then click "Edit MCP Settings". Add the following to the configuration JSON:

```json
{
  "mcpServers": {
    "openrouter": {
      "command": "dotnet-openrouter",
      "args": ["mcp"],
      "env": {
        "OPENROUTER_API_KEY": "sk-or-v1-YOUR_KEY_HERE"
      },
      "disabled": false,
      "autoApprove": []
    }
  }
}
```

Save and Cline will connect to the server automatically. You will see `openrouter` listed under "Connected MCP Servers" in the Cline panel.

---

## Automatic Configuration from NuGet

The `OpenRouterMcp` NuGet package ships with a `.mcp/server.json` file that describes the MCP server. MCP-aware tools can use this metadata to configure the server automatically without manual JSON editing.

The bundled metadata is located at `.mcp/server.json` in the package and looks like this:

```json
{
  "$schema": "https://static.modelcontextprotocol.io/schemas/2025-10-17/server.schema.json",
  "name": "io.github.brendankowitz/dotnet-openrouter",
  "description": "Dual-mode OpenRouter integration tool supporting both direct CLI operations and MCP server mode for AI assistants.",
  "packages": [
    {
      "registryType": "nuget",
      "registryBaseUrl": "https://api.nuget.org",
      "identifier": "OpenRouterMcp",
      "transport": {
        "type": "stdio"
      },
      "packageArguments": ["mcp"]
    }
  ],
  "repository": {
    "url": "https://github.com/brendankowitz/dotnet-openrouter",
    "source": "github"
  }
}
```

Tools that support the MCP package registry (such as Claude Desktop's forthcoming one-click install flow) can install and configure the server directly from NuGet using this metadata.

---

## Authentication Options

The tool resolves your OpenRouter API key from two sources, in this order:

1. **`OPENROUTER_API_KEY` environment variable** — checked first; takes precedence
2. **`~/.openrouter-mcp/config.json`** — used if the environment variable is not set

### File-based (recommended for local use)

Run the auth command once and the key is stored securely on disk:

```bash
dotnet-openrouter auth --key sk-or-v1-YOUR_KEY_HERE
```

The key is written to `~/.openrouter-mcp/config.json`:

```json
{
  "apiKey": "sk-or-v1-..."
}
```

On Unix/macOS, the directory is created with `chmod 700` and the file with `chmod 600`. This means only your user account can read or write the key.

With file-based auth in place, your MCP configuration does not need to include the key at all:

```json
{
  "mcpServers": {
    "openrouter": {
      "command": "dotnet-openrouter",
      "args": ["mcp"]
    }
  }
}
```

### Environment variable (recommended for CI/CD)

Set the environment variable before launching the MCP host, or include it in the `env` block of your MCP configuration:

```bash
# Shell session
export OPENROUTER_API_KEY=sk-or-v1-...
dotnet-openrouter image -d "A test image"
```

```json
// MCP host configuration
{
  "env": {
    "OPENROUTER_API_KEY": "sk-or-v1-YOUR_KEY_HERE"
  }
}
```

In GitHub Actions or other CI systems, store the key as a secret and inject it as an environment variable rather than committing it to configuration files.

---

## Troubleshooting

### The MCP server does not appear in my AI assistant

**Check the tool is installed:**

```bash
dotnet-openrouter --version
```

If this command is not found, the tool is not on your `PATH`. Run `dotnet tool install -g OpenRouterMcp` and ensure `~/.dotnet/tools` (Unix) or `%USERPROFILE%\.dotnet\tools` (Windows) is on your `PATH`.

**Check the config file syntax:**

MCP configuration files must be valid JSON. A trailing comma or missing brace will prevent the host from loading the configuration. Validate your JSON with a linter or paste it into [jsonlint.com](https://jsonlint.com).

**Restart the MCP host:**

Claude Desktop and VS Code cache MCP server configurations. After editing the config file, fully restart the application (not just reload the window).

---

### Authentication error: 401 Unauthorized

The tool cannot find a valid API key. Verify one of the following:

1. You have run `dotnet-openrouter auth --key sk-or-v1-...` and the file `~/.openrouter-mcp/config.json` exists and contains a valid key.
2. The `OPENROUTER_API_KEY` environment variable is set and visible to the process launched by your MCP host.

Check the config file:

```bash
# Unix/macOS
cat ~/.openrouter-mcp/config.json

# Windows (PowerShell)
Get-Content "$env:USERPROFILE\.openrouter-mcp\config.json"
```

Verify your key is valid by making a direct API call:

```bash
curl https://openrouter.ai/api/v1/models \
  -H "Authorization: Bearer sk-or-v1-YOUR_KEY_HERE"
```

---

### Rate limit error: 429 Too Many Requests

The `OpenRouterService` automatically retries with exponential back-off on 429 responses. If you are hitting rate limits consistently, consider:

- Upgrading your OpenRouter plan at [openrouter.ai/settings/billing](https://openrouter.ai/settings/billing)
- Using a lower-cost model variant (e.g., `google/gemini-2.0-flash-exp:image` instead of `google/gemini-2.5-flash-image-preview`)

---

### Generated files are not saved where expected

The `outputPath` parameter (MCP) and `--output` option (CLI) accept both directory paths and full file paths.

- If you provide a directory, the tool generates a timestamped filename automatically.
- If you provide a full path including a filename, it is used as-is.
- Relative paths are resolved relative to the process working directory, which may not be what you expect when invoked from an MCP host. Use absolute paths to be certain:

```json
{
  "tool": "GenerateImage",
  "arguments": {
    "description": "A mountain lake",
    "outputPath": "/Users/alice/Pictures/generated"
  }
}
```

---

### MCP server crashes immediately on startup

Check stderr output from the process. The most common causes are:

- **.NET runtime not found** — ensure .NET 10 is installed: `dotnet --version`
- **Missing config directory** — the tool creates `~/.openrouter-mcp/` automatically on first run, but if the home directory is unavailable (e.g., in a containerised environment), this will fail
- **Permission denied on config file** — on Unix, check that `~/.openrouter-mcp/config.json` is readable by the current user: `ls -la ~/.openrouter-mcp/`

To capture stderr when testing manually:

```bash
dotnet-openrouter mcp 2>mcp-errors.log
```

---

### Tools are not listed when I ask the assistant

Some MCP hosts require you to explicitly enable or approve tools from new servers. In Claude Desktop, click the tools icon in the chat input area and confirm the `openrouter` server's tools are enabled. In VS Code Copilot, open the Copilot chat panel, click the tools icon, and tick `GenerateImage` and `GenerateAudio`.

---

## Example AI Assistant Interactions

Once the MCP server is connected, you can give natural-language instructions. Here are examples of what you can ask:

### Image generation examples

> "Generate a photorealistic image of a red fox sitting in autumn leaves. Use a 16:9 aspect ratio and 2K resolution. Save it to my Desktop."

> "Create three variations of a minimalist logo for a coffee shop. Square format, 1K resolution. Put them in ~/Documents/logos."

> "Make an image of a futuristic cityscape at night with neon lights, 4:3 aspect ratio."

### Audio generation examples

> "Record a short podcast intro: 'Welcome to the AI Weekly podcast. I am your host, and today we explore the latest in machine learning.' Use the shimmer voice and save it as a WAV file."

> "Generate an audio greeting for a customer support IVR system using a warm, professional tone. The text is: 'Thank you for calling. Your call is important to us. Please hold while we connect you.'"

> "Create a narration in the fable voice: [paste your script here]. Save it as MP3 to ~/Audio/narration.mp3."

### Combined workflow examples

> "I am writing a blog post about ocean conservation. Generate a header image showing a vibrant coral reef in 16:9 format, and also record a short 30-second audio teaser narrated in the nova voice. Save both to ~/blog/ocean-post/."

The assistant will call `GenerateImage` and `GenerateAudio` in sequence (or in parallel, depending on the host) and report the saved file paths when complete.

---

For further help, open an issue at [github.com/brendankowitz/dotnet-openrouter/issues](https://github.com/brendankowitz/dotnet-openrouter/issues).
