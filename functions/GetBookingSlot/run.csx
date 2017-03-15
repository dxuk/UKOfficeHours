#load "..\Shared\SharedData.csx"
using System.Net;

public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<bookingslot> inTable, TraceWriter log)
{

    DateTime outdate = DateTime.Now.AddDays(90);

    // Return all entries in the ArticleHeader Table
    return req.CreateResponse(HttpStatusCode.OK,
        (from slot in inTable select slot).Where(
            e =>
                    e.StartDateTime >= DateTime.Now &&
                    e.StartDateTime <= outdate &&
                    e.BookedToISV == "None"
        ).ToList().OrderBy(e => e.StartDateTime)
    );
}
