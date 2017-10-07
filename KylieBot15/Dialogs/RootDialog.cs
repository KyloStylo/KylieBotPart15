using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using KylieBot.Models;
using System.Net.Http;
using CRMApi.Models;
using System.Collections.Generic;
using Search.Services;
using Microsoft.Bot.Builder.Internals.Fibers;
using static Microsoft.Bot.Builder.Dialogs.PromptDialog;

namespace KylieBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private ISearchClient searchClient;
        public RootDialog(ISearchClient searchClient)
        {
            SetField.NotNull(out this.searchClient, nameof(searchClient), searchClient);
        }
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var userData = context.UserData;
            User retrieveUser = userData.GetValue<User>("User");

            Activity lastActivity = await result as Activity;
            bool AuthenticationAsked = userData.ContainsKey("AuthenticationAsked");

            if (lastActivity != null)
            {
                retrieveUser.searchTerm = lastActivity.Text;
                userData.SetValue<User>("User", retrieveUser);

                if (AuthenticationAsked)
                {
                    context.Call(new ProcessActionDialog(searchClient), ReSendIntro);
                }
            }

            if (!retrieveUser.WantsToBeAuthenticated && !AuthenticationAsked)
            {
                List<string> opts = new List<string>();
                opts.Add("Yes, let's authenticate");
                opts.Add("No need for that");
                Choice(context, MessageReceivedAsync, new PromptOptions<string>("Do you want to authenticate?", null, null, opts, 3, null));

                userData.SetValue<bool>("AuthenticationAsked", true);
                userData.SetValue<Activity>("lastActivity", lastActivity);
            }

            var x = "";

            if (lastActivity == null)
            {
                x = await result as string;
                if (x == "Yes, let's authenticate")
                {
                    retrieveUser.WantsToBeAuthenticated = true;
                    userData.SetValue<User>("User", retrieveUser);

                    if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
                    {
                        await context.Forward(new AzureAuthDialog(AuthSettings.Scopes), this.ResumeAfterAuth, userData.GetValue<Activity>("lastActivity"), CancellationToken.None);
                    }
                }
                else if (x == "Additional Info")
                {
                    var message = context.MakeMessage();

                    Attachment attachment = new HeroCard
                    {
                        Title = "About **Kylie Bot (KB)**",
                        Subtitle = "Intelligent Conversations",
                        Text = "*Empowered Knowledge Will Trivialise The Mundane*",
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Find Out More", value: "https://www.daringdynamics.co.uk"),
                                                                        new CardAction(ActionTypes.ImBack, "Back To Kylie Bot", value: "Back To Kylie Bot")
                                                                     }
                    }.ToAttachment();

                    message.Attachments.Add(attachment);

                    retrieveUser.searchTerm = "Back To Kylie Bot";
                    userData.SetValue<User>("User", retrieveUser);

                    await context.PostAsync(message);
                }
                else
                {
                    retrieveUser.searchTerm = x;
                    userData.SetValue<User>("User", retrieveUser);
                    context.Call(new ProcessActionDialog(searchClient), ReSendIntro);
                }
            }
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var userData = context.UserData;

            var message = await result;
            AuthResult lResult = result as AuthResult;
            User retrieveUser = userData.GetValue<User>("User");
            retrieveUser.Token = await context.GetAccessToken(AuthSettings.Scopes);
            userData.SetValue<User>("User", retrieveUser);

            await context.PostAsync(message);

            await getCRMContact(context, retrieveUser);

            context.Call(new ProcessActionDialog(searchClient), ReSendIntro);
        }

        private Task ReSendIntro(IDialogContext context, IAwaitable<object> result)
        {
            var userData = context.UserData;
            User retrieveUser = userData.GetValue<User>("User");

            if (retrieveUser.searchTerm != "I'm done for now")
            {
                List<string> options = new List<string>();
                options.Add("Empowered Search");
                options.Add("Additional Info");
                options.Add("I'm done for now");
                Choice(context, MessageReceivedAsync, new PromptOptions<string>("What else can I assist with?", null, null, options, 3, null));
                return Task.CompletedTask;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public async Task getCRMContact(IDialogContext context, User retrieveUser)
        {
            User user = retrieveUser;

            if (user != null)
            {
                HttpClient cons = new HttpClient();
                cons.BaseAddress = new Uri("https://TO DO.azurewebsites.net/");
                cons.DefaultRequestHeaders.Accept.Clear();
                cons.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                cons.Timeout = TimeSpan.FromMinutes(1);

                using (cons)
                {
                    HttpResponseMessage res = await cons.GetAsync("CRM/GetContact/'" + user.AADEmail.ToString() + "'/");
                    if (res.IsSuccessStatusCode)
                    {
                        CRMContact contact = await res.Content.ReadAsAsync<CRMContact>();
                        user.CRMContactId = contact.ContactId;
                        context.UserData.SetValue<User>("User", user);
                    }
                }
                cons.Dispose();
            }
        }
    }
}