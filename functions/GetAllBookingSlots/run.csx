#load "..\Shared\SharedData.csx"
#load "..\Shared\httpUtils.csx"

using System.Net;

public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<bookingslot> inTable, TraceWriter log)
{
    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };

    // Return all entries in the ArticleHeader Table
    return req.CreateResponse(HttpStatusCode.OK, (from slot in inTable select slot).ToList().OrderBy(e => e.StartDateTime));

}
