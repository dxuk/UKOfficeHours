#load "..\Shared\httpUtils.csx"
#load "..\Domain\BookingSlot.csx"
#load "..\Domain\isv.csx"
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

public static void Run(TimerInfo myTimer, IQueryable<bookingslot> inTable, IQueryable<isv> isvTable,  CloudTable outObj, CloudTable outputBookingSlots, TraceWriter log)
{
 
    List<bookingslot> thisBS = (from slot in inTable select slot).Where(e => e.StartDateTime < DateTime.Now && e.BookedToISV != "None" && e.BookingCode != "Archived").ToList();
    
    bookingslot outerBookingSlot = null;  
    
    foreach(bookingslot loopBookingSlot in thisBS)
    {
       log.Warning("The ISV for slot " + loopBookingSlot.BookingCode + "(" + loopBookingSlot.BookedToISV + ") needs to be cleared as it is no longer required.");
       
       try
       {    
              outerBookingSlot = loopBookingSlot; 

              isv thisISV = (from isvs in isvTable select isvs).Where(e => e.PartitionKey == loopBookingSlot.PBE && e.RowKey.StartsWith(loopBookingSlot.BookedToISV) && e.CurrentCode == loopBookingSlot.BookingCode).First(); 
       
              var operation = TableOperation.Delete(thisISV);
              outObj.ExecuteAsync(operation);
              
              log.Info("The ISV record for slot " + loopBookingSlot.BookingCode + " has been deleted");                           
              
       }
       catch (Exception ex) 
       {

            log.Error("Error looking up an ISV record for slot" + loopBookingSlot.BookingCode + " (" + loopBookingSlot.BookedToISV + ") :" + ex.InnerException.Message);
       
       }
       finally
       {

              log.Warning("The Booking Slot record for slot " + outerBookingSlot.BookingCode + " needs to be updated to remove the booking code.");

              outerBookingSlot.BookingCode = "Archived";
              
              var operationbooking = TableOperation.Merge(outerBookingSlot);
              outputBookingSlots.ExecuteAsync(operationbooking);

              log.Info("The Booking Slot record for slot " + outerBookingSlot.BookingCode + " has been updated to remove the booking code.");
       }
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