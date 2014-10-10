using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Interfaces;
using KwasantCore.ExternalServices;
using KwasantCore.ExternalServices.REST;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantCore.Managers.APIManagers.Authorizers.Google;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.Managers.APIManagers.Packagers.CalDAV;
using KwasantCore.Managers.APIManagers.Packagers.Mandrill;
using KwasantCore.Managers.APIManagers.Packagers.Twilio;
using KwasantCore.Security;
using KwasantCore.Services;
using Moq;
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
                For<IEmailPackager>().Use<GmailPackager>().Singleton().Named(EnvelopeDO.GmailHander);
                For<IEmailPackager>().Use<MandrillPackager>().Singleton().Named(EnvelopeDO.MandrillHander);
                For<IBookingRequest>().Use<BookingRequest>();
                For<IAttendee>().Use<Attendee>();
                For<IEmailAddress>().Use<EmailAddress>();
                For<ICalDAVClientFactory>().Use<CalDAVClientFactory>();
                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();

                For<IOAuthAuthorizer>().Use<GoogleCalendarAuthorizer>().Named("Google");

                For<IKwasantRoleStore>().Use(new KwasantRoleStore());
                For<IKwasantUserStore>().Use(new KwasantUserStore());

                For<IImapClient>().Use<ImapClientWrapper>();
                For<ISmtpClient>().Use<SmtpClientWrapper>();
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
                For<IEmailPackager>().Use<GmailPackager>().Singleton().Named(EnvelopeDO.GmailHander);
                For<IEmailPackager>().Use<MandrillPackager>().Singleton().Named(EnvelopeDO.MandrillHander);
                For<IBookingRequest>().Use<BookingRequest>();
                For<IAttendee>().Use<Attendee>();
                For<IEmailAddress>().Use<EmailAddress>();
                For<ITracker>().Use<SegmentIO>();

                For<IKwasantRoleStore>().Use(new MockedRoleStore());
                For<IKwasantUserStore>().Use(new MockedUserStore());

                var mockSegment = new Mock<ITracker>();
                For<ITracker>().Use(mockSegment.Object);
            }
        }

        #endregion       
    }
}