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

public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<bookingslot> inTable, TraceWriter log)
{
    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };

    IOrderedEnumerable<bookingslot> bslist = 
        (from slot in inTable select slot)
        .ToList()
        .OrderBy(e => e.StartDateTime);
        
    log.Info($"Found {bslist.Count().ToString()}");
    // Return all entries in the BookingSlot Table
    return req.CreateResponse(HttpStatusCode.OK, bslist);

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
