# Deploys a multi-site app architecture in a resource group, with table storage, a web front end in app service and a functions middle tier
# Login first with Login-AzureRmAccount, or VSTS, then run this.

# On my local pc - cd to C:\Users\wieastbu\Source\Repos\UKOfficeHours\UKOfficeHours
# Then run with .\armdeploy\poshdeploy -deployname "dv"
# Skipping -deployname param will run a dummy ci deployment to a resgroup of "dummyci"

param
(

    $deployname = "dummyci",
    $rg = "oh$deployname",
    $loc = "UKSouth",
    $sub = "Internal DX OH Subscription",
    $siteroot = "https://$deployname-ukofficehours.azurewebsites.net",
    $display = "$deployname-ukofficehours"

)


Write-Host "Script Running - deploying rg $deployname" -ForegroundColor yellow
$templatefile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbooking.json")
$tempparamfile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbooking" + $deployname + "params.json")
$temploc = "test.output.txt"
Add-Type -A System.IO.Compression.FileSystem

Write-Host "Retrieving AD Application $display" -ForegroundColor yellow
$currentapp = (get-azurermadapplication -IdentifierUri $siteroot)

if ($currentapp -ne $null) 
{
    Write-Host "Found App:" $currentapp.ApplicationId
    
}
else
{
    Write-Host "App URI not found ... Creating AD Application $display" -ForegroundColor yellow
    $currentapp = New-AzureRmADApplication -DisplayName $display -HomePage $siteroot -IdentifierUris $siteroot 
    Write-Host "Created App: " $currentapp.ApplicationId
}

# Pull the appropriate tenant (from the named subscription) and application
$clientid = $currentapp.ApplicationId 
$tenantid = (Get-AzureRmsubscription -SubscriptionName $sub).TenantId # This will need to be overwritten if the subscription is backed by a different AD tenant from the application
$tenanturi = "https://sts.windows.net/$tenantid/"

Write-Host "Resource Group Deployment Running" -ForegroundColor yellow

# Trigger the resource group deployment #
Select-AzureRmSubscription -SubscriptionName $sub
New-AzureRmResourceGroup -Name $rg -Location $loc -Force
New-AzureRmResourceGroupDeployment -ResourceGroupName $rg -TemplateFile $templatefile -Force -TemplateParameterFile $tempparamfile -AzureAD_ClientID $clientid

Write-Host "Resource Group Deployment Complete" -ForegroundColor Green
Write-Host "Function App Deployment Running" -ForegroundColor yellow

# Deploy the function app #
Remove-Item ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip") -ErrorAction ignore
[IO.Compression.ZipFile]::CreateFromDirectory(((Get-Item -Path ".\" -Verbose).FullName + "\functions\"),((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip"))
$site = Get-AzureRmWebAppPublishingProfile -Name ($deployname + "ukohfn") -ResourceGroup $rg -Format "WebDeploy" -OutputFile $temploc
$username =  ([xml] $site).publishData.publishProfile[0].userName # The Username
$password =  ([xml] $site).publishData.publishProfile[0].userPWD  # The Password 
$fnsitename = ($deployname + "ukohfn")
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username, $password)))
$apifn = "https://$fnsitename.scm.azurewebsites.net/api/zip/site/wwwroot/"
Invoke-RestMethod -Uri $apifn -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method PUT -InFile ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip")

Write-Host "Function App Deployment Complete" -ForegroundColor Green
Write-Host "Web App Deployment Running" -ForegroundColor yellow

# Deploy the front end site #
Remove-Item ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip") -ErrorAction ignore
[IO.Compression.ZipFile]::CreateFromDirectory(((Get-Item -Path ".\" -Verbose).FullName + "\wwwroot\"),((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip"))
$site2 = Get-AzureRmWebAppPublishingProfile -Name ($deployname + "-ukofficehours") -ResourceGroup $rg -Format "WebDeploy" -OutputFile $temploc
$username2 =  ([xml] $site2).publishData.publishProfile[0].userName # The Username
$password2 =  ([xml] $site2).publishData.publishProfile[0].userPWD  # The Password 
$sitename = ($deployname + "-ukofficehours")
$base64AuthInfo2 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username2, $password2)))
$apiwb = "https://$sitename.scm.azurewebsites.net/api/zip/site/wwwroot/"
Invoke-RestMethod -Uri $apiwb -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo2)} -Method PUT -InFile ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip")

Write-Host "All Deployments Complete, $sitename online" -ForegroundColor Green





















