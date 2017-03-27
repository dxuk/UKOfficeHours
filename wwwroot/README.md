# UKOfficeHours
Home for the source code of the DX UK Office Hours Booking Site

Deploys a multi-site app architecture in a resource group, with table storage, a web front end in app service and a functions middle tier
Login first with Login-AzureRmAccount, or VSTS, then run poshdeploy.ps1

This should run on any machine with the azure sdk installed - 
Download the repo then - 
cd to <Repo>\UKOfficeHours\UKOfficeHours
Then run .\armdeploy\poshdeploy -deployname "<two letter environment code>"

Valid environments for our internal usage deployment are "lo","cd","de","ts","pr", but any free combination can in theory be used.
"lo" local services configured to interact with a local frontend
"co" continuous delivery
"tr" training environment with realistic user dummy data
"tp" test environment (with performance settings for perf and load test)
"pr" production / live environment

Skipping -deployname param will run a dummy ci deployment to a resgroup of "dc" - a dummy ci testing environment.

The script should then take care of everything else apart from setting the Azure AD App Manifest to allow oauth implicit flows, which you will have to do manually in the Azure AD part of the azure portal.

Apart from that, get the repo and do this to deploy a dev environment

PS C:\Users\wieastbu\Source\Repos\UKOfficeHours\UKOfficeHours> .\armdeploy\poshdeploy "de" 

