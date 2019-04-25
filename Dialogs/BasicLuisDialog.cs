using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public string customerName;
        public string complaint;
        public string email;
        public string phone;
        public string servicename;
        public string appointmentdate;
        public string appointmenttime;
        public string hospitalname;
        public string doctorname;
        private string name;
        public string specialist;

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string message = "I'm afraid I cannot help you with that. Please try with different keywords.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            if (customerName == null)
            {
                string message = "Glad to talk to you. Welcome to Virtual Customer Service.";
                await context.PostAsync(message);

                PromptDialog.Text(
                context: context,
                resume: CustomerNameFromGreeting,
                prompt: "May i know your Name please?",
                retry: "Sorry, I don't understand that.");
            }
            else
            {
                string message = "Tell me " + customerName + ". How i can help you?";
                await context.PostAsync(message);
            }
        }
        public async Task CustomerNameFromGreeting(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            customerName = response;

            string message = "Thanks " + customerName + ".Tell me. How i can help you?";
            await context.PostAsync(message);
        }
        [LuisIntent("CASE")]
        public async Task CASE(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: CustomerNameHandler,
            prompt: "What is your complaint/suggestion?",
            retry: "Sorry, I don't understand that.");
        }
        public virtual async Task CustomerNameHandler(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            complaint = response;

            PromptDialog.Text(
            context: context,
            resume: CustomerEmailHandler,
            prompt: "What is the best number to contact you?",
            retry: "Sorry, I don't understand that.");
        }
        public virtual async Task CustomerEmailHandler(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            phone = response;

            PromptDialog.Text(
            context: context,
            resume: FinalResultHandler,
            prompt: "What is your email id?",
            retry: "Sorry, I don't understand that.");
        }
        public virtual async Task FinalResultHandler(IDialogContext context, IAwaitable<string> argument)
        {
            string response = await argument;
            email = response;

            await context.PostAsync($@"Thank you for your interest, your request has been logged. Our customer service team will get back to you shortly.
                                    {Environment.NewLine}Your service request  summary:
                                   
                                    {Environment.NewLine}Complaint Title: {complaint},
                                    {Environment.NewLine}Customer Name: {customerName},
                                    {Environment.NewLine}Phone Number: {phone},
                                    {Environment.NewLine}Email: {email}");

            await context.PostAsync("One moment please");

            var activity = context.Activity as Activity;
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new System.Uri(activity.ServiceUrl));
                var isTyping = activity.CreateReply("Nerdibot is thinking...");
                isTyping.Type = ActivityTypes.Typing;
                await connector.Conversations.ReplyToActivityAsync(isTyping);

                // DEMO: I've added this for demonstration purposes, so we have time to see the "Is Typing" integration in the UI. Else the bot is too quick for us :)
                Thread.Sleep(2500);
            }
            var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AnythingElseHandler);
        }
        [LuisIntent("STATUS")]
        public async Task CaseStatus(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: CaseRefStatus,
            prompt: "Enter the Case Reference Number.",
            retry: "Didn't get that!");
        }
        public virtual async Task CaseRefStatus(IDialogContext context, IAwaitable<string> result)
        {
            var refno = await result;

            await context.PostAsync("Your complaint/Suggestion status is: In Progress");

            var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AnythingElseHandler);

        }
        [LuisIntent("APPOINTMENT")]
        public async Task APPOINTMENT(IDialogContext context, LuisResult result)
        {
            //PromptDialog.Text(
            //context: context,
            //resume: AppointmentIssue,
            //prompt: "Sure, May i know what is your issue?",
            //retry: "Didn't get that!");
            var feedback = ((Activity)context.Activity).CreateReply("Please choose a specialist.");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Allergist", Type=ActionTypes.PostBack, Value=$"Allergist" },
                    new CardAction(){ Title = "Cardiologist", Type=ActionTypes.PostBack, Value=$"Cardiologist" },
                      new CardAction(){ Title = "Dermatologist", Type=ActionTypes.PostBack, Value=$"Dermatologist" },
                        new CardAction(){ Title = "Family Physician", Type=ActionTypes.PostBack, Value=$"FamilyPhysician" },
                          new CardAction(){ Title = "Gastro Enterologist", Type=ActionTypes.PostBack, Value=$"GastroEnterologist" },
                            new CardAction(){ Title = "Pediatrician", Type=ActionTypes.PostBack, Value=$"Pediatrician" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AppointmentSpecialist);

        }
        public virtual async Task AppointmentSpecialist(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var result = await argument;
            specialist = result.Text;

            var feedback = ((Activity)context.Activity).CreateReply("Which specialist do you want to see?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                     new CardAction(){ Title = "Dr Nicole Graham", Type=ActionTypes.PostBack, Value=$"Nicole" },
                    new CardAction(){ Title = "Dr Nancy Paterson", Type=ActionTypes.PostBack, Value=$"Nancy" },
                      new CardAction(){ Title = "Dr Michael Smith", Type=ActionTypes.PostBack, Value=$"Michael" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AppointmentDate);
        }
        public virtual async Task AppointmentDate(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var result = await argument;
            doctorname = result.Text;

            var feedback = ((Activity)context.Activity).CreateReply("Here are few dates available. Let us know which date you want to make an appointment?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                     new CardAction(){ Title = "28th Apr 2019", Type=ActionTypes.PostBack, Value=$"28" },
                    new CardAction(){ Title = "30th Apr 2019", Type=ActionTypes.PostBack, Value=$"30" },
                      new CardAction(){ Title = "2nd May 2019", Type=ActionTypes.PostBack, Value=$"2nd" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AppointmentTime);
        }
        public virtual async Task AppointmentTime(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var result = await argument;
            appointmentdate = result.Text;

            var feedback = ((Activity)context.Activity).CreateReply("Here are few schedule timings available. Let us know which time you want to make an appointment?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                     new CardAction(){ Title = "2 PM", Type=ActionTypes.PostBack, Value=$"2" },
                    new CardAction(){ Title = "5 PM", Type=ActionTypes.PostBack, Value=$"5" },
                      new CardAction(){ Title = "7 PM", Type=ActionTypes.PostBack, Value=$"7" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(FinalAppointment);
        }
        public virtual async Task FinalAppointment(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var result = await argument;
            appointmenttime = result.Text;

            await context.PostAsync($@"Thank you for your interest, your appointment has been logged.
                                    {Environment.NewLine}Your Appointment  summary:                                   
                                   {Environment.NewLine}Name : {customerName},
                                    {Environment.NewLine}Spcialist : {specialist},
                                    {Environment.NewLine}Doctor : {doctorname},
                                    {Environment.NewLine}Appointment Date: {appointmentdate},
                                    {Environment.NewLine}Appointment Time: {appointmenttime}");

            await context.PostAsync("One moment please");

            var activity = context.Activity as Activity;
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new System.Uri(activity.ServiceUrl));
                var isTyping = activity.CreateReply("Nerdibot is thinking...");
                isTyping.Type = ActivityTypes.Typing;
                await connector.Conversations.ReplyToActivityAsync(isTyping);

                // DEMO: I've added this for demonstration purposes, so we have time to see the "Is Typing" integration in the UI. Else the bot is too quick for us :)
                Thread.Sleep(2500);
            }
            var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AnythingElseHandler);
        }
        public async Task AnythingElseHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var answer = await argument;
            if (answer.Text.Contains("Yes"))
            {
                await GeneralGreeting(context, null);
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);

                //var survey = context.MakeMessage();

                //var attachment = GetSurveyCard();
                //survey.Attachments.Add(attachment);

                //await context.PostAsync(survey);

                context.Done<string>("conversation ended.");
            }
        }
        public virtual async Task GeneralGreeting(IDialogContext context, IAwaitable<string> argument)
        {
            string message = $"Great! What else that can I help you?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string message = "I'm afraid I cannot help you with that. Please try with different keywords.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string message = "I'm afraid I cannot help you with that. Please try with different keywords.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}