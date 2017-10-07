using Autofac;
using KylieBot.Dialogs;
using KylieBot.Helpers;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Search.Azure.Services;
using Search.Models;
using Search.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace KylieBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Models.AuthSettings.Mode = ConfigurationManager.AppSettings["ActiveDirectory.Mode"];
            Models.AuthSettings.EndpointUrl = ConfigurationManager.AppSettings["ActiveDirectory.EndpointUrl"];
            Models.AuthSettings.Tenant = ConfigurationManager.AppSettings["ActiveDirectory.Tenant"];
            Models.AuthSettings.RedirectUrl = ConfigurationManager.AppSettings["ActiveDirectory.RedirectUrl"];
            Models.AuthSettings.ClientId = ConfigurationManager.AppSettings["ActiveDirectory.ClientId"];
            Models.AuthSettings.ClientSecret = ConfigurationManager.AppSettings["ActiveDirectory.ClientSecret"];
            Models.AuthSettings.Scopes = ConfigurationManager.AppSettings["ActiveDirectory.Scopes"].Split(',');

            Conversation.UpdateContainer(builder =>
            {
                builder.RegisterType<RootDialog>()
                  .As<IDialog<object>>()
                  .InstancePerDependency();

                builder.RegisterType<IndexMapper>()
                   .Keyed<IMapper<DocumentSearchResult, GenericSearchResult>>(FiberModule.Key_DoNotSerialize)
                   .AsImplementedInterfaces()
                   .SingleInstance();

                builder.RegisterType<AzureSearchClient>()
                    .Keyed<ISearchClient>(FiberModule.Key_DoNotSerialize)
                    .AsImplementedInterfaces()
                    .SingleInstance();
            });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
