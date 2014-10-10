﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OutboundEmailTests : BaseTest
    {
        [Test]
        [Category("OutboundEmail")]
        public void CanSendGmailEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var outboundEmailDaemon = new OutboundEmail();

                // SETUP
                var email = fixture.TestEmail1();

                uow.EmailRepository.Add(email);

                // EXECUTE
                var envelope = uow.EnvelopeRepository.ConfigurePlainEmail(email);

                uow.SaveChanges();

                //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
                AddNewTestCustomer(email.From);

                var mockEmailer = new Mock<IEmailPackager>();
                mockEmailer.Setup(a => a.Send(envelope)).Verifiable();
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockEmailer.Object).Named(EnvelopeDO.GmailHander));
                DaemonTests.RunDaemonOnce(outboundEmailDaemon);

                // VERIFY
                mockEmailer.Verify(a => a.Send(envelope), "OutboundEmail daemon didn't dispatch email via Gmail.");
            }
        }

        [Test]
        [Category("OutboundEmail")]
        public void CanSendMandrillEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var outboundEmailDaemon = new OutboundEmail();

                // SETUP
                var email = fixture.TestEmail1();

                // EXECUTE
                var envelope = uow.EnvelopeRepository.ConfigureTemplatedEmail(email, "template", null);
                uow.SaveChanges();

                //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
                AddNewTestCustomer(email.From);
                
                var mockEmailer = new Mock<IEmailPackager>();
                mockEmailer.Setup(a => a.Send(envelope)).Verifiable();
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockEmailer.Object).Named(EnvelopeDO.MandrillHander));
                DaemonTests.RunDaemonOnce(outboundEmailDaemon);

                // VERIFY
                mockEmailer.Verify(a => a.Send(envelope), "OutboundEmail daemon didn't dispatch email via Mandrill.");
            }
        }

        [Test]
        [Category("OutboundEmail")]
        public void FailsToSendInvalidEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var outboundEmailDaemon = new OutboundEmail();

                // SETUP
                var email = fixture.TestEmail1();

                // EXECUTE
                var envelope = uow.EnvelopeRepository.ConfigureTemplatedEmail(email, "template", null);

                envelope.Handler = "INVALID EMAIL PACKAGER";
                uow.SaveChanges();

                //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
                AddNewTestCustomer(email.From);

                var mockMandrillEmailer = new Mock<IEmailPackager>();
                mockMandrillEmailer.Setup(a => a.Send(envelope)).Throws<ApplicationException>(); // shouldn't be invoked
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockMandrillEmailer.Object).Named(EnvelopeDO.MandrillHander));
                var mockGmailEmailer = new Mock<IEmailPackager>();
                mockGmailEmailer.Setup(a => a.Send(envelope)).Throws<ApplicationException>(); // shouldn't be invoked
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockGmailEmailer.Object).Named(EnvelopeDO.GmailHander));

                // VERIFY
                Assert.Throws<UnknownEmailPackagerException>(
                    () => DaemonTests.RunDaemonOnce(outboundEmailDaemon),
                    "OutboundEmail daemon didn't throw an exception for invalid EnvelopeDO.");
            }
        }

        private void AddNewTestCustomer(EmailAddressDO emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var outboundEmailDaemon = new OutboundEmail();

                emailAddress.Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(uow, new MailAddress("joetest2@edelstein.org")),
                        EmailParticipantType = EmailParticipantType.To
                    }
                };
                uow.AspNetRolesRepository.Add(fixture.TestRole());
                var u = new UserDO();
                var user = new User();
                UserDO currUserDO = new UserDO();
                currUserDO.EmailAddress = emailAddress;
                uow.UserRepository.Add(currUserDO);
            }
        }
    }
}
