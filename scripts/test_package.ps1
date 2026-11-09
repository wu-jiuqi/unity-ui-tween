$ErrorActionPreference = 'Stop'
# Fixtures exercise prerequisite validation only, never Unity or release packaging.
$project = Split-Path -Parent $PSScriptRoot
$version = (Get-Content -LiteralPath (Join-Path $project 'VERSION') -Raw).Trim()
$root = Join-Path ([IO.Path]::GetTempPath()) ('tween-packaging-tests-' + [guid]::NewGuid().ToString('N'))
$fixture = Join-Path $root 'fixture with spaces'
New-Item -ItemType Directory -Path (Join-Path $fixture 'scripts'),(Join-Path $fixture 'evidence') -Force | Out-Null
$script = Join-Path $fixture 'scripts\package.ps1'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'package.ps1') -Destination $script
Set-Content -LiteralPath (Join-Path $fixture 'VERSION') -Value $version -Encoding ascii
$script:passed = 0
function Reject($label, $pattern) {
    $rejected = $false
    try { & $script -ValidateOnly | Out-Null }
    catch {
        if ($_.Exception.Message -notmatch $pattern) { throw "$label returned unexpected error: $($_.Exception.Message)" }
        $rejected = $true
    }
    if (-not $rejected) { throw "$label was accepted unexpectedly" }
    $script:passed++
    Write-Output "PASS: $label"
}
function Accept($label) {
    & $script -ValidateOnly -Python 'deliberately nonexistent python' | Out-Null
    $script:passed++
    Write-Output "PASS: $label"
}
function Report($value) {
    Set-Content -LiteralPath (Join-Path $fixture 'evidence\build-result.json') -Value $value -Encoding utf8
}
function BuildReport($changes) {
    $report = @{version=$version; unity='6000.0.26f1'; result='Succeeded'; errors=0}
    foreach ($key in $changes.Keys) { $report[$key] = $changes[$key] }
    Report ($report | ConvertTo-Json -Compress)
}
Reject 'Missing Windows files' 'Incomplete Windows build'
$build = Join-Path $fixture 'Builds\Windows'
New-Item -ItemType Directory -Path $build,(Join-Path $build 'UIMotionGallery_Data'),(Join-Path $build 'MonoBleedingEdge') -Force | Out-Null
Set-Content -LiteralPath (Join-Path $build 'UIMotionGallery.exe') -Value 'test-only placeholder' -Encoding ascii
Set-Content -LiteralPath (Join-Path $build 'UnityPlayer.dll') -Value 'test-only placeholder' -Encoding ascii
Reject 'No build report' 'Missing build report'
Report '{ not json'
Reject 'Malformed build report' 'Invalid build report JSON'
BuildReport @{result='Failed'; errors=1}
Reject 'Failed build' 'successful build'
BuildReport @{errors=2}
Reject 'Inconsistent succeeded report with errors' 'successful build'
BuildReport @{version='0.4.0'}
Reject 'Stale release version' 'version does not match'
Report ('{"version":"' + $version + '","result":"Succeeded","errors":0}')
Reject 'Missing Unity version' 'Unity version is missing'
Report ('{"version":"' + $version + '","unity":"6000.0.26f1","result":"Succeeded"}')
Reject 'Missing error count' 'successful build'
BuildReport @{errors=$null}
Reject 'Null error count' 'successful build'
BuildReport @{errors=$false}
Reject 'Boolean error count' 'successful build'
BuildReport @{errors='0'}
Reject 'String error count' 'successful build'
BuildReport @{unity='   '}
Reject 'Blank Unity version' 'Unity version is missing'
BuildReport @{unity=6000}
Reject 'Non-string Unity version' 'Unity version is missing'
BuildReport @{}
Accept 'Successful report without ignored root BUILD_INFO.json or Python'
Set-Content -LiteralPath (Join-Path $fixture 'BUILD_INFO.json') -Value 'stale root metadata must not be used' -Encoding ascii
Accept 'Canonical build report takes precedence over stale root metadata'
if (Test-Path -LiteralPath (Join-Path $fixture 'dist')) { throw 'ValidateOnly wrote package output' }
if (Get-ChildItem -LiteralPath $fixture -Directory -Filter 'packaging-*') { throw 'ValidateOnly created staging output' }
$script:passed++
Write-Output 'PASS: ValidateOnly creates no package or staging files'
Write-Output "Package prerequisite tests passed: $script:passed"
Write-Output "Test-only fixtures retained at $root"
