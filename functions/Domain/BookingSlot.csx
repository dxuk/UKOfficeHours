#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Table;

public class bookingslot : TableEntity
{
    public bookingslot()
    {
        CreatedDateTime = DateTime.Now; 
    }
    
    public string MailID {get;set;}
    public string TechnicalEvangelist {get; set;}
    public string Topic {get; set;}
    public string BookedToISV { get; set; }
    public string BookingCode { get; set; }
    public string PBE { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime StartDateTime {get; set;}
    public DateTime EndDateTime {get; set;}

    public int Duration
    {
        get
        {
            TimeSpan ts = EndDateTime - StartDateTime;
            return ts.Minutes + (ts.Hours * 60);
        }
        set { }
    }
}
