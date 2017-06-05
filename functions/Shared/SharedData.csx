#r "Microsoft.WindowsAzure.Storage"
//#r "Microsoft.Azure.KeyVault"

using System.Security.Cryptography;
using Microsoft.WindowsAzure.Storage.Table;
//using Microsoft.Azure.KeyVault;
using System.Text;
using System.Net;
using System.Linq;

// public class EncryptUtils
// {
//     public static TableEncryptionPolicy GetPolicy()
//     {
//         var rsacsp = new RSACryptoServiceProvider(2048);
       
//         TableRequestOptions options = new TableRequestOptions();
//         options.EncryptionPolicy = new TableEncryptionPolicy(new RsaKey("private:thiskey12", rsacsp), null);
//         return options;
        
//     }
// }

public class isv : TableEntity
{
   // [EncryptProperty]
    public string Name { get; set; }
   // [EncryptProperty]
    public string ContactName { get; set; }
   // [EncryptProperty]
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
public class technicalevangelist : TableEntity
{

    // RowKey is the User's Alias
    // PartitionKey is a static 'ALL' value
    //[EncryptProperty]
    public string SkypeLink {get; set;}
    public string TEName {get; set;}

}
public class partnerbusinessevangelist : TableEntity
{
    public string SkypeID {get; set;}

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


public class CompleteAppointmentDTO
{
    public string MailID {get;set;}

    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
    
    public int Duration {get;set;} 
    
    public string TEMail {get;set;}
    public string TEName {get;set;}
    public string TESkypeData {get;set;}    

    public string PBEMail {get;set;}

    public string ISVMail {get;set;}
    public string ISVName {get;set;}
    public string ISVContact {get;set;}
        
}

public class UserFeatureAuthorisation : TableEntity
{

    // PartitionKey = UserName
    // RowKey = Function / Permission
    public bool Authorised { get; set; }

}
