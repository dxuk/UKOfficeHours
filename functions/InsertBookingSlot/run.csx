#load "..\Shared\httpUtils.csx"

#r "Microsoft.ServiceBus"
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
 
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, IAsyncCollector<bookingslot> outputSbMsg, ICollector<bookingslot> outObj, TraceWriter log)
{

    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };
  
    log.Info(await httpUtils.GetRequestBodyAsString(req));
    bookingslot thisBS = await httpUtils.GetTFromJSONRequestBody<bookingslot>(req);

    thisBS.PartitionKey = thisBS.StartDateTime.Date.ToString("yyyyMM");
    thisBS.RowKey = Guid.NewGuid().ToString();

    thisBS.TechnicalEvangelist = httpUtils.GetCurrentUserEmailFromClaims();
 
    thisBS.BookedToISV = "None";
    thisBS.BookingCode = "None";
    thisBS.PBE = "None";
 
    log.Info(JsonConvert.SerializeObject(thisBS));
    outObj.Add(thisBS);

    await outputSbMsg.AddAsync(thisBS);

    return req.CreateResponse(HttpStatusCode.OK, thisBS);

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