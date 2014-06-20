using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using Data.Validators;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;

namespace KwasantCore.Services
{
    public class Email
    {
        private readonly IUnitOfWork _uow;
        private EmailDO _curEmailDO;
        private EventValidator _curEventValidator;
        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        /// 

        //this constructor enables the creation of an email that doesn't necessarily have anything to do with an Event. It gets called by the other constructors
        public Email(IUnitOfWork uow)
        {
            _uow = uow;
            _curEventValidator = new EventValidator();
        }

        public Email(IUnitOfWork uow, EmailDO curEmailDO) : this(uow)
        {
            //should add validation here
            _curEmailDO = curEmailDO;
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        public EnvelopeDO SendTemplate(string templateName, EmailDO message, Dictionary<string, string> mergeFields)
        {
            var envelope = Envelope.CreateMandrillEnvelope(message, templateName, mergeFields);
            message.EmailStatus = EmailStatus.QUEUED;
            _uow.EnvelopeRepository.Add(envelope);
            _uow.SaveChanges();
            return envelope;
        }

        public EnvelopeDO Send()
        {
            var envelope = Envelope.CreateGmailEnvelope(_curEmailDO);
            _curEmailDO.EmailStatus = EmailStatus.QUEUED;
            _uow.EnvelopeRepository.Add(envelope);
            _uow.SaveChanges();
            return envelope;
        }

        public static void InitialiseWebhook(String url)
        {
            MandrillPackager.InitialiseWebhook(url);
        }

        public static void HandleWebhookResponse(String responseStr)
        {
            MandrillPackager.HandleWebhookResponse(responseStr);
        }

        public static void Ping()
        {
            string results = MandrillPackager.PostPing();
            Debug.WriteLine(results);
        }

        #endregion

      
        public static EmailDO ConvertMailMessageToEmail(IEmailRepository emailRepository, MailMessage mailMessage)
        {
            return ConvertMailMessageToEmail<EmailDO>(emailRepository, mailMessage);            
        }

        public static TEmailType ConvertMailMessageToEmail<TEmailType>(IGenericRepository<TEmailType> emailRepository, MailMessage mailMessage)
            where TEmailType : EmailDO, new()
        {
            String body = String.Empty;
            if (!mailMessage.IsBodyHtml)
            {
                foreach (var av in mailMessage.AlternateViews)
                {
                    if (av.ContentType.MediaType == "text/html")
                    {
                        body = new StreamReader(av.ContentStream).ReadToEnd();
                        break;
                    }
                }
            }
            if (String.IsNullOrEmpty(body))
                body = mailMessage.Body;

            String strDate = String.Empty;
            strDate = mailMessage.Headers["Date"];

            TEmailType emailDO = new TEmailType
            {                
                Subject = mailMessage.Subject,
                HTMLText = body,
                DateReceived = Convert.ToDateTime(strDate),
                Attachments = mailMessage.Attachments.Select(CreateNewAttachment).Union(mailMessage.AlternateViews.Select(CreateNewAttachment)).Where(a => a != null).ToList(),
                Events = null
            };
            var uow = emailRepository.UnitOfWork;

            emailDO.From = GenerateEmailAddress(uow, mailMessage.From);
            foreach (var addr in mailMessage.To.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.TO, addr);    
            }
            foreach (var addr in mailMessage.Bcc.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.BCC, addr);
            }
            foreach (var addr in mailMessage.CC.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.CC, addr);
            }

            emailDO.Attachments.ForEach(a => a.Email = emailDO);
            //emailDO.EmailStatus = EmailStatus.QUEUED; we no longer want to set this here. not all Emails are outbound emails. This should only be set in functions like Event#Dispatch
            emailDO.EmailStatus = EmailStatus.UNSTARTED; //we'll use this new state so that every email has a valid status.
            emailRepository.Add(emailDO);
            return emailDO;
        }

        public static EmailAddressDO GenerateEmailAddress(IUnitOfWork uow, MailAddress address)
        {
            return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
        }

        public static AttachmentDO CreateNewAttachment(Attachment attachment)
        {
            AttachmentDO att = new AttachmentDO
            {
                OriginalName = attachment.Name,
                Type = attachment.ContentType.MediaType,
            };
            
            att.SetData(attachment.ContentStream);
            return att;
        }

        public static AttachmentDO CreateNewAttachment(AlternateView av)
        {
            if (av.ContentType.MediaType == "text/html")
                return null;

            AttachmentDO att = new AttachmentDO
            {
                OriginalName = String.IsNullOrEmpty(av.ContentType.Name)? av.ContentType.MediaType : "File",
                Type = av.ContentType.MediaType,
            };

            att.SetData(av.ContentStream);
            return att;
        }
    }
}
