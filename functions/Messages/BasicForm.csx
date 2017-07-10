using System;
using Microsoft.Bot.Builder.FormFlow;

public enum SessionTypeOptions { Concierge = 1, OfficeHour, Hack, ADS, Event, Webinar};

// For more information about this template visit http://aka.ms/azurebots-csharp-form
[Serializable]
public class BasicForm 
{
    [Prompt("Hi! What is the customer (ISV) {&}?")]
    public string Name { get; set; }

    [Prompt("What type of session are you currently in {||}")]
    public SessionTypeOptions SessionType { get; set; }

    [Prompt("What is the context of the session {||}")]
    public string SessionContext { get; set; }
 
    public static IForm<BasicForm> BuildForm()
    {
        // Builds an IForm<T> based on BasicForm
        return new FormBuilder<BasicForm>().Build();
    }

    public static IFormDialog<BasicForm> BuildFormDialog(FormOptions options = FormOptions.PromptInStart)
    {
        // Generated a new FormDialog<T> based on IForm<BasicForm>
        return FormDialog.FromForm(BuildForm, options);
    }

}
