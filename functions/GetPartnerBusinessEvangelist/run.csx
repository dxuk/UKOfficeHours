#load "..\Shared\SharedData.csx"
using System.Net;

public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<partnerbusinessevangelist> inTable, TraceWriter log)
{

    // Return all entries in the ArticleHeader Table
    return req.CreateResponse(HttpStatusCode.OK, (from pbe in inTable select pbe).ToList());

}
