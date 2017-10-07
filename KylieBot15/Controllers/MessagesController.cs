using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Internals;
using KylieBot.Models;
using Autofac;
using KylieBot.Helpers;

namespace KylieBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            bool userCreated;

            if (userData.Data == null)
            {
                userCreated = false;
            }
            else
            {
                userCreated = userData.GetProperty<bool>("UserCreated");
            }
            User user = null;

            if (!userCreated)
            {
                user = BotHelper.createUser(activity);
                userData.SetProperty<User>("User", user);
                userData.SetProperty<bool>("UserCreated", true);
                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            }
            else
            {
                user = userData.GetProperty<User>("User");
            }

            if (activity.Type == ActivityTypes.Message)
            {
                int messageCount = user.MessageCount;
                user.MessageCount = messageCount + 1;
                userData.SetProperty<User>("User", user);

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var working = activity.CreateReply();
                    working.Type = ActivityTypes.Typing;
                    await connector.Conversations.ReplyToActivityAsync(working).ConfigureAwait((new BotLogger().Log(activity, userData)).IsCompleted);

                    await Conversation.SendAsync(activity, () => scope.Resolve<IDialog<object>>());
                }
            }
            else
            {
                await HandleSystemMessageAsync(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData) { }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                List<User> memberList = new List<User>();

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    var activityMembers = await client.Conversations.GetConversationMembersAsync(message.Conversation.Id);

                    foreach (var member in activityMembers)
                    {
                        memberList.Add(new User() { Id = member.Id, Name = member.Name });
                    }

                    if (message.MembersAdded != null && message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                    {
                        var intro = message.CreateReply("");

                        intro.Attachments = new List<Attachment>
                            {
                                new HeroCard
                                {
                                    Title = "Hello. I'm **Kylie Bot (KB)**",
                                    Subtitle = "What can I assist you with?",
                                    Text = "*Select one of the following options for me to assist you with:*",
                                    Images = new List<CardImage> { new CardImage("https://static.wixstatic.com/media/85fb2e_42b7a62b39fc4ba39b606e240c3cfa54~mv2.png/v1/fill/w_256,h_256,al_c/85fb2e_42b7a62b39fc4ba39b606e240c3cfa54~mv2.png") },
                                    Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Empowered Search", null, "Empowered Search"),
                                                                        new CardAction(ActionTypes.OpenUrl, "Additional Info", value: "https://www.daringdynamics.co.uk")
                                                                     }
                                }.ToAttachment()
                            };
                        await connector.Conversations.ReplyToActivityAsync(intro);
                    }
                }

                if (message.MembersAdded != null && message.MembersAdded.Any() && memberList.Count > 2)
                {
                    var added = message.CreateReply(message.MembersAdded[0].Name + " joined the conversation");
                    await connector.Conversations.ReplyToActivityAsync(added);
                }

                if (message.MembersRemoved != null && message.MembersRemoved.Any())
                {
                    var removed = message.CreateReply(message.MembersRemoved[0].Name + " left the conversation");
                    await connector.Conversations.ReplyToActivityAsync(removed);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate) { }
            else if (message.Type == ActivityTypes.Typing) { }
            else if (message.Type == ActivityTypes.Ping)
            {
                Activity reply = message.CreateReply();
                reply.Type = ActivityTypes.Ping;
                return reply;
            }
            return null;
        }
    }
}