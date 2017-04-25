#load "..\Shared\SharedData.csx"
#load "..\Shared\httpUtils.csx"
#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
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