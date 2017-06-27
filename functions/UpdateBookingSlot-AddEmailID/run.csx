#load "..\Shared\httpUtils.csx"

#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Configuration;
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json; 

public static void Run(string myQueueItem, CloudTable tblbk, TraceWriter log)
{
    UpdateDTO mydto = JsonConvert.DeserializeObject<UpdateDTO>(myQueueItem);
   
    // Get the original record 
    TableOperation operationrd = TableOperation.Retrieve<bookingslot>(mydto.PartitionKey, mydto.RowKey);
    bookingslot bs = (bookingslot)tblbk.Execute(operationrd).Result;

    // Do the table update.
    bs.MailID = mydto.EventID; 

    TableOperation operation2 = TableOperation.Replace(bs);
    bookingslot bsout = (bookingslot)tblbk.Execute(operation2).Result;
    
}

public class UpdateDTO 
{
    public string PartitionKey {get;set;}
    public string RowKey {get;set;}
    public string EventID {get;set;}

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