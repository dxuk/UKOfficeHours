param
(
    $deployname = "tdp",
    $rg = "oh$deployname",
    $loc = "UKSouth",
    $sub = "Internal DX OH Subscription",
    $templatefile = "ohbooking.json"
)
    
   $TemplateParameterObject = @{prefix = $deployname};

#Login-AzureRmAccount
Select-AzureRmSubscription -SubscriptionName $sub
New-AzureRmResourceGroup -Name $rg -Location $loc -Force
New-AzureRmResourceGroupDeployment -ResourceGroupName $rg -TemplateFile $templatefile -Force -TemplateParameterFile ("ohbooking" + $deployname + "params.json")
