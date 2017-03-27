# cd to <Repo>\UKOfficeHours\UKOfficeHours
# Then run .\armdeploy\poshdeploy -deployname "<two letter environment code>"

param
(
    $deployname = "dc",
    $rg = "oh$deployname",
    $loc = "UKSouth",
    $sub = ""
)

#$localtenantdomainoverride

$siteroot = "https://$deployname-ukofficehours.azurewebsites.net"
$display = "$deployname-ukofficehours"
Write-Host "Script Running - deploying environment $deployname to rg $rg in subscription $sub" -ForegroundColor yellow
$templatefile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbooking.json")
$tempparamfile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbookingparams.json")
$temploc = "test.output.txt"

Add-Type -A System.IO.Compression.FileSystem

#$clientid = $localappidoverride
#$tenantid = $localtenantoverride
#$tenanturi = "https://sts.windows.net/$tenantid/"
#$currentADdomainandtenant = $localtenantdomainoverride

#if ($localappidoverride -eq $null) 
#{
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
    # ** This will need to be overwritten if the subscription is backed by a different AD tenant from the application, or you don't have permissions to look it up !**

    $clientid = $currentapp.ApplicationId 
    $tenantid = (Get-AzureRmsubscription -SubscriptionName $sub).TenantId 
    $tenanturi = "https://sts.windows.net/$tenantid/"
    $currentADdomainandtenant = (Get-AzureRmTenant -TenantId (Get-AzureRmSubscription -SubscriptionName $sub).TenantId).Domain

#}
#else
#{#
#
#    Write-Host "AppID and tenant data overridden with these 4 values"
#
#    $clientid 
#    $tenantid 
#    $tenanturi 
#    $currentADdomainandtenant 
#}

Write-Host "Resource Group Deployment Running" -ForegroundColor yellow

# Trigger the resource group deployment #
Select-AzureRmSubscription -SubscriptionName $sub
New-AzureRmResourceGroup -Name $rg -Location $loc -Force

write-host "Deploying with prefix :" + $deployname 
WRITE-HOST "exec command: New-AzureRmResourceGroupDeployment -ResourceGroupName $rg -TemplateFile $templatefile -Force -TemplateParameterFile $tempparamfile -svprefix $deployname -AzureAD_TenantURI $tenanturi -AzureAD_TenantID $currentADdomainandtenant -AzureAD_ClientID $clientid "

New-AzureRmResourceGroupDeployment -ResourceGroupName $rg -TemplateFile $templatefile -Force -TemplateParameterFile $tempparamfile -svprefix $deployname -AzureAD_TenantURI $tenanturi -AzureAD_TenantID $currentADdomainandtenant -AzureAD_ClientID $clientid

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
New-AzureStorageTable -Name "isv" -Context $Ctx -ErrorAction ignore
New-AzureStorageTable -Name "bookingslot" -Context $Ctx -ErrorAction ignore

$Ctx = $null

Write-Host "All Deployments Complete, $sitename online" -ForegroundColor Green





















