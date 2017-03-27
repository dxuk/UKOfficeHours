using System.Net;
public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    myconfig myconf = new myconfig;
    // Return local configuration
    return req.CreateResponse(HttpStatusCode.OK, myconf);

}

public class myconfig
{
    public string clientid = Environment.GetEnvironmentVariable("AzureAD_ClientID")
    public string tenantid = Environment.GetEnvironmentVariable("AzureAD_TenantID")
}