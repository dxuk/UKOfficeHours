#load "..\Shared\SharedData.csx"
#load "..\Shared\httpUtils.csx"

using System.Net;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<isv> outObj, TraceWriter log)
{

    // httpUtils.AuthDiags(log); 
    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };

    isv thisISV = await httpUtils.GetTFromJSONRequestBody<isv>(req);

    thisISV.RowKey = thisISV.Name;
    thisISV.PartitionKey = httpUtils.GetCurrentUserEmailFromClaims();

    if (thisISV.CurrentCode == "" || thisISV.CurrentCode == null) { thisISV.AddUniqueAlphaNumCodeAndSave(); }

    outObj.Add(thisISV);
    return req.CreateResponse(HttpStatusCode.OK, thisISV);

}
