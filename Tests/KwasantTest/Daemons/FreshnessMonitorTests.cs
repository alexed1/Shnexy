﻿using System;
using System.Linq;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class FreshnessMonitorTests : BaseTest
    {
        private Func<DateTimeOffset> GetThroughputCheckingStartTime = () => DateTimeOffset.Now.AddHours(-1);
        private Func<DateTimeOffset> GetThroughputCheckingEndTime = () => DateTimeOffset.Now.AddHours(1);

        private Mock<ISMSPackager> _smsPackagerMock;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);

            AlertReporter alertReporter = new AlertReporter();
            alertReporter.SubscribeToAlerts();

            _smsPackagerMock = new Mock<ISMSPackager>();
            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(_smsPackagerMock.Object));

            var configRepositoryMock = new Mock<IConfigRepository>();
            configRepositoryMock
                .Setup(c => c.Get<string>(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    switch (key)
                    {
                        case "MaxBRIdle":
                            return "0.04";
                        case "MaxBRReservationPeriod":
                            return "0.04";
                        case "ExpectedResponseActiveDuration":
                            return "0.04";
                        case "EmailAddress_GeneralInfo":
                            return "info@kwasant.com";
                        case "ThroughputCheckingStartTime":
                            return GetThroughputCheckingStartTime().ToString();
                        case "ThroughputCheckingEndTime":
                            return GetThroughputCheckingEndTime().ToString();
                        default:
                            return new MockedConfigRepository().Get<string>(key);
                    }
                });
            ObjectFactory.Configure(cfg => cfg.For<IConfigRepository>().Use(configRepositoryMock.Object));
        }

        [Test]
        public void TestFreshnessMonitorExpired()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                var bookingRequestDO = new FixtureData(uow).TestBookingRequest1();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.Customer = new FixtureData(uow).TestUser1();
                bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 1, 0, 0));
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                var freshnessMonitor = new FreshnessMonitor();
                DaemonTests.RunDaemonOnce(freshnessMonitor);

                _smsPackagerMock.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), () => Times.Exactly(1));
            }
        }

        [Test]
        public void TestFreshnessMonitorNotExpired()
        {
            GetThroughputCheckingStartTime = () => DateTimeOffset.Now.AddHours(1);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                var bookingRequestDO = new FixtureData(uow).TestBookingRequest1();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.Customer = new FixtureData(uow).TestUser1();
                bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 0, 20, 0));
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                var freshnessMonitor = new FreshnessMonitor();
                DaemonTests.RunDaemonOnce(freshnessMonitor);

                _smsPackagerMock.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
            }
        }

        [Test]
        public void TestFreshnessMonitorNotRunningOutsideSpecifiedTime()
        {
            var smsPackager = new Mock<ISMSPackager>();

            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(smsPackager.Object));

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                var bookingRequestDO = new FixtureData(uow).TestBookingRequest1();
                bookingRequestDO.State = BookingRequestState.Unstarted;
                bookingRequestDO.Customer = new FixtureData(uow).TestUser1();
                bookingRequestDO.DateCreated = DateTime.Now.Subtract(new TimeSpan(0, 0, 20, 0));
                bookingRequestRepo.Add(bookingRequestDO);

                uow.SaveChanges();

                var freshnessMonitor = new FreshnessMonitor();
                DaemonTests.RunDaemonOnce(freshnessMonitor);

                smsPackager.Verify(s => s.SendSMS(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
            }
        }
    }
}
