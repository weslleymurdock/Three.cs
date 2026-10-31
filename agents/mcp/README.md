# Agent MCP Environment

The project keeps MCP configuration and startup helpers under `agents/mcp/` and `scripts/` so local agents can reproduce the same tooling without committing secrets.

## Included MCPs

- **GitHub MCP** — repository, issue, pull-request and GitHub workflow context. Uses GitHub's official container image.
- **Filesystem MCP** — scoped workspace file access. Uses the official MCP filesystem image and mounts only the repository workspace.
- **Playwright MCP** — browser automation for validating Blazor/browser flows. Uses `npx @playwright/mcp@latest` and is installed automatically by `npx` when absent.

The GitHub and filesystem MCP containers are created once and then started/stopped as needed; they are not recreated on every agent session. Docker images are pulled on first initialization or when missing.

## Secrets

Copy `agents/mcp/.env.example` to `agents/mcp/.env` and set `GITHUB_PERSONAL_ACCESS_TOKEN` if token authentication is desired. `.env` is intentionally ignored by Git. OAuth can be used instead when supported by the MCP client.

## Initialization

Run from the repository root:

```powershell
./scripts/Initialize-Mcps.ps1
```

This verifies Docker, starts Docker Desktop when available, pulls required images, and creates persistent containers if they do not exist.

## Agent configuration

`agents/mcp/antigravity.mcp.json` is the project-owned MCP configuration template. Antigravity uses the `mcpServers` shape; copy/import this configuration into the Antigravity MCP settings rather than storing credentials in the repository.

The configuration intentionally points the GitHub and filesystem servers at the PowerShell startup scripts so container lifecycle is deterministic on Windows.