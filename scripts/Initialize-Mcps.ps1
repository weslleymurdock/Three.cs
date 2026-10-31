[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$envFile = Join-Path $repoRoot 'agents/mcp/.env'
$githubImage = 'ghcr.io/github/github-mcp-server:latest'
$filesystemImage = 'mcp/filesystem:latest'
$githubContainer = 'threecs-mcp-github'
$filesystemContainer = 'threecs-mcp-filesystem'

function Assert-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

Assert-Command 'docker'

try { docker info *> $null } catch {
    $dockerDesktop = Join-Path $env:ProgramFiles 'Docker\Docker\Docker Desktop.exe'
    if (Test-Path $dockerDesktop) {
        Start-Process $dockerDesktop | Out-Null
    }
    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Seconds 2
        try { docker info *> $null; $ready = $true; break } catch { }
    }
    if (-not $ready) { throw 'Docker Desktop is not running and could not be started automatically.' }
}

Write-Host 'Pulling MCP images when necessary...'
docker image inspect $githubImage *> $null
if ($LASTEXITCODE -ne 0) { docker pull $githubImage }
docker image inspect $filesystemImage *> $null
if ($LASTEXITCODE -ne 0) { docker pull $filesystemImage }

$githubExists = docker container inspect $githubContainer 2>$null
if (-not $githubExists) {
    $args = @('create', '-i', '--name', $githubContainer, '-p', '127.0.0.1:8085:8085', '-e', 'GITHUB_OAUTH_CALLBACK_PORT=8085')
    if (Test-Path $envFile) { $args += @('--env-file', $envFile) }
    $args += $githubImage
    docker @args | Out-Null
}

$filesystemExists = docker container inspect $filesystemContainer 2>$null
if (-not $filesystemExists) {
    docker create -i --name $filesystemContainer --mount "type=bind,src=$repoRoot,dst=/projects/workspace" $filesystemImage /projects/workspace | Out-Null
}

Write-Host "MCP environment ready. Containers: $githubContainer, $filesystemContainer"
