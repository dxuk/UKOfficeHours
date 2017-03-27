# Deploys a multi-site app architecture in a resource group, with table storage, a web front end in app service and a functions middle tier
# Login first with Login-AzureRmAccount, or VSTS, then run poshdeploy.ps1

# This should run on any machine with the azure sdk installed - 
# Download the repo then - 
# cd to <Repo>\UKOfficeHours\UKOfficeHours
# Then run .\armdeploy\poshdeploy -deployname "<two letter environment code>"

# Valid environments for our internal usage deployment are "lo","cd","de","ts","pr", but any free combination can in theory be used.
# "lo" local
# "co" continuous delivery
# "de" development testing
# "ts" test environment and qa
# "pr" production / live environment

# Skipping -deployname param will run a dummy ci deployment to a resgroup of "dc" - a dummy ci testing environment.

# The script should then take care of everything else apart from setting the Azure AD App Manifest to allow oauth implicit flows, which you will have to do manually.

param
(

    $deployname = "dc",
    $rg = "oh$deployname",
    $loc = "UKSouth",
    $sub = "Internal DX OH Subscription"
    
)

$siteroot = "https://$deployname-ukofficehours.azurewebsites.net"
$display = "$deployname-ukofficehours"
Write-Host "Script Running - deploying environment $deployname to rg $rg in subscription $sub" -ForegroundColor yellow
$templatefile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbooking.json")
$tempparamfile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbookingparams.json")
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
    $currentapp = New-AzureRmADApplication -DisplayName $display -HomePage $siteroot -IdentifierUris $siteroot -ReplyUrls @($siteroot)
    
    # We could fire the Set-AzureADApplication cmdlet from the AzureAD module here to add OAuth Implicit flow support here
    # but we will add this manually to the manifest at the end of the 1st deployment.
     
    Write-Host "Created App: " $currentapp.ApplicationId
}

# Pull the appropriate tenant data (from the named subscription) and application
# ** This will need to be overwritten if the subscription is backed by a different AD tenant from the application **
$clientid = $currentapp.ApplicationId 
$tenantid = (Get-AzureRmsubscription -SubscriptionName $sub).TenantId 
$tenanturi = "https://sts.windows.net/$tenantid/"
$currentADdomainandtenant = (Get-AzureRmTenant -TenantId (Get-AzureRmSubscription -SubscriptionName $sub).TenantId).Domain

Write-Host "Resource Group Deployment Running" -ForegroundColor yellow

# Trigger the resource group deployment #
Select-AzureRmSubscription -SubscriptionName $sub
New-AzureRmResourceGroup -Name $rg -Location $loc -Force
New-AzureRmResourceGroupDeployment -ResourceGroupName $rg -TemplateFile $templatefile -Force -TemplateParameterFile $tempparamfile -prefix $deployname -AzureAD_TenantURI $tenanturi -AzureAD_ClientID $clientid -AzureAD_TenantID $currentADdomainandtenant

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
while ((curl $apifn).StatusCode -ne 200)
{
    Write-Host "Waiting for fn site to startup"
    Start-sleep -Seconds 1 
}
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
while ((curl $apiwb).StatusCode -ne 200)
{
    Write-Host "Waiting for fn site to startup"
    Start-sleep -Seconds 1 
}
Invoke-RestMethod -Uri $apiwb -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo2)} -Method PUT -InFile ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip")

Write-Host "Deploying Storage Account Tables" -ForegroundColor Green

# Create the tables inside the storage accounts

##############
# bookingslot
# isv
##############

# Define the storage account and context.
$StorageAccountName = $deployname + "ukohstoragedata"
$Ctx = New-AzureStorageContext $StorageAccountName -StorageAccountKey (Get-AzureRmStorageAccountKey -Name ($deployname + "ukohstoragedata") -ResourceGroupName $rg)[0].Value

#Create the tables
New-AzureStorageTable –Name "isv" -Context $Ctx -ErrorAction ignore
New-AzureStorageTable –Name "bookingslot" –Context $Ctx -ErrorAction ignore

$Ctx = $null

Write-Host "All Deployments Complete, $sitename online" -ForegroundColor Green





















