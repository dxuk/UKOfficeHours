#load "..\Shared\SharedData.csx"

#r "Newtonsoft.Json"

using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security;
using Newtonsoft.Json;

public class httpUtils
{
    public static async Task<Dictionary<string, string>> GetDictFromFormValuesAsync(HttpRequestMessage req)
    {
        string formdata = await req.Content.ReadAsStringAsync();

        Dictionary<string, string> formDictionary = new Dictionary<string, string>();

        foreach (string strformitem in formdata.Split('&'))
        {

            string[] strsplit = strformitem.Split('=');
            string formkey = WebUtility.UrlDecode(strsplit[0]);
            string formvalue = WebUtility.UrlDecode(strsplit[1]);

            formDictionary.Add(formkey, formvalue);
        }

        return formDictionary;
    }

    public static Dictionary<string, string> GetDictFromQueryStringValues(HttpRequestMessage req)
    {

        return req.GetQueryNameValuePairs().ToDictionary(keypair => keypair.Key, keypair => keypair.Value);

    }



    public static async Task<string> GetRequestBodyAsString(HttpRequestMessage req)
    {

        return await req.Content.ReadAsStringAsync();

    }
    public static async Task<T> GetTFromJSONRequestBody<T>(HttpRequestMessage req)
    {

        return JsonConvert.DeserializeObject<T>(await req.Content.ReadAsStringAsync());

    }


    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        return ClaimsPrincipal.Current;
    }

    public static string GetCurrentUserEmailFromClaims()
    {
        return ClaimsPrincipal.Current.Claims.Where(e => e.Type == ClaimTypes.Name).FirstOrDefault().Value;
    }

    public static bool IsAuthenticated()
    {
        return (ClaimsPrincipal.Current.Claims.Count() > 0);
    }

    public static bool IsAuthorised()
    {
        // Override with your authorisation logic, at the moment everyone in your tenant (including guests) can do all application features
        // Which for our use case is what we need - but you will have to either connect to the AD Graph API, or use a table to query for user permissions.
        return true;

    }

    public static void EnsureAuthenticatedOrRedirect()
    {
        if (ClaimsPrincipal.Current.Claims.Count() == 0)
        {

            throw new SecurityException("You aren't allowed to do this! You should be logged in with an appropriate token!");

        }
    }

    public static IList<Claim> GetListOfClaims()
    {
        return ClaimsPrincipal.Current.Claims.ToList<Claim>();
    }

    public static void AuthDiags(TraceWriter log)
    {

        log.Info($"There are {httpUtils.GetListOfClaims().Count().ToString()} claims in this {httpUtils.GetClaimsPrincipal().ToString()} object ");

        foreach (Claim claim in httpUtils.GetListOfClaims())
        {
            log.Verbose("Claim:" + claim.Type + "= " + claim.Value);
        }

    }
}