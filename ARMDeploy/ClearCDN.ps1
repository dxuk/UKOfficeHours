Login-AzureRMAccount

$sub = "External Demo Subscription"
$ResourceGroupName = "doandavoidResg"
$ProfileName = "azurevip"
$EndpointName = "azurevip"
$PurgeFolder = "/*"

Select-AzureRmSubscription -SubscriptionName $Sub

$cdnep = Get-AzureRmCdnEndpoint -EndpointName $EndpointName -ResourceGroupName $ResourceGroupName -ProfileName $ProfileName
Unpublish-AzureRmCdnEndpointContent -CdnEndpoint $cdnep -PurgeContent $PurgeFolder
