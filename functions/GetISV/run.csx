#load "..\Shared\SharedData.csx"
#load "..\Shared\httpUtils.csx"
using System.Net;

public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<isv> inTable, TraceWriter log)
{

    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };

    var lookupKey = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "ISVCode", true) == 0).Value;

    // Return the requested ISV Code.
    return req.CreateResponse(HttpStatusCode.OK, (from article in inTable select article).Where(e => e.CurrentCode == lookupKey).ToList());

}

