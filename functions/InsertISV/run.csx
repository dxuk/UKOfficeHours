#load "..\Shared\httpUtils.csx"
#load "..\Domain\BookingSlot.csx"
#load "..\Domain\isv.csx"

#r "Microsoft.WindowsAzure.Storage"

using System;    
using System.Threading; 
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure; 
using Microsoft.Azure.KeyVault; 
using Microsoft.Azure.KeyVault.Core; 
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json; 
using Microsoft.WindowsAzure.Storage.Table.Queryable;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable tblbk, CloudTable outObj, TraceWriter log)
{ 
 
    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };
    
    isv thisISV = await httpUtils.GetTFromJSONRequestBody<isv>(req);

    if (thisISV.CurrentCode == "" || thisISV.CurrentCode == null) 
    {
        try {
            log.Info("ISV is a new one, generating code"); 
            thisISV.AddUniqueAlphaNumCodeAndSave(tblbk);
        }
        catch (ApplicationException ex) {
            return req.CreateResponse(HttpStatusCode.Conflict, "Unable to generate a unique booking code.");
        }
    }

    thisISV.RowKey = $"{thisISV.Name}_{thisISV.CurrentCode}";
    thisISV.PartitionKey = httpUtils.GetCurrentUserEmailFromClaims();
    //thisISV.Createdby = httpUtils.GetCurrentUserEmailFromClaims();

    TableOperation operationadd = TableOperation.Insert(thisISV);
    outObj.Execute(operationadd, GetEncryptionPolicy());

    return req.CreateResponse(HttpStatusCode.OK, thisISV);
}

public static TableRequestOptions GetEncryptionPolicy()
{
    KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(async (string authority, string resource, string scope) => {
            ClientCredential credential = new ClientCredential(Environment.GetEnvironmentVariable("KVClientId"), Environment.GetEnvironmentVariable("KVKey"));
            AuthenticationContext ctx = new AuthenticationContext(new Uri(authority).AbsoluteUri, false);
            AuthenticationResult result = await ctx.AcquireTokenAsync(resource, credential);
            return result.AccessToken;}
    );
    
    IKey cloudKey1 = cloudResolver.ResolveKeyAsync(Environment.GetEnvironmentVariable("KVKeyID"), CancellationToken.None).GetAwaiter().GetResult(); 
    TableEncryptionPolicy encryptionPolicy = new TableEncryptionPolicy(cloudKey1, cloudResolver);
    TableRequestOptions rqOptions = new TableRequestOptions() { EncryptionPolicy = encryptionPolicy };
    return rqOptions; 
} 
