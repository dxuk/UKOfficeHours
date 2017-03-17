# login first with Login-AzureRmAccount
# cd to C:\Users\wieastbu\Source\Repos\UKOfficeHours\UKOfficeHours
# run with .\armdeploy\poshdeploy -deployname "tdp"
param
(
    $deployname = "ci",
    $rg = "oh$deployname",
    $loc = "UKSouth",
    $sub = "Internal DX OH Subscription"
)

$templatefile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbooking.json")
$tempparamfile = ((Get-Item -Path ".\" -Verbose).FullName + "\armdeploy\ohbooking" + $deployname + "params.json")
 
Select-AzureRmSubscription -SubscriptionName $sub
New-AzureRmResourceGroup -Name $rg -Location $loc -Force
New-AzureRmResourceGroupDeployment -ResourceGroupName $rg -TemplateFile $templatefile -Force -TemplateParameterFile $tempparamfile

Remove-Item ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip")
Remove-Item ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip")

Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory(((Get-Item -Path ".\" -Verbose).FullName + "\wwwroot\"),((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip"))
[IO.Compression.ZipFile]::CreateFromDirectory(((Get-Item -Path ".\" -Verbose).FullName + "\functions\"),((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip"))

$temploc = "test.output.txt"
# For web deploy - Get the publishing data from the site.
$site = Get-AzureRmWebAppPublishingProfile -Name ($deployname + "ukohfn") -ResourceGroup $rg -Format "WebDeploy" -OutputFile $temploc

# Pull the user details from the first Pub profile object returned
$username =  ([xml] $site).publishData.publishProfile[0].userName # The Username
$password =  ([xml] $site).publishData.publishProfile[0].userPWD  # The Password 

$ZipPath = "path to zip file"
$sitename = ($deployname + "-ukofficehours")
$fnsitename = ($deployname + "ukohfn")

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username, $password)))

$apiwb = "https://$sitename.scm.azurewebsites.net/api/zip/site/wwwroot/"
$apifn = "https://$fnsitename.scm.azurewebsites.net/api/zip/site/wwwroot/"

Invoke-RestMethod -Uri $apiwb -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method PUT -InFile ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip")
Invoke-RestMethod -Uri $apifn -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method PUT -InFile ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip")
