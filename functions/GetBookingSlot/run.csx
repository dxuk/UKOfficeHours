#load "..\Shared\httpUtils.csx"
#load "..\Domain\BookingSlot.csx"

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

    DateTime outdate = DateTime.Now.AddDays(90);
    IOrderedEnumerable<bookingslot> bslist;

    if (httpUtils.IsAuthenticated())
    {
        // Logged in employees can book anything not already booked
        bslist = (from slot in inTable select slot)
                .Where(e => 

                            e.StartDateTime >= DateTime.Now &&
                            e.StartDateTime <= outdate &&
                            e.BookedToISV == "None")
                .ToList()
                .OrderBy(e => e.StartDateTime);
                log.Info(bslist.Count().ToString());
    }
    else
    {
        // Don't show ADS' to customers to book directly 
        bslist = (from slot in inTable select slot)
                .Where(e => 
                            e.Duration < 120 && 
                            e.StartDateTime >= DateTime.Now &&
                            e.StartDateTime <= outdate &&
                            e.BookedToISV == "None")
                .ToList()
                .OrderBy(e => e.StartDateTime);
                log.Info(bslist.Count().ToString());

    }
        
    // Return all entries in the BookingSlot Table
    return req.CreateResponse(HttpStatusCode.OK, bslist);
    
}
