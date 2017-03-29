
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
# IIS Express not happy with other methods used to generate path - this works
$physicalPath = (Get-Item -Path "$invocationPath\..\wwwroot").FullName
$command = "`"$iisExpress`" /path:`"$physicalPath`""
if ($invokeCommands) { cmd /c start cmd /k $command }

Start-Sleep -Seconds 3

$config = ConvertFrom-Json "$(get-content $configFilepath)"
$env:AzureWebJobsStorageConnection = $config.AzureWebJobsStorage

Push-Location
Set-Location $invocationPath
if ($invokeCommands) { cmd /c start cmd /k "func host start" }
Pop-Location

