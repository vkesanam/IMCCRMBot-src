using LuisBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class IMCDailog : LuisDialog<object>
    {
        public string Symptom;
        public string SymptomID;

        public IMCDailog() : base(new LuisService(new LuisModelAttribute(
           ConfigurationManager.AppSettings["LuisAppId"],
           ConfigurationManager.AppSettings["LuisAPIKey"],
           domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            string message = "I'm afraid I cannot help you with that. Please try with different keywords.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);

        }
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            string message = "Glad to talk to you. Welcome to Virtual Customer Service.";
            await context.PostAsync(message);

            var feedback = ((Activity)context.Activity).CreateReply("Lets start with your preferred Service?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                     new CardAction(){ Title = "Appointment", Type=ActionTypes.PostBack, Value=$"Appointment" },
                     new CardAction(){ Title = "Customer Service", Type=ActionTypes.PostBack, Value=$"Service" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(CustomerName);
        }
        public async Task CustomerName(IDialogContext context, IAwaitable<IMessageActivity> aregument)
        {
            var result = await aregument;
            if (result.Text.Contains("Appointment"))
            {
              PromptDialog.Text(
              context: context,
              resume: SymptomCheck,
              prompt: "What concerns you most about your health? Please describe your symptoms?",
              retry: "Sorry, I don't understand that.");
            }
            else if (result.Text.Contains("Service"))
            {
             //   PromptDialog.Text(
             //context: context,
             //resume: CustomerNameFromGreetingArabic,
             //prompt: "هل لي ان اعرف اسمك من فضلك ؟",
             //retry: "Sorry, I don't understand that.");
            }

        }
        public async Task SymptomCheck(IDialogContext context, IAwaitable<string> argument)
        {
            var result = await argument;
            SymptomID=executeParsingAPI(result);

            await context.PostAsync("Okay, let me ask you a couple of questions.");
            context.Wait(MessageReceived);

            PromptDialog.Number(
             context: context,
             resume: AgeCheck,
             prompt: "How old are you?",
             retry: "Sorry, I don't understand that.");

           
        }

        private static string executeParsingAPI(string result)
        {
            string SymptomID = "";
            try
            {

                string url = "https://api.infermedica.com/v2/parse";

                var jsondata = new
                {
                    text = "fever"
                };
                string inputJson = (new JavaScriptSerializer()).Serialize(jsondata);
                byte[] data = Encoding.UTF8.GetBytes(inputJson);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                // turn our request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(inputJson);

                // this is important - make sure you specify type this way
                request.ContentType = "application/json; charset=UTF-8";
                request.Headers.Add("App-Id", "00f45dc3");
                request.Headers.Add("App-Key", "dc46a176996d0ffbc591052812a9acbe");
                request.Accept = "application/json";
                request.ContentLength = postBytes.Length;
                //request.CookieContainer = Cookies;
                //request.UserAgent = currentUserAgent;
                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                // grab te response and print it out to the console along with the status code
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string finalResult;

                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    finalResult = rdr.ReadToEnd();

                    Parsing res = (new JavaScriptSerializer()).Deserialize<Parsing>(finalResult);

                    if (res.mentions.Count >= 0)
                    {
                        foreach (var men in res.mentions)
                        {
                            if(men.choice_id=="present")
                            {
                                SymptomID = men.id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return SymptomID;
        }

        public async Task AgeCheck(IDialogContext context,IAwaitable<long> PatientAge)
        {
            long Age = await PatientAge;

            var feedback = ((Activity)context.Activity).CreateReply("Are you female or male?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                     new CardAction(){ Title = "Male", Type=ActionTypes.PostBack, Value=$"Male" },
                     new CardAction(){ Title = "Female", Type=ActionTypes.PostBack, Value=$"Female" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(MessageReceived);
        }
    }
}