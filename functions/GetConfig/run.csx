#load "..\Shared\SharedData.csx"
using System.Net;
public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    LocalConfig myconf = new LocalConfig();
    // Return local configuration
    return req.CreateResponse(HttpStatusCode.OK, myconf);

}