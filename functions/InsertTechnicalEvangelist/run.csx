#load "..\Shared\SharedData.csx"
using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<technicalevangelist> outQuestion, TraceWriter log)
{
    // Check SecCode ... 

    if (!httpUtils.IsAuthenticated()) { return req.CreateResponse(HttpStatusCode.Forbidden, "You have to be signed in!"); };

    dynamic data = await req.Content.ReadAsAsync<object>();

    outQuestion.Add(new Questions()
    {
        PartitionKey = data?.SessionDate,
        RowKey = Guid.NewGuid().ToString(),
        Question = data?.Question,
        Topic = data?.Topic,
        AskedBy = data?.AskedBy

    });
    return req.CreateResponse(HttpStatusCode.Created);

}