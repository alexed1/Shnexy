﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.MockedDB
{
    [TestFixture]
    public class MockedDBTests : BaseTest
    {
        //This test is to ensure our mocking properly distinguishes between saved and local DbSets (to mimic EF behaviour)
        [Test]
        public void TestDBMocking()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var brOne = new BookingRequestDO();
                brOne.Id = 1;
                brOne.UserID = "Rob";
                uow.BookingRequestRepository.Add(brOne);

                using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    Assert.AreEqual(0, subUow.BookingRequestRepository.GetQuery().Count());
                    Assert.AreEqual(0, subUow.BookingRequestRepository.DBSet.Local.Count());
                }

                Assert.AreEqual(0, uow.BookingRequestRepository.GetQuery().Count());
                Assert.AreEqual(1, uow.BookingRequestRepository.DBSet.Local.Count());

                uow.SaveChanges();

                using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    Assert.AreEqual(1, subUow.BookingRequestRepository.GetQuery().Count());
                    Assert.AreEqual(0, subUow.BookingRequestRepository.DBSet.Local.Count());
                }
            }
        }

        [Test]
        public void TestDBMockingForeignKeyUpdate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = new UserDO();
                var brOne = new BookingRequestDO();
                brOne.Id = 1;
                brOne.UserID = user.Id;
                uow.BookingRequestRepository.Add(brOne);
                uow.UserRepository.Add(user);

                uow.SaveChanges();

                Assert.NotNull(brOne.User);
            }
        }

        [Test]
        public void TestCollectionsProperlyUpdated()
        {
            //Force a seed -- helps with debug
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negDO = new NegotiationDO();
                negDO.Id = 1;
                uow.NegotiationsRepository.Add(negDO);

                var attendee = new AttendeeDO();
                attendee.NegotiationID = 1;
                uow.AttendeeRepository.Add(attendee);
                
                uow.SaveChanges();

                Assert.AreEqual(1, negDO.Attendees.Count);
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negDO = uow.NegotiationsRepository.GetQuery().First();

                Assert.AreEqual(1, negDO.Attendees.Count);
            }
        }


        [Test]
        public void AssertAllTestsImplementBaseTest()
        {
            var failedTypes = new List<Type>();
            foreach (var testClass in GetType().Assembly.GetTypes().Where(t => t.GetCustomAttributes<TestFixtureAttribute>().Any()))
            {
                if (testClass != typeof(BaseTest) && !testClass.IsSubclassOf(typeof(BaseTest)))
                    failedTypes.Add(testClass);
            }
            var exceptionMessages = new List<String>();
            foreach (var failedType in failedTypes)
            {
                var testClassName = failedType.Name;
                exceptionMessages.Add(testClassName + " must implement 'BaseTest'");
            }
            if (exceptionMessages.Any())
                Assert.Fail(String.Join(Environment.NewLine, exceptionMessages));
        }

        [Test, ExpectedException(ExpectedMessage = "Foreign row does not exist.\nValue '0' on 'NegotiationDO.NegotiationState' pointing to '_NegotiationStateTemplate.Id'")]
        public void TestForeignKeyEnforced()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negDO = new NegotiationDO {Id = 1};
                uow.NegotiationsRepository.Add(negDO);

                uow.SaveChanges();
            }
        }
    }
}
