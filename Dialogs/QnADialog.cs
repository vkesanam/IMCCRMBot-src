using Microsoft.Bot.Builder.Dialogs;
using QnAMakerDialog;
using QnAMakerDialog.Models;
using System;
using System.Threading.Tasks;

namespace LuisBot.Dialogs
{
    [Serializable]
    [QnAMakerService("6ea60bb6-c790-4e56-abbf-0a6bd96bff47", "01b06abf-cf9d-44fe-b668-da090a209f43")]
    public class QnADialog : QnAMakerDialog<object>
    {
        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'.");
            context.Wait(MessageReceived);
        }

        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            await context.PostAsync($"I found an answer that might help...{result.Answers[0].Answer}.");
            context.Wait(MessageReceived);
        }
    }
}