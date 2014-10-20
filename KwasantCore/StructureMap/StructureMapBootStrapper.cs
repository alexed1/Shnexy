using System.Net;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Interfaces;
using KwasantCore.ExternalServices;
using KwasantCore.ExternalServices.REST;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantCore.Managers.APIManagers.Authorizers.Google;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.Managers.APIManagers.Packagers.CalDAV;
using KwasantCore.Managers.APIManagers.Packagers.Mandrill;
using KwasantCore.Managers.APIManagers.Packagers.SendGrid;
using KwasantCore.Managers.APIManagers.Packagers.Twilio;
using KwasantCore.Security;
using KwasantCore.Services;
using Moq;
using SendGrid;
using StructureMap;
using StructureMap.Configuration.DSL;
using AutoMapper;
using Utilities;

namespace KwasantCore.StructureMap
{
    public class StructureMapBootStrapper
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        #region Method

        public static void ConfigureDependencies(DependencyType type)
        {

            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Initialize(x => x.AddRegistry<TestMode>());
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    break;
            }
        }

        public class KwasantCoreRegistry : Registry
        {
            public KwasantCoreRegistry()
            {
                For<IEvent>().Use<Event>();
            }
        }

        public class LiveMode : DatabaseStructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IConfigRepository>().Use<ConfigRepository>();
                For<ISMSPackager>().Use<TwilioPackager>();
                For<IMappingEngine>().Use(Mapper.Engine);
                For<IEmailPackager>().Use<SendGridPackager>().Singleton().Named(EnvelopeDO.SendGridHander);
                For<IBookingRequest>().Use<BookingRequest>();
                For<IAttendee>().Use<Attendee>();
                For<IEmailAddress>().Use<EmailAddress>();
                For<ICalDAVClientFactory>().Use<CalDAVClientFactory>();
                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();

                For<IOAuthAuthorizer>().Use<GoogleCalendarAuthorizer>().Named("Google");

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchy>();
                For<IImapClient>().Use<ImapClientWrapper>();
                For<ITransport>().Use<Web>(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>()));
                For<IRestfullCall>().Use<RestfulCallWrapper>();
                For<ITwilioRestClient>().Use<TwilioRestClientWrapper>();
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<ISMSPackager>().Use<TwilioPackager>();
                For<IMappingEngine>().Use(Mapper.Engine);
                For<IEmailPackager>().Use<SendGridPackager>().Singleton().Named(EnvelopeDO.SendGridHander);
                For<IBookingRequest>().Use<BookingRequest>();
                For<IAttendee>().Use<Attendee>();
                For<IEmailAddress>().Use<EmailAddress>();
                For<ITracker>().Use<SegmentIO>();

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchyWithoutCTE>();
                var mockSegment = new Mock<ITracker>();
                For<ITracker>().Use(mockSegment.Object);
            }
        }

        #endregion       
    }
}