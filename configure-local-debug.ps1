
#
# PowerShell script to enable local debugging of website and functions
# Launches 
# Azure Storage Emulator - local emulator for Azure Storage
# IIS Express - web server to host the wwwroot website
# Func.exe - host for the Azure functions
#

Param(
    # Set to false to run but not execute the commands for debugging
    [bool]$InvokeCommands = $true
)

$invocationPath = Split-Path $MyInvocation.MyCommand.Path

function LoadConfig([string]$ConfigFilePath) {
    return ConvertFrom-Json "$(get-content $ConfigFilepath)"
}

function LaunchStorageEmulator() {
    Push-Location
    Set-Location -Path "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\Storage Emulator"
    if ($InvokeCommands) { 
        $result = & ".\AzureStorageEmulator.exe" start 
    }
    Pop-Location
}

function LaunchIISExpress() {
    $iisExpress = "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe"
    # IIS Express not happy with other methods used to generate path - this works
    $physicalPath = (Get-Item -Path "$invocationPath\wwwroot").FullName
    $command = "`"$iisExpress`" /path:`"$physicalPath`""
    if ($InvokeCommands) {
        cmd /c start cmd /k $command 
    }
}

function LaunchFuncExe() {
    Push-Location
    Set-Location "$invocationPath\functions"
    if ($InvokeCommands) {
        cmd /c start cmd /k "func host start --cors http://localhost:8080" 
    }
    Pop-Location
}

#Write-Host $invocationPath

$config = LoadConfig "$invocationPath\wwwroot\local-debug-config.json"

$env:AzureWebJobsStorageConnection = $config.AzureWebJobsStorage
#Write-Host $env:AzureWebJobsStorage
$env:Service_Description = $config.Service_Description
#Write-Host $env:Service_Description
$env:APPSETTING_WEBSITE_SITE_NAME = $config.WebsiteName
#Write-Host $env:APPSETTING_WEBSITE_SITE_NAME

LaunchStorageEmulator
LaunchIISExpress
Start-Sleep -Seconds 3
LaunchFuncExe

