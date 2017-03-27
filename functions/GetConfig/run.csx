using System.Net;
public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    myconfig myconf = new myconfig();
    // Return local configuration
    return req.CreateResponse(HttpStatusCode.OK, myconf);

}

public class myconfig
{
    public string ClientId = Environment.GetEnvironmentVariable("AzureAD_ClientID");
    public string TenantId = Environment.GetEnvironmentVariable("AzureAD_TenantID");
}
