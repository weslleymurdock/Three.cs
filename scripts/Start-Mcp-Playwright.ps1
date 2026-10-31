[CmdletBinding()]
param(
    [int]$Port = 8931
)

$ErrorActionPreference = 'Stop'

# If an MCP server is already listening at http://localhost:8931/mcp, reuse it
try {
    $resp = Invoke-WebRequest -Uri "http://localhost:$Port/mcp" -Method Post -Body '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"probe","version":"1.0"}}}' -ContentType 'application/json' -Headers @{ "Accept" = "application/json, text/event-stream" } -TimeoutSec 2 -ErrorAction Stop
    if ($resp.StatusCode -eq 200) {
        Write-Host "Playwright MCP is already running at http://localhost:$Port/mcp"
        exit 0
    }
} catch {
    # Not running, proceed to start
}

if (-not (Get-Command node -ErrorAction SilentlyContinue)) { throw 'Node.js is required for Playwright MCP.' }
if (-not (Get-Command npx -ErrorAction SilentlyContinue)) { throw 'npx is required for Playwright MCP.' }

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$profile = Join-Path $repoRoot '.agents-runtime/playwright'
New-Item -ItemType Directory -Force -Path $profile | Out-Null

# npx installs/caches the package on first use. The browser profile is project-local and persistent.
& npx --yes @playwright/mcp@latest --port $Port --headless "--user-data-dir=$profile"
exit $LASTEXITCODE
