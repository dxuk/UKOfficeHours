#load "..\Shared\httpUtils.csx"
#load "..\Domain\TechnicalResource.csx"
#load "..\Domain\BookingSlot.csx"
#load "..\Domain\isv.csx"
#load "..\Domain\CompleteAppointment.csx"

#r "Microsoft.ServiceBus"
#r "Microsoft.WindowsAzure.Storage"

using System;    
using System.Threading; 
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure; 
using Microsoft.Azure.KeyVault; 
using Microsoft.Azure.KeyVault.Core; 
using System.Text;
using System.Web;
using System.Web.Http;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json; 
using Microsoft.WindowsAzure.Storage.Table.Queryable;
 
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable tblbk, CloudTable tblisv, CloudTable tblte, IAsyncCollector<CompleteAppointmentDTO> outputSbMsg, TraceWriter log)
{
    // if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };
  
    log.Info($"Incoming: {await httpUtils.GetRequestBodyAsString(req)}");

    bookingslot bsjs = await httpUtils.GetTFromJSONRequestBody<bookingslot>(req);

    log.Info($"Submitted slot PK:{bsjs.PartitionKey} RK:{bsjs.RowKey} BCode:{bsjs.BookingCode}");

    TableOperation operationrd = TableOperation.Retrieve<bookingslot>(bsjs.PartitionKey, bsjs.RowKey);
    bookingslot bs = (bookingslot)tblbk.Execute(operationrd).Result;

    log.Info($"Loaded slot PK:{bs.PartitionKey} RK:{bs.RowKey} BCode:{bs.BookingCode} BISVName:{bs.BookedToISV}");

    // ToDo: This is horribly inefficient consider there being over 100,000 isv's, for example this will be a NASTY speed lookup 
    // but we'll take it for now as we only have a few registered ISVs present in one region.

    // What we SHOULD do here is either use the CosmosDB Table storage API to get an automatic Index
    // Or we should create a new table where the RowKey is the Code and lookup in that.

    // Lookup matching isv record
    // ##########################

    // A: check it exists (i.e. that the booking code is valid.  
    // B: retrieve the ISV name. 

    isv queryisv = (from isv in tblisv.CreateQuery<isv>() select isv).Where(e => e.CurrentCode == bsjs.BookingCode).WithOptions(GetTableRequestOptionsWithEncryptionPolicy()).FirstOrDefault();
    


    if (queryisv != null)
    {
        log.Info($"Located ISV:{queryisv.Name} Code: {queryisv.CurrentCode}");  
    }
    else
    {
        log.Info($"No ISV found with code, returning 404: {bsjs.BookingCode}"); 

        // Todo: Clean up UX and check this clientside too
        return req.CreateResponse(HttpStatusCode.NotFound, $"No ISV found with code: {bsjs.BookingCode}");

    }

    // Check the code has not already been used on an existing booking

    bookingslot chkslt = (from slot in tblbk.CreateQuery<bookingslot>() select slot).Where(e => e.BookingCode == bsjs.BookingCode).FirstOrDefault();

    if (chkslt != null)

    {

        log.Info($"Found existing booking for that code: {bsjs.BookingCode}"); 

        // Todo: Clean up UX and check this clientside too
        return req.CreateResponse(HttpStatusCode.Conflict, $"That code has already been used once! {bsjs.BookingCode}");

    }

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

            // Compile and Write the mail record away to the ServiceBus if this is enabled for this TE (if the TE master record is populated);

            // Find the TE master data record 
            technicalevangelist chkte = (from te in tblte.CreateQuery<technicalevangelist>() select te).Where(e => e.RowKey == bsout.TechnicalEvangelist && e.PartitionKey == "ALL").FirstOrDefault();
            if (chkte != null)
            {
                log.Info($"Found TE : {chkte.TEName}");
                // Build the DTO ready for sending

                CompleteAppointmentDTO co = new CompleteAppointmentDTO() 
                {
                    MailID = bsout.MailID,
                    StartDate = bsout.StartDateTime,
                    EndDate = bsout.EndDateTime,
                    Duration = bsout.Duration,
                    TEMail = bsout.TechnicalEvangelist,
                    TEName = chkte.TEName,
                    TESkypeData = chkte.SkypeLink, 
                    PBEMail = bs.PBE,
                    ISVMail = queryisv.ContactEmail,
                    ISVName = queryisv.Name,
                    ISVContact = queryisv.ContactName 
                };

                // Write it away to queue the email invite async from the TE's flow
                await outputSbMsg.AddAsync(co);
                log.Info($"Output successful");
            }
            else
            {
                log.Info($"No TE Configuration found for {bsout.TechnicalEvangelist}, SKIPPING email INVITE");
            }

            return req.CreateResponse(HttpStatusCode.OK, bsout);

        }
        else
        {
            log.Info($"Slot not available, returning 409 conflict : {bsjs.BookingCode}"); 
            // This slot is already booked ! Error !
            return req.CreateResponse(HttpStatusCode.Conflict);

        }
    }

public static TableRequestOptions GetTableRequestOptionsWithEncryptionPolicy()
{
    KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(async (string authority, string resource, string scope) => {
            ClientCredential credential = new ClientCredential(Environment.GetEnvironmentVariable("KVClientId"), Environment.GetEnvironmentVariable("KVKey"));
            AuthenticationContext ctx = new AuthenticationContext(new Uri(authority).AbsoluteUri, false);
            AuthenticationResult result = await ctx.AcquireTokenAsync(resource, credential);
            return result.AccessToken;}
    );
    IKey cloudKey1 = cloudResolver.ResolveKeyAsync(Environment.GetEnvironmentVariable("KVKeyID"), CancellationToken.None).GetAwaiter().GetResult(); 
    TableEncryptionPolicy encryptionPolicy = new TableEncryptionPolicy(cloudKey1, cloudResolver);
    TableRequestOptions rqOptions = new TableRequestOptions() { EncryptionPolicy = encryptionPolicy };
    return rqOptions; 
}
