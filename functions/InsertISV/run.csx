#load "..\Shared\httpUtils.csx"
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

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable outObj, TraceWriter log)
{ 
 
    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };
    log.Info(Environment.GetEnvironmentVariable("KVKeyID"));
    isv thisISV = await httpUtils.GetTFromJSONRequestBody<isv>(req);

    thisISV.RowKey = thisISV.Name;
    thisISV.PartitionKey = httpUtils.GetCurrentUserEmailFromClaims();

    if (thisISV.CurrentCode == "" || thisISV.CurrentCode == null) {thisISV.AddUniqueAlphaNumCodeAndSave();}

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

public class bookingslot : TableEntity
{
    public bookingslot()
    {

        CreatedDateTime = DateTime.Now; 

    }
    
    public string TechnicalEvangelist {get; set;}
    public DateTime StartDateTime {get; set;}
    public DateTime EndDateTime {get; set;}

    public string MailID {get;set;}

    public int Duration
    {
        get
        {
            TimeSpan ts = EndDateTime - StartDateTime;
            return ts.Minutes + (ts.Hours * 60);
        }
        set { }
    }

    public string BookedToISV { get; set; }
    public string BookingCode { get; set; }
    public string PBE { get; set; }

    public DateTime CreatedDateTime { get; set; }

}

public class isv : TableEntity
{
    [EncryptProperty]
    public string Name { get; set; }
    [EncryptProperty]
    public string ContactName { get; set; }
    [EncryptProperty]
    public string ContactEmail { get; set; }
    public string CurrentCode { get; set; }
    public string AddUniqueAlphaNumCodeAndSave()
    {

        // Generate a unique alphanumeric 8 digit code. 

        // MD5 hash the ISV name pair + date created + random number, all concatenated
        // encode as a string, then Strip the first 8 chars
        // Confirm it's unique in the isv table 
        // If unique store it ! 

        Random rnd = new Random();

        string source = $"{ContactName}{Name}{DateTime.UtcNow.ToLongTimeString()}{rnd.Next().ToString()}{PartitionKey}";

        using (SHA512 Hash = SHA512.Create())
        {

            string hash = GetSHA512Hash(Hash, source);

            CurrentCode = hash.ToUpper().Substring(0, 8);

        }

        return CurrentCode;
    }
    private static string GetSHA512Hash(SHA512 shaHash, string input)
    {

        // Convert the input string to a byte array and compute the hash.
        byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

} 


