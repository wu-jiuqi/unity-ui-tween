param([string]$Unity = 'D:\unity\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not (Test-Path -LiteralPath $Unity)) { throw "Unity executable missing: $Unity" }
New-Item -ItemType Directory -Path (Join-Path $project 'evidence') -Force | Out-Null
$log = Join-Path $project 'evidence\build.log'
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $project + '"'), '-executeMethod', 'TweenGallery.Editor.GalleryBuild.BuildWindows', '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity failed ($($process.ExitCode)); inspect $log" }
$result = Get-Content -LiteralPath (Join-Path $project 'evidence\build-result.json') -Raw | ConvertFrom-Json
if ($result.result -ne 'Succeeded') { throw "Build did not succeed; inspect $log" }
Write-Output "Windows build: $($result.result), Unity $($result.unity), $($result.bytes) bytes"
