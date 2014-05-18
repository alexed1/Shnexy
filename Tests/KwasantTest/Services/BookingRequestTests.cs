﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Managers
{
    [TestFixture]
    public class BookingRequestTests 
    {
        public ICustomerRepository _customerRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _customerRepo = new CustomerRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        [Category("BookingRequest")]
        public void NewCustomerCreated()
        {
            List<CustomerDO> customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(0, customersNow.Count);

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Kwasant"))
            {
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);
            Assert.AreEqual("customer@gmail.com", customersNow.First().EmailAddress);
            Assert.AreEqual("Mister Customer", customersNow.First().FirstName);
        }

        [Test]
        [Category("BookingRequest")]
        public void ExistingCustomerNotCreatedButUsed()
        {
            List<CustomerDO> customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(0, customersNow.Count);

            CustomerDO customer = _fixture.TestCustomer();
            _customerRepo.Add(customer);
            _uow.SaveChanges();

            customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);

            MailMessage message = new MailMessage(new MailAddress(customer.EmailAddress, customer.FirstName), new MailAddress("kwa@sant.com", "Kwasant"))
            {
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            customersNow = _customerRepo.GetAll().ToList();
            Assert.AreEqual(1, customersNow.Count);
            Assert.AreEqual(customer.EmailAddress, customersNow.First().EmailAddress);
            Assert.AreEqual(customer.FirstName, customersNow.First().FirstName);
        }

        [Test]
        [Category("BookingRequest")]
        public void ParseAllDay()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Kwasant"))
            {
                Body = "CCADE",
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.EventDuration.MarkAsAllDayEvent, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BookingRequest")]
        public void Parse30MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Kwasant"))
            {
                Body = "cc30",
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add30MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BookingRequest")]
        public void Parse60MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Kwasant"))
            {
                Body = "cc60",
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add60MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BookingRequest")]
        public void Parse90MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Kwasant"))
            {
                Body = "cc90",
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add90MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }

        [Test]
        [Category("BookingRequest")]
        public void Parse120MinsInAdvance()
        {

            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Kwasant"))
            {
                Body = "cc120",
            };

            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(_uow);
            BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
            BookingRequest.ProcessBookingRequest(_uow, bookingRequest);

            bookingRequest = bookingRequestRepo.GetAll().ToList().First();
            Assert.AreEqual(1, bookingRequest.Instructions.Count);
            Assert.AreEqual(InstructionConstants.TravelTime.Add120MinutesTravelTime, bookingRequest.Instructions.First().InstructionID);
        }
    }
}