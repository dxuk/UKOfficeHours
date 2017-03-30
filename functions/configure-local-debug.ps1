
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

$env:AzureWebJobsStorage = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;"

$invocationPath = Split-Path $MyInvocation.MyCommand.Path
$configFilepath = "$invocationPath\local-debug-config.json"

function LaunchStorageEmulator() {
    Push-Location
    Set-Location -Path "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\Storage Emulator"
    if ($InvokeCommands) { 
        $result = & ".\AzureStorageEmulator.exe" status 
    }
    Pop-Location
}

function LaunchIISExpress() {
    $iisExpress = "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe"
    # IIS Express not happy with other methods used to generate path - this works
    $physicalPath = (Get-Item -Path "$invocationPath\..\wwwroot").FullName
    $command = "`"$iisExpress`" /path:`"$physicalPath`""
    if ($InvokeCommands) {
        cmd /c start cmd /k $command 
    }
}

function LaunchFuncExe() {
    Push-Location
    Set-Location $invocationPath
    if ($InvokeCommands) {
        cmd /c start cmd /k "func host start --cors http://localhost:8080" 
    }
    Pop-Location
}

LaunchStorageEmulator
LaunchIISExpress
Start-Sleep -Seconds 3
LaunchFuncExe

