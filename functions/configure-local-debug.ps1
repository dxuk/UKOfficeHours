
#
#
#

Param(
    # Parameter help description
    [bool]$invokeCommands = $false
)

$invocationPath = Split-Path $MyInvocation.MyCommand.Path
$configFilepath = "$invocationPath\local-debug-config.json"

Push-Location
Set-Location -Path "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\Storage Emulator"
if ($invokeCommands) { & ".\AzureStorageEmulator.exe" status }
Pop-Location

$iisExpress = "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe"
#
# TODO - Need to find a way to find a path that IIS Express is happy with
#
$physicalPath = "C:\Users\mormond\Repos\UKOfficeHours\wwwroot" #Join-Path $invocationPath "\..\wwwroot"
$command = "`"$iisExpress`" /path:`"$physicalPath`""
if ($invokeCommands) { cmd /c start cmd /k $command }

Start-Sleep -Seconds 3

$config = ConvertFrom-Json "$(get-content $configFilepath)"
$env:AzureWebJobsStorageConnection = $config.AzureWebJobsStorage

Push-Location
Set-Location ".\functions"
if ($invokeCommands) { cmd /c start cmd /k "func host start" }
Pop-Location

