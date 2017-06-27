using System.Net;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    LocalConfig myconf = new LocalConfig();
    
    // Return local configuration
    return req.CreateResponse(HttpStatusCode.OK, myconf);

}

public class LocalConfig
{

    public string ClientId = Environment.GetEnvironmentVariable("AzureAD_ClientID");
    public string TenantId = Environment.GetEnvironmentVariable("AzureAD_TenantID");
    public string Service_Description = Environment.GetEnvironmentVariable("Service_Description");
    public string MySite = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
    public string MyHost = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
    public string KeyVaultKeyName = Environment.GetEnvironmentVariable("KeyVaultKeyName");

}