param([ValidateSet('EditMode','PlayMode','Build')][string]$Stage)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$unityPath='C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Unity.exe'
$unityArgs='-batchmode -projectPath "'+$taskRoot+'" '
if($Stage -eq 'Build') {
    $env:RYDERS_BLOCK_ANDROID_JDK='C:\Users\lin4s\AppData\Local\RydersBlock\AndroidDeps\OpenJDK17'
    $env:RYDERS_BLOCK_ANDROID_NDK='C:\Users\lin4s\AppData\Local\RydersBlock\AndroidDeps\NDK-r27c'
    $env:RYDERS_BLOCK_ANDROID_SDK='C:\Users\lin4s\AppData\Local\RydersBlock\AndroidDeps\AndroidSDK'
    $unityArgs+='-quit -nographics -buildTarget Android -executeMethod Avoidance.EditorTools.AndroidDevelopmentBuilder.BuildAndroidDevelopment -logFile Logs/mastery-android-build.log'
} else {
    $unityArgs+='-runTests -testPlatform '+$Stage+' -testResults Logs/mastery-final-'+$Stage+'.xml -logFile Logs/mastery-final-'+$Stage+'.log'
}
$run=Start-Process -FilePath $unityPath -ArgumentList $unityArgs -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -Wait
if($run.ExitCode -ne 0){throw "$Stage failed with exit code $($run.ExitCode). Inspect the stage log."}
Write-Output "$Stage completed successfully."
