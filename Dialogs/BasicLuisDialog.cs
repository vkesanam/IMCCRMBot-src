using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LuisBot;
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
        public static string host = "https://api.microsofttranslator.com";
        public static string path = "/V2/Http.svc/Translate";

        // NOTE: Replace this example key with a valid subscription key.
        public static string key = "830fda84bdce4810a78cc508745a2f9e";

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        private async Task<string> Translation(string text)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            string uri = host + path + "?from=ar-ae&to=en-us&text=" + System.Net.WebUtility.UrlEncode(text);

            HttpResponseMessage response = await client.GetAsync(uri);

            string result = await response.Content.ReadAsStringAsync();
            var content = XElement.Parse(result).Value;
            return content;
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
            //if (customerName == null)
            //{
                string message = "Glad to talk to you. Welcome to Virtual Customer Service.";
                await context.PostAsync(message);

                var feedback = ((Activity)context.Activity).CreateReply("Which Language you want to prefer?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "English", Type=ActionTypes.PostBack, Value=$"English" },
                    new CardAction(){ Title = "Arabic", Type=ActionTypes.PostBack, Value=$"Arabic" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(CustomerName);

               
            //}
            //else
            //{
            //    string message = "Tell me " + customerName + ". How i can help you?";
            //    await context.PostAsync(message);
            //}
        }
        public async Task CustomerName(IDialogContext context, IAwaitable<IMessageActivity> aregument)
        {
            var result = await aregument;
            if(result.Text.Contains("English"))
            {
                PromptDialog.Text(
              context: context,
              resume: CustomerNameFromGreeting,
              prompt: "May i know your Name please?",
              retry: "Sorry, I don't understand that.");
            }
            else if(result.Text.Contains("Arabic"))
            {
                PromptDialog.Text(
             context: context,
             resume: CustomerNameFromGreetingArabic,
             prompt: "هل لي ان اعرف اسمك من فضلك ؟",
             retry: "Sorry, I don't understand that.");
            }
           
        }
        public async Task CustomerNameFromGreeting(IDialogContext context, IAwaitable<string> result)
        {
            var feedback = ((Activity)context.Activity).CreateReply("Which Service you want to prefer?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Appointment", Type=ActionTypes.PostBack, Value=$"Appointment" },
                    new CardAction(){ Title = "Location", Type=ActionTypes.PostBack, Value=$"Location" },
                     new CardAction(){ Title = "Customer Support", Type=ActionTypes.PostBack, Value=$"Service" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(Main);
        }
        public async Task Main(IDialogContext context, IAwaitable<IMessageActivity> aregument)
        {
            var result = await aregument;
            if (result.Text.Contains("Appointment"))
            {
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
            else if (result.Text.Contains("Location"))
            {
                PromptDialog.Text(
                   context: context,
                   resume: DisplaySelectedCard,
                   prompt: "Please let me know your location preference?",
                   retry: "Sorry, I don't understand that."
                           );
            }
            else if (result.Text.Contains("Service"))
            {
                PromptDialog.Text(
            context: context,
            resume: CustomerNameHandler,
            prompt: "What is your complaint/suggestion?",
            retry: "Sorry, I don't understand that.");
            }
        }
                public async Task CustomerNameFromGreetingArabic(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            customerName = response;

            string message = "شكرا أخبريني كيف يمكنني مساعدتك ؟";
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

            CRMConnection.CreateCase(complaint, customerName, phone, email);

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
        [LuisIntent("LOCATION")]
        public async Task LOCATION(IDialogContext context, LuisResult result)
        {
            EntityRecommendation employeeName;

            string name = string.Empty;

            if (result.TryFindEntity("Location", out employeeName))
            {
                name = employeeName.Entity;

                var message = context.MakeMessage();

                var attachment = GetSelectedCard(name);
                message.Attachments.Add(attachment);

                await context.PostAsync(message);

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
            else
            {
                PromptDialog.Text(
                    context: context,
                    resume: DisplaySelectedCard,
                    prompt: "Please let me know your location preference?",
                    retry: "Sorry, I don't understand that."
                            );
            }
        }
        public async Task DisplaySelectedCard(IDialogContext context, IAwaitable<string> result)
        {
            var selectedCard = await result;

            var message = context.MakeMessage();

            var attachment = GetSelectedCard(selectedCard);
            message.Attachments.Add(attachment);

            await context.PostAsync(message);

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
        private static Microsoft.Bot.Connector.Attachment GetSelectedCard(string selectedCard)
        {
            if (selectedCard.Contains("mankhool") || selectedCard.Contains("Mankhool") || selectedCard.Contains("al mankhool") || selectedCard.Contains("Al Mankhool"))
            {
                return GetMankhoolCard();
            }
            else if (selectedCard.Contains("barsha") || selectedCard.Contains("Barsha") || selectedCard.Contains("Al Barsha") || selectedCard.Contains("al barsha"))
            {
                return GetAlBarshaCard();
            }
            else if (selectedCard.Contains("Al Qusais") || selectedCard.Contains("qusais") || selectedCard.Contains("Qusais"))
            {
                return GetQusaisCard();
            }
            else if (selectedCard.Contains("Nad Al Hamar") || selectedCard.Contains("nad al hamar"))
            {
                return GetNadAlHamarCard();
            }
            else if (selectedCard.Contains("Al Safa") || selectedCard.Contains("al safa"))
            {
                return GetAlSafaCard();
            }
            else if (selectedCard.Contains("Al Badaa") || selectedCard.Contains("al badaa"))
            {
                return GetAlBadaaCard();
            }
            return GetDefaultCard();
        }
        private static Microsoft.Bot.Connector.Attachment GetAlBadaaCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/AlBadaa.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/place/Al+Badaa+Health+Centre/@25.2114413,55.2724694,17z/data=!4m8!1m2!2m1!1sAl+Badaa+Health+Center!3m4!1s0x3e5f42e8b1e728f1:0x9386335357fb7552!8m2!3d25.2120592!4d55.2722835") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetAlSafaCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/AlSafa.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/place/Al+Safa+Primary+Health+Care+Center/@25.1896,55.2362383,17z/data=!3m1!4b1!4m5!3m4!1s0x3e5f4208f522d93f:0x4c43896c0d306972!8m2!3d25.1896!4d55.238427") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetNadAlHamarCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/NadAlHamar.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/place/Nadd+Al+Hamar+Health+Center/@25.199364,55.3745396,16z/data=!4m8!1m2!2m1!1sNad+Al+Hamar+Clinic!3m4!1s0x3e5f6715c4716b27:0xbd16bd0f728ddd3a!8m2!3d25.199364!4d55.378917") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetMankhoolCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/DMankhool.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/''/@25.2459475,55.2868377,14.83z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f4321e1e53069:0x218a7bfc58fbcf0e!2m2!1d55.2923198!2d25.2466252") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetAlBarshaCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/DAlBarsha.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/AL+Barsha+Health+Centre+-+Al+Barsha+3+-+Dubai/@25.095679,55.1989079,13.7z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f6b8b20ab6a27:0xc41e11236896cc64!2m2!1d55.2024733!2d25.0975157") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetDefaultCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/DDHAHeadOffice.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/DHA+HEAD+OFFICE+-+Dubai/@25.243802,55.3158075,13.42z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f5d2967d29a6d:0xcd99cea2aee34140!2m2!1d55.3196429!2d25.2468578") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetQusaisCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://intertechealthwebapp.azurewebsites.net/AlQusais.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/DHA+HEAD+OFFICE+-+Dubai/@25.243802,55.3158075,13.42z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f5d2967d29a6d:0xcd99cea2aee34140!2m2!1d55.3196429!2d25.2468578") }
            };

            return heroCard.ToAttachment();
        }
        [LuisIntent("ENQUIRY")]
        public async Task ENQUIRY(IDialogContext context, LuisResult result)
        {
           

            var feedback = ((Activity)context.Activity).CreateReply("Let us know what are you interested in?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Appointments", Type=ActionTypes.PostBack, Value=$"Appointments" },
                    new CardAction(){ Title = "Search Hospitals", Type=ActionTypes.PostBack, Value=$"Search" },
                    new CardAction(){ Title = "Suggestions and Complaints", Type=ActionTypes.PostBack, Value=$"Suggestions" },
                    new CardAction(){ Title = "Status", Type=ActionTypes.PostBack, Value=$"Status" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(ResumeTypeSubOptionsAsync);
        }
        public virtual async Task ResumeTypeSubOptionsAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var selection = await argument;
            if (selection.Text.Contains("Suggestions"))
            {
                PromptDialog.Text(
                 context: context,
                 resume: CustomerNameHandler,
                 prompt: "What is your complaint/suggestion?",
                 retry: "Sorry, I don't understand that the question."
             );
            }
            else if (selection.Text.Contains("Search"))
            {
                PromptDialog.Text(
                context: context,
                resume: DisplaySelectedCard,
                prompt: "Please let me know your location preference?",
                retry: "Sorry, I don't understand that."
                        );
            }
            else if (selection.Text.Contains("Appointments"))
            {
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
            else if (selection.Text.Contains("Status"))
            {
                PromptDialog.Text(
           context: context,
           resume: CaseRefStatus,
           prompt: "Enter the Case Reference Number.",
           retry: "Didn't get that!");
            }
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