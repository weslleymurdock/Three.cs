[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Initialize-Mcps.ps1')
$container = 'threecs-mcp-filesystem'
$running = docker inspect -f '{{.State.Running}}' $container 2>$null
if ($running -eq 'true') {
    docker attach $container
} else {
    docker start -ai $container
}
