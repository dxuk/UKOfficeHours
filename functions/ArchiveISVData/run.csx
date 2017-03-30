#load "..\Shared\httpUtils.csx"

using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using System.Web;

public static void Run(TimerInfo myTimer, CloudTable outObj, IQueryable<bookingslot> inTable, IQueryable<isv> isvTable, TraceWriter log)
{

    List<bookingslot> thisBS = (from slot in inTable select slot).Where(e => e.StartDateTime < DateTime.Now && e.BookedToISV != "None").ToList();

    foreach(bookingslot loopBookingSlot in thisBS)
    {
        log.Info("The ISV for slot " + loopBookingSlot.BookingCode + " needs to be cleared as it is no longer required.");
      
        isv thisISV = (from isvs in isvTable select isvs).Where(e => e.PartitionKey == loopBookingSlot.PBE && e.RowKey == loopBookingSlot.BookedToISV && e.CurrentCode == loopBookingSlot.BookingCode).First(); 

        log.Info("The ISV for slot " + loopBookingSlot.BookingCode + " is found " + thisISV.Name);
    
        var operation = TableOperation.Delete(thisISV);
        outObj.ExecuteAsync(operation);

        log.Info("The ISV record for slot " + loopBookingSlot.BookingCode + " has been deleted");

    }
    
}
