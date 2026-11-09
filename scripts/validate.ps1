param([string]$Unity='D:\Unity\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
foreach($validator in @('LibraryValidation','RuntimeValidation','ButtonFeedbackValidation')) {
    $log=Join-Path $project ('evidence\'+$validator+'.log')
    $run=Start-Process -FilePath $Unity -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+$project+'"'),'-executeMethod',('TweenGallery.Editor.'+$validator+'.Run'),'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
    $run.WaitForExit()
    if($run.ExitCode -ne 0){throw "Editor validation failed: $validator ($log)"}
    Write-Output "PASS $validator"
}
foreach($size in @(@(1600,900),@(1280,720))) {
    foreach($mode in @('presentation','workbench','self','gallery-enhancement')) {
        $folder=Join-Path $project ('validation-output\v060p1-'+$size[0]+'-'+$mode)
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        $run=Start-Process -FilePath (Join-Path $project 'Builds\Windows\UIMotionGallery.exe') -ArgumentList @('-screen-width',$size[0],'-screen-height',$size[1],'-screen-fullscreen','0',('--'+$mode+'-test'),'--test-output',('"'+$folder+'"'),'-logFile',('"'+$folder+'\player.log"')) -WindowStyle Hidden -PassThru
        $run.WaitForExit()
        if($run.ExitCode -ne 0){throw "Player validation failed: $mode $($size[0]) ($folder)"}
        Write-Output "PASS $mode $($size[0])x$($size[1])"
    }
}
