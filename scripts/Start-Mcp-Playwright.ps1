[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if (-not (Get-Command node -ErrorAction SilentlyContinue)) { throw 'Node.js is required for Playwright MCP.' }
if (-not (Get-Command npx -ErrorAction SilentlyContinue)) { throw 'npx is required for Playwright MCP.' }

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$profile = Join-Path $repoRoot '.agents-runtime/playwright'
New-Item -ItemType Directory -Force -Path $profile | Out-Null

# npx installs/caches the package on first use. The browser profile is project-local and persistent.
& npx --yes @playwright/mcp@latest --headless "--user-data-dir=$profile"
exit $LASTEXITCODE
