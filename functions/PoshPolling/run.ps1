Write-Output "Keep-alive ping:$(get-date)";
$resp = Invoke-WebRequest -uri "https://" + $env:APPSETTING_WEBSITE_SITE_NAME + "/api/GetConfig" -UseBasicParsing