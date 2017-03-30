Write-Output "Keep-alive ping:$(get-date)";

if ($env:APPSETTING_WEBSITE_SITE_NAME) {
    $resp = Invoke-WebRequest -uri "https://" + $env:APPSETTING_WEBSITE_SITE_NAME + "/api/GetConfig" -UseBasicParsing
} else {
    $resp = Invoke-WebRequest -uri "http://localhost:7071/api/GetConfig" -UseBasicParsing
}
