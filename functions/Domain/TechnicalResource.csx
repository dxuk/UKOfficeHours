using System.Text;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.Storage.Table;

public class technicalevangelist : TableEntity
{
    // RowKey is the User's Alias
    // PartitionKey is a static 'ALL' value
    [EncryptProperty]
    public string SkypeLink {get; set;}
    public string TEName {get; set;}
}
