using System;
using System.Collections.Generic;
using System.Configuration;
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
        public string email;
        public string phone;
        public string complaint;

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            // await this.ShowLuisResult(context, result);
            string message = "I'm sorry, currently i am not in a situation to answer your questions.";
            await context.PostAsync(message);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string message = "Glad to talk to you. Welcome to iBot - your Virtual Saudi Contractor Authority Consultant.";
            await context.PostAsync(message);

            var feedback = ((Activity)context.Activity).CreateReply("Let's start by choosing your preferred service?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "New Membership", Type=ActionTypes.PostBack, Value=$"Membership" },
                    new CardAction(){ Title = "Check Membership Details", Type=ActionTypes.PostBack, Value=$"CheckMembership" },
                    new CardAction(){ Title = "Customer Support", Type=ActionTypes.PostBack, Value=$"CustomerSupport" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(CRMProcessFlow);
        }
        public async Task CRMProcessFlow(IDialogContext context,IAwaitable<IMessageActivity> result)
        {
            var userFeedback = await result;

            if (userFeedback.Text.Contains("Membership"))
            {
                //context.Call(new FeedbackDialog("qnaURL", "userQuestion"), ResumeAfterFeedback);
                var feedback = ((Activity)context.Activity).CreateReply("Are you a Contractor?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes, I am a Contractor", Type=ActionTypes.PostBack, Value=$"YesContractor" },
                    new CardAction(){ Title = "No, I am not a Contractor but I am interested in becoming an SCA member", Type=ActionTypes.PostBack, Value=$"NoContractor" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(MessageReceivedAsync);
            }
            else if (userFeedback.Text.Contains("CheckMembership"))
            {
                PromptDialog.Text(
               context: context,
               resume: RegistrationCheck,
               prompt: "Enter your Contract Number to verify?",
               retry: "Sorry, I don't understand that.");
            }
            else if (userFeedback.Text.Contains("CustomerSupport"))
            {
                PromptDialog.Text(
           context: context,
           resume: Customer,
           prompt: "May i know your name please?",
           retry: "Sorry, I don't understand that.");
            }
        }
        [LuisIntent("ENQUIRY")]
        public async Task ENQUIRY(IDialogContext context, LuisResult result)
        {
            var feedback = ((Activity)context.Activity).CreateReply("Let's start by choosing your preferred service?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                      new CardAction(){ Title = "New Membership", Type=ActionTypes.PostBack, Value=$"Membership" },
                    new CardAction(){ Title = "Check Membership Details", Type=ActionTypes.PostBack, Value=$"CheckMembership" },
                    new CardAction(){ Title = "Customer Support", Type=ActionTypes.PostBack, Value=$"CustomerSupport" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(CRMProcessFlow);
        }
        [LuisIntent("CASE")]
        public async Task CASE(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: Customer,
            prompt: "May i know your name please?",
            retry: "Sorry, I don't understand that.");
        }
        public async Task Customer(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            customerName = response;

            PromptDialog.Text(
                context: context,
                resume: CustomerMobileNumber,
                prompt: "What is your complaint/suggestion? ",
                retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerMobileNumber(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            complaint = response;

            PromptDialog.Text(
                context: context,
                resume: CustomerEmail,
                prompt: "May I have your Mobile Number? ",
                retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerEmail(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            phone = response;

            PromptDialog.Text(
               context: context,
               resume: FinalResultHandler,
               prompt: "May I have your Email ID? ",
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

            //PromptDialog.Confirm(
            //context: context,
            //resume: AnythingElseHandler,
            //prompt: "Is there anything else that I could help?",
            //retry: "Sorry, I don't understand that.");
            //CRMConnection.CreateCase(complaint, customerName, phone, email);

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
        public async Task RegistrationCheck(IDialogContext context, IAwaitable<string> result)
        {
            var response = await result;

            await context.PostAsync("Your membership registration is under process. We will check your details and will get back to you shortly.");

            //PromptDialog.Confirm(
            //     context: context,
            //     resume: AnythingElseHandler,
            //     prompt: "Is there anything else that I could help?",
            //     retry: "Sorry, I don't understand that.");
            //CRMConnection.CreateLeadReg(customerName, email);

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
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var userFeedback = await result;

            if (userFeedback.Text.Contains("YesContractor"))
            {
                //context.Call(new FeedbackDialog("qnaURL", "userQuestion"), ResumeAfterFeedback);
                var feedback = ((Activity)context.Activity).CreateReply("Please choose below a category of your organization.");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes, I am a Saudi Organization", Type=ActionTypes.PostBack, Value=$"YesOrganization" },
                    new CardAction(){ Title = "No, I am a foreign or mixed nationality organization", Type=ActionTypes.PostBack, Value=$"Noorganization" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(ServiceMessageReceivedAsync);
            }
            else if (userFeedback.Text.Contains("NoContractor"))
            {
                //context.Call(new FeedbackDialog("qnaURL", "userQuestion"), ResumeAfterFeedback);
                var feedback = ((Activity)context.Activity).CreateReply("Please choose below a category of your organization.");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                       new CardAction(){ Title = "Yes, I am a Saudi Organization", Type=ActionTypes.PostBack, Value=$"YesOrganization" },
                    new CardAction(){ Title = "No, I am a foreign or mixed nationality organization", Type=ActionTypes.PostBack, Value=$"Noorganization" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(ServiceMessageReceivedAsync);
            }
        }
        public async Task ServiceMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var userFeedback = await result;

            if (userFeedback.Text.Contains("YesOrganization"))
            {
                PromptDialog.Text(
                context: context,
                resume: ComapanySize,
                prompt: "Enter your Commercial Registration Number?",
                retry: "Sorry, I don't understand that.");
            }
            else if (userFeedback.Text.Contains("Noorganization"))
            {
                PromptDialog.Text(
           context: context,
           resume: ComapanySize,
           prompt: "Enter your Commercial Registration Number?",
           retry: "Sorry, I don't understand that.");
            }
        }
        public async Task ComapanySize(IDialogContext context, IAwaitable<string> result)
        {
            var userFeedback = await result;


                //context.Call(new FeedbackDialog("qnaURL", "userQuestion"), ResumeAfterFeedback);
                var feedback = ((Activity)context.Activity).CreateReply("What is the size of your company?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "0-5 Employees", Type=ActionTypes.PostBack, Value=$"First" },
                    new CardAction(){ Title = "6-49 Employees", Type=ActionTypes.PostBack, Value=$"Second" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(RegistrationFinal);
        
        }
        public async Task RegistrationFinal(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var response = await result;

            await context.PostAsync("Thank you for your membership registration. We will check your details and will get back to you shortly.");

            //PromptDialog.Confirm(
            //     context: context,
            //     resume: AnythingElseHandler,
            //     prompt: "Is there anything else that I could help?",
            //     retry: "Sorry, I don't understand that.");
            //CRMConnection.CreateLeadReg(customerName, email);

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
            string message = "I'm sorry, currently i am not in a situation to answer your questions.";
            await context.PostAsync(message);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string message = "I'm sorry, currently i am not in a situation to answer your questions.";
            await context.PostAsync(message);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}