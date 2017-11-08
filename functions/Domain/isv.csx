using System.Text;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.Storage.Table;

public class isv : TableEntity
{
    [EncryptProperty]
    public string Name { get; set; }
    [EncryptProperty]
    public string ContactName { get; set; }
    [EncryptProperty]
    public string ContactEmail { get; set; }

    public string ContactTopic { get; set; }
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