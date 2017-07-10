#load "BasicForm.csx"

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

/// This dialog is the main bot dialog, which will call the Form Dialog and handle the results
[Serializable]
public class MainDialog : IDialog<BasicForm>
{
    public MainDialog()
    {
    }

    public Task StartAsync(IDialogContext context)
    {
        context.Wait(MessageReceivedAsync);
        return Task.CompletedTask;
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        context.Call(BasicForm.BuildFormDialog(FormOptions.PromptInStart), FormComplete);
    }

    private async Task FormComplete(IDialogContext context, IAwaitable<BasicForm> result)
    {
        try
        {
            var form = await result;
            if (form != null)
            {
                await context.PostAsync("Thanks for completing the form! Just type anything to restart it.");
  
                // // Create a queue Message
                // var queueMessage = new Message
                // {
                //     RelatesTo = context.Activity.ToConversationReference(),
                //     Text = message.Text
                // };

                // // Write the queue Message to the queue
                // // AddMessageToQueue() is a utility method you can find in the template
                // await AddMessageToQueue(JsonConvert.SerializeObject(queueMessage));
                // await context.PostAsync($"{this.count++}: You said {queueMessage.Text}. Message added to the queue.");
                // context.Wait(MessageReceivedAsync);
            
            }
            else
            {
                await context.PostAsync("Form returned empty response! Type anything to restart it.");
            }
        }
        catch (OperationCanceledException)
        {
            await context.PostAsync("You canceled the form! Type anything to restart it.");
        }

        context.Wait(MessageReceivedAsync);
    }
}