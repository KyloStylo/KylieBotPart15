using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KylieBot.Models;
using Search.Services;
using Search.Models;

namespace KylieBot.Dialogs
{
    [Serializable]
    public class ProcessActionDialog : IDialog<object>
    {
        private ISearchClient searchClient;
        public ProcessActionDialog(ISearchClient searchClient)
        {
            this.searchClient = searchClient;
        }
        public Task StartAsync(IDialogContext context)
        {
            var userData = context.UserData;
            User retrieveUser = userData.GetValue<User>("User");

            switch (retrieveUser.searchTerm)
            {
                case "Empowered Search":
                    context.Call(new IndexSearchDialog(this.searchClient), SearchCompleted);
                    break;
                case "Back To Kylie Bot":
                    context.Done<string>(null);
                    break;
                case "Additional Info":
                    break;
                case "I'm done for now":
                    context.PostAsync("Hope to see you again soon. Just send me a message if you need to wake me up.");
                    context.EndConversation("");
                    context.Done<string>(null);
                    break;
                default:
                    context.Done<string>(null);
                    break;
            }
            return Task.CompletedTask;
        }

        private Task SearchCompleted(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<string>(null);
            return Task.CompletedTask;
        }

        public Task SearchCompleted(IDialogContext context, IAwaitable<IList<SearchHit>> result)
        {
            context.Done<string>(null);
            return Task.CompletedTask;
        }
    }
}