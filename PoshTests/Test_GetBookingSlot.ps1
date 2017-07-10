param
(
    [string] $servicetotest = "coukohfn", 
    [string] $authresource = "co-ukofficehours",
    [string] $clientId = "c71134f1-5d19-4ece-8929-ee5c7a553e80",
    [string] $key = "Xp12Fon0EsjpkjZH3s4e8JG2aBiBSBNjk6guXZFFKHk=",
    [string] $adTenant = "microsoft.onmicrosoft.com"
)

[string] $authority = "https://login.microsoftonline.com/$adTenant"
[string] $baseuri = "https://" + $servicetotest + ".azurewebsites.net"
[string] $authuri = "https://" + $authresource + ".azurewebsites.net"
[string] $fnuri = $baseuri + "/api/GetBookingSlot"

$authContext = New-Object "Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" -ArgumentList $authority 
$cred = New-Object "Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential" -ArgumentList $clientid, $key
$token = $AuthContext.AcquireTokenAsync($authuri,$cred)

# API:GetBookingSlot Tests
# Returns an array of available slots that can be booked using a code in the next 90 days


$cont = Invoke-RestMethod -uri $fnuri -method GET

# The function under test does not take any parameters
# Note: Behaviour varies depending on authentication, test both authed and unauthed

# This test is idempotent and non-destructive, hence it can be run repeatedly and at random
# It should NEVER throw an error, and during normal operation should always return data


# Test Set 0: Check returned shape: Should return a json array with the following shape 
# [Array of bookingslot objects in this format] 
if (!($cont -is [array])) { throw "Fail: Did not return an array" }
if (!($cont.count -gt 0)) { throw "Fail: Returned an empty array (i.e. No bookingslots)"}
 
# On ALL Runs 
# Test each item in the returned set
foreach($item in $cont)
{ 
    # TechnicalEvangelist : wieastbu@microsoft.com
    # StartDateTime       : 2017-07-03T19:30:00Z
    # EndDateTime         : 2017-07-03T20:00:00Z
    # MailID              : (can be nullable or not present)
    # Duration            : 30
    # BookedToISV         : None
    # BookingCode         : None
    # PBE                 : None
    # CreatedDateTime     : 2017-06-27T19:32:27.2533407Z
    # PartitionKey        : 201707
    if (($item.TechnicalEvangelist -eq $null)) { throw "Fail: Field TechnicalEvangelist not present or is null on returned item" }
    if (($item.StartDateTime -eq $null))       { throw "Fail: Field StartDateTIme not present or is null on returned item" }
    if (($item.EndDateTime -eq $null))         { throw "Fail: Field EndDateTIme not present or is null on returned item"}
    if (($item.Duration -eq $null))            { throw "Fail: Field Duration not present or is null on returned item"}
    if (($item.BookedToISV -eq $null))         { throw "Fail: Field BookedToISV not present or is null on returned item"}
    if (($item.BookingCode -eq $null))         { throw "Fail: Field BookingCode not present or is null on returned item"}
    if (($item.PBE -eq $null))                 { throw "Fail: Field PBE not present or is null on returned item"}
    if (($item.CreatedDateTime -eq $null))     { throw "Fail: Field CreatedDateTime not present or is null on returned item"}
    if (($item.PartitionKey -eq $null))        { throw "Fail: Field PartitionKey not present or is null on returned item"}
    if (($item.RowKey -eq $null))              { throw "Fail: Field PartitionKey not present or is null on returned item"}

    # Test 1 RowKey must be a GUID
    try { [System.Guid]::Parse($item.RowKey) } catch [FormatException] { throw "Fail: Field RowID is not a GUID: it returned " + $item.RowKey }

    # PartitionKey should be set to the year and month of the StartDateTimeValue
    if (($item.PartitionKey -ne ($item.StartDateTime.Split("-")[0] + $item.StartDateTime.Split("-")[1]))) { throw "Fail: Field PartitionKey does not match the year and month of the StartDateTimeValue"}

    # This set should not return any results in the past
    if ((Get-Date -Date ($item.StartDateTime)) -lt (Get-Date)) { throw "Fail: Field StartDateTime returned a value in the past"}

    # Sessions should always end after they start
    if ((Get-Date $item.EndDateTime) -lt (Get-Date $item.StartDateTime)) { throw "Fail: Field EndDateTime returned a value earlier than StartDateTime"}

}

# Only When run unauthenticated:
# This should not return one or more 180 day entities
foreach($item in $cont)
{ 
    # Should not return any entities bigger than 60 minutes (no ADS')
    if ($item.Duration -gt 60) { throw "Fail: Field Duration returned a value larger than 60 minutes"}

        
}

$headers = @{"Authorization" = CreateAuthHeader($baseuri)}
$authcont = Invoke-RestMethod -Uri $fnuri -Headers $headers

# Only When run authenticated:
# This should return one or more 180 day entities

$count180 = 0

foreach($item in $authcont)
{ 
    # Should not return any entities bigger than 60 minutes (no ADS')
    if ($item.Duration -ge 61) { $count180 = $count180 + 1}
        
}

if ($count180 -eq 0) {throw "Fail: Did not return one or more sessions with a duration of longer than 60 minutes"}


