# Feature: dotnet-openrouter CLI & MCP Server

**Status**: Planning
**Created**: 2026-03-13

## Overview

A .NET 10 global tool and MCP server that connects to [OpenRouter](https://openrouter.ai/) to provide image generation and audio generation capabilities. The tool follows the same dual-mode architecture as `dotnet-gmail-mcp` — serving both as a CLI tool and as an MCP server for AI assistants.

## Goals

- Provide a `dotnet-openrouter` global tool installable via NuGet
- Support API key authentication with local config storage
- Enable image generation via OpenRouter-compatible models
- Enable audio generation via OpenRouter-compatible models
- Expose all capabilities as MCP tools for AI assistant integration
- Ship with polished README documentation in both the repo root and NuGet package

## Key Commands

| Command | Description |
|---------|-------------|
| `dotnet-openrouter auth --key <key>` | Store OpenRouter API key in local config |
| `dotnet-openrouter image --description "..." --model "..."` | Generate an image |
| `dotnet-openrouter audio --description "..." --model "..."` | Generate audio |
| `dotnet-openrouter mcp` | Start MCP server (stdio transport) |

## Investigations

| Investigation | Status | Summary |
|---------------|--------|---------|
| [openrouter-api-cli-mcp](investigations/openrouter-api-cli-mcp.md) | In Progress | Full CLI + MCP architecture following dotnet-gmail-mcp patterns |

## Non-Goals (for now)

- Chat/text completion commands (focus is on media generation)
- Streaming responses
- Multi-user / team key management
