﻿using System.Linq;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Models
{
    [TestFixture]
    public class CustomerTests : BaseTest
    {
        [Test]
        [Category("Customer")]
        public void Customer_Add_CanCreateUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP
                //create a customer from fixture data
                UserDO curUserDO = fixture.TestUser1();

                //EXECUTE
                uow.UserRepository.Add(curUserDO);
                uow.SaveChanges();

                //VERIFY
                //check that it was saved to the db
                UserDO savedUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == curUserDO.Id);
                Assert.AreEqual(curUserDO.FirstName, savedUserDO.FirstName);
                Assert.AreEqual(curUserDO.EmailAddress, savedUserDO.EmailAddress);

            }

        }
    }
}
