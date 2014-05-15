﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Twilio;
using StructureMap;
using Twilio;
using Microsoft.WindowsAzure;

namespace KwasantCore.Managers.CommunicationManager
{
    public class CommunicationManager
    {
        //Register for interesting events

        public void SubscribeToAlerts()
        {
            AlertManager.alertCustomerCreated += NewCustomerWorkflow;
        }


        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(KwasantSchedulingAlertData eventData)
        {
            Debug.WriteLine("NewCustomer has been created.");
        }

        public void ProcessBRNotifications(IList<BookingRequestDO> bookingRequests)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CommunicationConfigurationRepository communicationConfigurationRepo = new CommunicationConfigurationRepository(uow);
            foreach (CommunicationConfigurationDO communicationConfig in communicationConfigurationRepo.GetAll().ToList())
            {
                if (communicationConfig.Type == CommunicationType.SMS)
                {
                    SendBRSMSes(communicationConfig.ToAddress, bookingRequests);
                } else if (communicationConfig.Type == CommunicationType.EMAIL)
                {
                    SendBREmails(communicationConfig.ToAddress, bookingRequests, uow);
                }
                else
                {
                    throw new Exception(String.Format("Invalid communication type '{0}'", communicationConfig.Type));
                }
            }
            uow.SaveChanges();
        }

        private void SendBRSMSes(String toAddress, IEnumerable<BookingRequestDO> bookingRequests)
        {
            TwilioPackager twil = new TwilioPackager();
            const string message = "A new booking request has been created. From '{0}'.";
            foreach (BookingRequestDO bookingRequest in bookingRequests)
            {
                SMSMessage result = twil.SendSMS(toAddress, String.Format(message, bookingRequest.From.Address));
               
            }
        }

        private void SendBREmails(String toAddress, IEnumerable<BookingRequestDO> bookingRequests, IUnitOfWork uow)
        {
            EmailRepository emailRepo = new EmailRepository(uow);
            const string message = "A new booking request has been created. From '{0}'.";
            foreach (BookingRequestDO bookingRequest in bookingRequests)
            {
                EmailDO outboundEmail = new EmailDO
                {
                    From = new EmailAddressDO {Address = "service@kwasant.com"},
                    To = new List<EmailAddressDO> {new EmailAddressDO {Address = toAddress}},
                    Subject = "New booking request!",
                    Text = String.Format(message, bookingRequest.From.Address),
                    Status = EmailStatus.QUEUED
                };

                emailRepo.Add(outboundEmail);
            }
        }

        //This is the default originator of outbound Kwasant emails
        public static string GetFromEmail()
        {
            string email = CloudConfigurationManager.GetSetting("fromEmail");
            if (email != null)
            {
                return email;
            }
            else
            {
                throw new Exception("Missing value for 'fromEmail'");
            }

        }
        public static string GetFromName()
        {
            string fromName = CloudConfigurationManager.GetSetting("fromName");
            if (fromName != null)
            {
                return fromName;
            }
            else
            {
                throw new Exception("Missing value for 'fromName'");
            }

        }
    }
}