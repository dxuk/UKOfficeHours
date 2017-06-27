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
 
public static void Run(string input, CloudTable outObj, IQueryable<bookingslot> inTable, TraceWriter log)
{

    // What happens when an admin needs to make an en-masse change to data, here's an 
    // example of what do do to reassign a batch of office hours to another
    // Email address - suppose someone leaves, or gets married ... 

    // Note that this will do a full tablescan across all partitions to find the records.

    // This is a manual function (it can only be triggered from the functions portal), and it is disabled by default in function.json
    
    string searchfor = "usertopatchfrom@microsoft.com";

    List<bookingslot> BS = (from slot in inTable select slot).Where(e => e.TechnicalEvangelist == searchfor).ToList();

    foreach(bookingslot boks in BS)
    {

        log.Info(boks.RowKey);
        boks.TechnicalEvangelist = "usertopatchto@microsoft.com";

        var operation = TableOperation.InsertOrReplace(boks);
        outObj.ExecuteAsync(operation);

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