#load "..\Shared\SharedData.csx"
#load "..\Shared\httpUtils.csx"

#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable tblbk, CloudTable tblisv, TraceWriter log)
{
    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };

    log.Info($"Incoming: {await httpUtils.GetRequestBodyAsString(req)}");

    bookingslot bsjs = await httpUtils.GetTFromJSONRequestBody<bookingslot>(req);

    log.Info($"Submitted slot PK:{bsjs.PartitionKey} RK:{bsjs.RowKey} BCode:{bsjs.BookingCode}");

    TableOperation operationrd = TableOperation.Retrieve<bookingslot>(bsjs.PartitionKey, bsjs.RowKey);
    bookingslot bs = (bookingslot)tblbk.Execute(operationrd).Result;


    log.Info($"Loaded slot PK:{bs.PartitionKey} RK:{bs.RowKey} BCode:{bs.BookingCode} BISVName:{bs.BookedToISV}");

    // ToDo: This is horribly inefficient consider there being over 100,000 isv's, for example this will be a NASTY speed lookup 
    // but we'll take it for now as we only have a few hundred registered ISVs present in one region.

    // Lookup matching isv record
    // ##########################

    // A: check it exists (i.e. that the booking code is valid.  
    // B: retrieve the ISV name. 

    isv queryisv = (from isv in tblisv.CreateQuery<isv>() select isv).Where(e => e.CurrentCode == bsjs.BookingCode).FirstOrDefault();
    log.Info($"Located ISV:{queryisv.Name} Code: {queryisv.CurrentCode}");

    // Check the code has not already been used on an existing booking

    bookingslot chkslt = (from slot in tblbk.CreateQuery<bookingslot>() select slot).Where(e => e.BookingCode == bsjs.BookingCode).FirstOrDefault();

    if (chkslt != null)
    {

        // Todo: Clean up UX and check this clientside too
        throw new InvalidOperationException("That code has already been used ! ");

    }
    else
    {
        // Update bs object with new values if not already booked.  
        if (bs.BookedToISV == "None")
        {

            bs.BookedToISV = queryisv.Name;
            bs.BookingCode = queryisv.CurrentCode;

            // Cache the PBE record on the Bookingslot to speed up searches from the portal
            bs.PBE = queryisv.PartitionKey;

            TableOperation operation2 = TableOperation.Replace(bs);
            bookingslot bsout = (bookingslot)tblbk.Execute(operation2).Result;

            log.Info($"Saved slot PK:{bsout.PartitionKey} RK:{bsout.RowKey} BCode:{bsout.BookingCode} BISVName:{bsout.BookedToISV}");

            return req.CreateResponse(HttpStatusCode.OK, bsout);

        }
        else
        {

            // This slot is already booked ! Error ! 
            return req.CreateResponse(HttpStatusCode.Conflict);

        }
    }
}
