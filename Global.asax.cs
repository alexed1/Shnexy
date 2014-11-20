﻿using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantCore.ModelBinders;
using KwasantCore.Security;
using KwasantCore.Services;
using KwasantCore.Managers;
using KwasantCore.StructureMap;
using KwasantWeb.AlertQueues;
using KwasantWeb.App_Start;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Segment;
using StructureMap;
using Utilities;
using Logger = Utilities.Logging.Logger;

namespace KwasantWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static bool _IsInitialised;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
                //set to either "test" or "live"

            var db = ObjectFactory.GetInstance<DbContext>();
            db.Database.Initialize(true);

            Utilities.Server.ServerPhysicalPath = Server.MapPath("~");

            //AutoMapper create map configuration
            AutoMapperBootStrapper.ConfigureAutoMapper();

            Logger.GetLogger().Info("Kwasant web starting...");

            Utilities.Server.IsProduction = ObjectFactory.GetInstance<IConfigRepository>().Get<bool>("IsProduction");
            Utilities.Server.IsDevMode = ObjectFactory.GetInstance<IConfigRepository>().Get<bool>("IsDev", true);

            CommunicationManager curCommManager = ObjectFactory.GetInstance<CommunicationManager>();
            curCommManager.SubscribeToAlerts();

            var segmentWriteKey = new ConfigRepository().Get("SegmentWriteKey");
            Analytics.Initialize(segmentWriteKey);

            AlertReporter curReporter = new AlertReporter();
            curReporter.SubscribeToAlerts();

            IncidentReporter incidentReporter = new IncidentReporter();
            incidentReporter.SubscribeToAlerts();

            ModelBinders.Binders.Add(typeof (DateTimeOffset), new KwasantDateBinder());

            SharedAlertQueues.Begin();

            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RemoteCalendarProviderRepository.CreateRemoteCalendarProviders(configRepository);
                uow.SaveChanges();
            }

            SetServerUrl();
        }


        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            String errorMessage = "Critical internal error occured.";
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    errorMessage += " URL accessed: " + HttpContext.Current.Request.Url;
            }
            catch (Exception)
            {
                errorMessage += " Error on startup.";
            }


            Logger.GetLogger().Error(errorMessage, exception);
        }

        private readonly object _initLocker = new object();

        //Optimization. Even if running in DEBUG mode, this will only execute once.
        //But on production, there is no need for this call
#if DEBUG
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            SetServerUrl(HttpContext.Current);
        }
#endif

        private void SetServerUrl(HttpContext context = null)
        {
            if (!_IsInitialised)
            {
                lock (_initLocker)
                {
                    //Not redunant - this check is more efficient for a 1-time set.
                    //If it's set, we exit without locking. We want to avoid locking as much as possible, so only do it once (at startup)
                    if (!_IsInitialised)
                    {
                        //First, try to read from the config
                        var config = ObjectFactory.GetInstance<IConfigRepository>();
                        var serverProtocol = config.Get("ServerProtocol", String.Empty);
                        var domainName = config.Get("ServerDomainName", String.Empty);
                        var domainPort = config.Get<int?>("ServerPort", null);

                        if (!String.IsNullOrWhiteSpace(domainName) && !String.IsNullOrWhiteSpace(serverProtocol) && domainPort.HasValue)
                        {
                            Utilities.Server.ServerUrl = String.Format("{0}{1}{2}/", serverProtocol, domainName,
                                domainPort.Value == 80 ? String.Empty : (":" + domainPort.Value));

                            Utilities.Server.ServerHostName = domainName;
                        }
                        else
                        {
                            if (context == null)
                                return;

                            //If the config is not set, then we setup our server URL based on the first request
                            string port = context.Request.ServerVariables["SERVER_PORT"];
                            if (port == null || port == "80" || port == "443")
                                port = "";
                            else
                                port = ":" + port;

                            string protocol = context.Request.ServerVariables["SERVER_PORT_SECURE"];
                            if (protocol == null || protocol == "0")
                                protocol = "http://";
                            else
                                protocol = "https://";

                            // *** Figure out the base Url which points at the application's root
                            Utilities.Server.ServerHostName = context.Request.ServerVariables["SERVER_NAME"];
                            string url = protocol + context.Request.ServerVariables["SERVER_NAME"] + port + context.Request.ApplicationPath;
                            Utilities.Server.ServerUrl = url;
                        }
                        _IsInitialised = true;
                    }
                }
            }
        }

        public void Application_End()
        {
            Logger.GetLogger().Info("Kwasant web shutting down...");

            // This will give LE background thread some time to finish sending messages to Logentries.
            var numWaits = 3;
            while (!LogentriesCore.Net.AsyncLogger.AreAllQueuesEmpty(TimeSpan.FromSeconds(5)) && numWaits > 0)
                numWaits--;
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var principal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            if (principal != null)
            {
                var claims = principal.Claims;
                GenericPrincipal userPrincipal =
                    new GenericPrincipal(new GenericIdentity(principal.Identity.Name),
                                         claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray());
                Context.User = userPrincipal;
            }
        }
    }
}

