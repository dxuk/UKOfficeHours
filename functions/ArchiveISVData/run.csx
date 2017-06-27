#load "..\Shared\httpUtils.csx"
#r "Microsoft.WindowsAzure.Storage"

using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography;
using Microsoft.Azure.KeyVault;
using System.Configuration;
using System.Text; 
using System.Linq;
using System.Net;
using System.IO;

public static void Run(TimerInfo myTimer, IQueryable<bookingslot> inTable, IQueryable<isv> isvTable,  CloudTable outObj, TraceWriter log)
{
 
    List<bookingslot> thisBS = (from slot in inTable select slot).Where(e => e.StartDateTime < DateTime.Now && e.BookedToISV != "None").ToList();

    foreach(bookingslot loopBookingSlot in thisBS)
    {
       log.Info("The ISV for slot " + loopBookingSlot.BookingCode + " needs to be cleared as it is no longer required.");
      
       isv thisISV = (from isvs in isvTable select isvs).Where(e => e.PartitionKey == loopBookingSlot.PBE && e.RowKey == loopBookingSlot.BookedToISV && e.CurrentCode == loopBookingSlot.BookingCode).First(); 

       log.Info("The ISV for slot " + loopBookingSlot.BookingCode + " is found " + thisISV.Name);
    
       var operation = TableOperation.Delete(thisISV);
       outObj.ExecuteAsync(operation);

       log.Info("The ISV record for slot " + loopBookingSlot.BookingCode + " has been deleted");
     
    }
    
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

public class EncryptUtils
{
    public static TableRequestOptions GetReqOptions()
    {
        TableRequestOptions options = new TableRequestOptions();
        options.EncryptionPolicy = GetPolicy("private:thiskey12");
        return options;
    }

    public static TableEncryptionPolicy GetPolicy(string KeyString)
    {
        var rsacsp = new RSACryptoServiceProvider(2048);
        return new TableEncryptionPolicy(new RsaKey(KeyString, rsacsp), null);
    }

    private async static Task<string> GetToken(string authority, string resource, string scope)
    {
        var authContext = new AuthenticationContext(authority);
        ClientCredential clientCred = new ClientCredential(ConfigurationManager.AppSettings["clientId"], ConfigurationManager.AppSettings["clientSecret"]);
        AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);
        if (result == null) throw new InvalidOperationException("Failed to obtain the JWT token");

        return result.AccessToken;
    }

    public KeyVaultKeyResolver getResolver()
    {
        
        // The Resolver object is used to interact with Key Vault for Azure Storage.
        return new KeyVaultKeyResolver(GetToken);

    }   
} 