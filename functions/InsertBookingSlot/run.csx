#load "..\Shared\SharedData.csx"
#load "..\Shared\httpUtils.csx"
#r "Newtonsoft.Json"
using System.Net;
using System.Web;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<bookingslot> outObj, TraceWriter log)
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

    return req.CreateResponse(HttpStatusCode.OK, thisBS);

}