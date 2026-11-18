param(
    [ValidateSet('Build','Validate')][string]$Target='Build',
    [string]$Unity='D:\Unity\Editor\Unity.exe'
)
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if($Target -eq 'Build') {
    $log=Join-Path $project 'evidence\recipes96-build.log'
    New-Item -ItemType Directory -Path (Split-Path $log) -Force | Out-Null
    $run=Start-Process -FilePath $Unity -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+$project+'"'),'-executeMethod','TweenGallery.Editor.RecipeGalleryBuild.BuildWindows','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
    $run.WaitForExit()
    if($run.ExitCode -ne 0){throw "96-recipe build failed. See $log"}
    Write-Output 'Built Builds/Recipes96/UIRecipeGallery.exe'
} else {
    foreach($size in @(@(1600,1000),@(1280,720))) {
        $out=Join-Path $project ('validation-output\recipes96-'+$size[0])
        New-Item -ItemType Directory -Path $out -Force | Out-Null
        # This is an interactive graphics test. Hidden/minimized windows can return black captures.
        $run=Start-Process -FilePath (Join-Path $project 'Builds\Recipes96\UIRecipeGallery.exe') -ArgumentList @('-screen-width',$size[0],'-screen-height',$size[1],'-screen-fullscreen','0','--recipes-test','--test-output',('"'+$out+'"'),'-logFile',('"'+$out+'\player.log"')) -PassThru
        if(!$run.WaitForExit(240000)){$run.Kill();throw 'Recipe test watchdog timeout'}
        if($run.ExitCode -ne 0){throw "96-recipe player validation failed: $out"}
        Write-Output "PASS 96 recipes at $($size[0]) x $($size[1])"
    }
}
