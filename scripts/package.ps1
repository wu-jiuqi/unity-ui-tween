$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$version = (Get-Content -LiteralPath (Join-Path $project 'VERSION') -Raw).Trim()
$dist = Join-Path $project 'dist'
$staging = Join-Path $project ('packaging-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $dist,$staging -Force | Out-Null
$build = Join-Path $project 'Builds\Windows'
foreach ($required in @('UIMotionGallery.exe','UnityPlayer.dll','UIMotionGallery_Data','MonoBleedingEdge')) {
    if (-not (Test-Path -LiteralPath (Join-Path $build $required))) { throw "Incomplete Windows build: $required" }
}
$win = Join-Path $staging 'Windows'
New-Item -ItemType Directory -Path $win -Force | Out-Null
Copy-Item -Path (Join-Path $build '*') -Destination $win -Recurse
Copy-Item -LiteralPath (Join-Path $project 'ThirdPartyLicenses') -Destination $win -Recurse
Copy-Item -LiteralPath (Join-Path $project 'docs') -Destination $win -Recurse
Copy-Item -LiteralPath (Join-Path $project 'VERSION') -Destination $win
Copy-Item -LiteralPath (Join-Path $project 'BUILD_INFO.json') -Destination $win
@'
Unity UI 动效展馆

完整解压压缩包，双击 UIMotionGallery.exe。
请保留 UIMotionGallery_Data、MonoBleedingEdge、UnityPlayer.dll 等同级文件。
不需要安装 Unity；展馆运行无需网络。
左侧选择动效，中间播放/暂停/慢放/对比，右侧调参和复制 Codex 提示词。
按 Escape 退出。详细使用方法与实际验证范围见 docs 文件夹。
'@ | Set-Content -LiteralPath (Join-Path $win '请先阅读.txt') -Encoding utf8
& python -X utf8 (Join-Path $PSScriptRoot 'portable_zip.py') $win (Join-Path $dist "UnityTweenGallery-$version-Windows-x64.zip") --force
if ($LASTEXITCODE -ne 0) { throw 'Windows ZIP verification failed' }

$skillZip = Join-Path $dist "unity-ui-tween-$version.zip"
& python -X utf8 (Join-Path $PSScriptRoot 'package_skill.py') --output $skillZip --force
if ($LASTEXITCODE -ne 0) { throw 'Skill ZIP verification failed' }
& python -X utf8 (Join-Path $PSScriptRoot 'package_skill.py') --profile runtime --output (Join-Path $dist "unity-ui-tween-$version-runtime.zip") --force
if ($LASTEXITCODE -ne 0) { throw 'Runtime Skill ZIP verification failed' }
$skillStage = Join-Path $staging 'SkillPackage'
Expand-Archive -LiteralPath $skillZip -DestinationPath $skillStage

$source = Join-Path $staging 'UnityTweenGallery'
New-Item -ItemType Directory -Path $source -Force | Out-Null
foreach ($name in @('Assets','Packages','ProjectSettings','docs','ThirdPartyLicenses','README.md','VERSION','BUILD_INFO.json','.gitignore','.gitattributes','ARCHITECTURE.md','PROGRESS.md')) {
    Copy-Item -LiteralPath (Join-Path $project $name) -Destination $source -Recurse
}
# Copy authored scripts only; Python caches vary by interpreter and are not source.
$scriptDestination = Join-Path $source 'scripts'
New-Item -ItemType Directory -Path $scriptDestination -Force | Out-Null
Get-ChildItem -LiteralPath $PSScriptRoot -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/]__pycache__[\\/]' -and $_.Extension -notin '.pyc','.pyo'
} | ForEach-Object {
    $relative = $_.FullName.Substring($PSScriptRoot.Length).TrimStart('\','/')
    $destination = Join-Path $scriptDestination $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $_.FullName -Destination $destination
}
New-Item -ItemType Directory -Path (Join-Path $source 'Skill') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $skillStage 'unity-ui-tween') -Destination (Join-Path $source 'Skill') -Recurse
& python -X utf8 (Join-Path $PSScriptRoot 'portable_zip.py') $source (Join-Path $dist "UnityTweenGallery-$version-Source.zip") --include-root --force
if ($LASTEXITCODE -ne 0) { throw 'Source ZIP verification failed' }
$hashes = Get-ChildItem -LiteralPath $dist -File | Where-Object { $_.Extension -in '.zip','.unitypackage' -and $_.Name.Contains($version) } | ForEach-Object {
    $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256
    "$($hash.Hash.ToLowerInvariant())  $($_.Name)"
}
$hashes | Set-Content -LiteralPath (Join-Path $dist ('SHA256SUMS-' + $version + '.txt')) -Encoding ascii
Write-Output "Packaged assets at $dist"
Get-ChildItem -LiteralPath $dist -File | Select-Object Name,Length
