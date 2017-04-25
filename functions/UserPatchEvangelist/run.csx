#load "..\Shared\httpUtils.csx"
#load "..\Shared\SharedData.csx"

using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using System.Web;
 
public static void Run(string input, CloudTable outObj, IQueryable<bookingslot> inTable, TraceWriter log)
{

    // What happens when an admin needs to make an en-masse change to data, here's an 
    // example of what do do to reassign a batch of office hours to another
    // Email address - suppose someone leaves, or gets married ...

    // This is a manual function (it can only be triggered from the functions portal), and it is disabled by default in function.json
    
    string searchfor = "usertopatchfrom@microsoft.com";

    List<bookingslot> BS = (from slot in inTable select slot).Where(e => e.TechnicalEvangelist == searchfor).ToList();

    foreach(bookingslot boks in BS)
    {

        log.Info(boks.RowKey);
        boks.TechnicalEvangelist = "usertopatchto@microsoft.com";

        var operation = TableOperation.InsertOrReplace(boks);
        outObj.ExecuteAsync(operation);

    }
}
