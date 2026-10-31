<#
.SYNOPSIS
    Evaluates test results and code coverage metrics against the repository quality gate (>= 90%).

.DESCRIPTION
    Parses test-results.trx and artifacts/coverage/Summary.txt.
    Outputs a structured summary to $GITHUB_STEP_SUMMARY (if available) and stdout.
    Exits with code 1 if any metric is below 90% or if any test failed; exits with code 0 otherwise.

.PARAMETER MinLineCoverage
    Minimum required line coverage percentage. Defaults to 90.

.PARAMETER MinMethodCoverage
    Minimum required method coverage percentage. Defaults to 90.

.PARAMETER MinBranchCoverage
    Minimum required branch coverage percentage. Defaults to 90.
#>
[CmdletBinding()]
param(
    [double]$MinLineCoverage = 90.0,
    [double]$MinMethodCoverage = 90.0,
    [double]$MinBranchCoverage = 90.0
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$testResultsDir = Join-Path $repoRoot 'artifacts/test-results'
$coverageDir = Join-Path $repoRoot 'artifacts/coverage'
$summaryTxt = Join-Path $coverageDir 'Summary.txt'
$summaryMd = Join-Path $coverageDir 'Summary.md'

# 1. Parse TRX test results
$trxFiles = Get-ChildItem -Path $testResultsDir -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue
if (-not $trxFiles) {
    throw "No .trx test result file found in '$testResultsDir'."
}
$trxFile = $trxFiles[0]
[xml]$trx = Get-Content $trxFile.FullName

$counters = $trx.TestRun.ResultSummary.Counters
$totalTests = [int]$counters.total
$passedTests = [int]$counters.passed
$failedTests = [int]$counters.failed
$skippedTests = [int]($counters.skipped ?? $counters.notExecuted ?? 0)

$times = $trx.TestRun.Times
$durationSec = 0.0
if ($times.start -and $times.finish) {
    $duration = [datetime]$times.finish - [datetime]$times.start
    $durationSec = [math]::Round($duration.TotalSeconds, 2)
}

# 2. Parse Coverage from Summary.txt
if (-not (Test-Path $summaryTxt)) {
    throw "Coverage summary file not found at '$summaryTxt'."
}
$summaryContent = Get-Content $summaryTxt -Raw

function Extract-Percentage([string]$pattern, [string]$text) {
    $match = [regex]::Match($text, $pattern)
    if (-not $match.Success) {
        throw "Could not extract metric matching '$pattern' from coverage summary."
    }
    return [double]::Parse($match.Groups['pct'].Value, [System.Globalization.CultureInfo]::InvariantCulture)
}

$actualLine = Extract-Percentage 'Line coverage:\s+(?<pct>[\d\.]+)%' $summaryContent
$actualBranch = Extract-Percentage 'Branch coverage:\s+(?<pct>[\d\.]+)%' $summaryContent
$actualMethod = Extract-Percentage 'Method coverage:\s+(?<pct>[\d\.]+)%' $summaryContent

# 3. Evaluate Quality Gate
$linePass = $actualLine -ge $MinLineCoverage
$methodPass = $actualMethod -ge $MinMethodCoverage
$branchPass = $actualBranch -ge $MinBranchCoverage
$testsPass = $failedTests -eq 0

$overallPass = $linePass -and $methodPass -and $branchPass -and $testsPass

$lineStatus = if ($linePass) { 'PASS' } else { 'FAIL' }
$methodStatus = if ($methodPass) { 'PASS' } else { 'FAIL' }
$branchStatus = if ($branchPass) { 'PASS' } else { 'FAIL' }
$finalGateStatus = if ($overallPass) { 'PASS' } else { 'FAIL' }

# 4. Read Summary.md content if available
$markdownSummaryContent = ''
if (Test-Path $summaryMd) {
    $markdownSummaryContent = (Get-Content $summaryMd -Raw).Trim()
}

# 5. Build GitHub Step Summary
$sb = New-Object System.Text.StringBuilder

[void]$sb.AppendLine('# Three.cs — Build & Test Summary')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('## Test Results')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('| Total | Passed | Failed | Skipped | Duration | File |')
[void]$sb.AppendLine('|---:|---:|---:|---:|---:|---|')
[void]$sb.AppendLine("| $totalTests | $passedTests | $failedTests | $skippedTests | ${durationSec}s | $($trxFile.Name) |")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('## Coverage')
[void]$sb.AppendLine('')
if (-not [string]::IsNullOrWhiteSpace($markdownSummaryContent)) {
    [void]$sb.AppendLine('### Markdown Summary')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine($markdownSummaryContent)
    [void]$sb.AppendLine('')
}
[void]$sb.AppendLine('### Text Summary')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('```text')
[void]$sb.AppendLine($summaryContent.Trim())
[void]$sb.AppendLine('```')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('## Quality Gate')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('| Metric | Actual | Required | Status |')
[void]$sb.AppendLine('|---|---:|---:|---|')
[void]$sb.AppendLine("| Line | $actualLine% | >= $MinLineCoverage% | $lineStatus |")
[void]$sb.AppendLine("| Method | $actualMethod% | >= $MinMethodCoverage% | $methodStatus |")
[void]$sb.AppendLine("| Branch | $actualBranch% | >= $MinBranchCoverage% | $branchStatus |")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('### Final Gate')
[void]$sb.AppendLine('')
[void]$sb.AppendLine("**QUALITY GATE: $finalGateStatus**")
[void]$sb.AppendLine('')

$outputText = $sb.ToString()

# Write to console
Write-Host $outputText

# Write to GITHUB_STEP_SUMMARY if available
if ($env:GITHUB_STEP_SUMMARY) {
    [System.IO.File]::AppendAllText($env:GITHUB_STEP_SUMMARY, $outputText, [System.Text.Encoding]::UTF8)
}

# 6. Exit with failure code if gate failed
if (-not $overallPass) {
    Write-Error "Quality Gate FAIL: One or more coverage metrics are below threshold or tests failed."
    exit 1
}

Write-Host "Quality Gate PASS: All thresholds satisfied."
exit 0

