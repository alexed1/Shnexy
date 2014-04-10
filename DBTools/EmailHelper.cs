﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DDay.DDay.iCal;
using Data.DDay.DDay.iCal.DataTypes;
using Data.DDay.DDay.iCal.ExtensionMethods;
using Data.DDay.DDay.iCal.Interfaces;
using Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers;
using Data.Models;
using DBTools.Managers;
using StructureMap;
using Attachment = Data.Models.Attachment;
using Attendee = Data.DDay.DDay.iCal.DataTypes.Attendee;


namespace DBTools
{
    public static class EmailHelper
    {
        public static void DispatchInvitation(IUnitOfWork uow, Invitation invitation)
        {
            var emailRepo = new EmailRepository(uow);

            var fromEmail = "lucreorganizer@gmail.com";
            var fromName = "Booqit Organizer";

            var mailMessage = new MailMessage { From = new MailAddress(fromEmail, fromName) };
            foreach (var attendee in invitation.Attendees)
                mailMessage.To.Add(new MailAddress(attendee.EmailAddress, attendee.Name));
            mailMessage.Subject = "Invitation via Booqit: " + invitation.Summary + "@ " + invitation.StartDate;
            mailMessage.Body = "This is a Booqit Event Request. For more information, see https://foo.com";

            var calendar = new iCalendar();
            var evnt = new Event();
            if (invitation.IsAllDay)
            {
                evnt.IsAllDay = true;
            }
            else
            {
                evnt.DTStart = new iCalDateTime(invitation.StartDate);
                evnt.DTEnd = new iCalDateTime(invitation.EndDate);
            }
            evnt.DTStamp = new iCalDateTime(DateTime.Now);
            evnt.LastModified = new iCalDateTime(DateTime.Now);

            evnt.Location = invitation.Where;
            evnt.Description = invitation.Description;
            evnt.Summary = invitation.Summary;
            foreach (var attendee in invitation.Attendees)
            {
                evnt.Attendees.Add(new Attendee
                {
                    CommonName = attendee.Name,
                    Type = "INDIVIDUAL",
                    Role = "REQ-PARTICIPANT",
                    ParticipationStatus = ParticipationStatus.NeedsAction,
                    RSVP = true,
                    Value = new Uri("mailto:" + attendee.EmailAddress),
                });
                attendee.Invitation = invitation;
            }
            evnt.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            calendar.Events.Add(evnt);
            
            var email = AddNewEmailToRepository(emailRepo, mailMessage);
            AttachCalendarToEmail(calendar, email);
            invitation.Emails.Add(email);

            uow.SaveChanges();
            SendEmail(email);
        }

        public static void AttachCalendarToEmail(iCalendar iCal, Email email)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            var fileToAttach = serializer.Serialize(iCal);

            var attachment = CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType {MediaType = "application/calendar", Name = "invite.ics"}
                ));

            email.Attachments.Add(attachment);
        }

        public static Email AddNewEmailToRepository(IEmailRepository emailRepository, MailMessage mailAddress)
        {
            var email = new Email
            {
                From = GetEmailAddress(mailAddress.From),
                BCC = mailAddress.Bcc.Select(GetEmailAddress).ToList(),
                CC = mailAddress.CC.Select(GetEmailAddress).ToList(),
                Subject = mailAddress.Subject,
                Text = mailAddress.Body,
                Attachments = mailAddress.Attachments.Select(CreateNewAttachment).ToList(),
                To = mailAddress.To.Select(GetEmailAddress).ToList(),
                Invitation = null
            };
            email.To.ForEach(a => a.ToEmail = email);
            email.CC.ForEach(a => a.BCCEmail = email);
            email.BCC.ForEach(a => a.CCEmail = email);
            email.Status = EmailStatusConstants.GetStatusRow(EmailStatusConstants.QUEUED);

            emailRepository.Add(email);
            return email;
        }

        private static EmailAddress GetEmailAddress(MailAddress address)
        {
            return new EmailAddress { Address = address.Address, Name = address.DisplayName };
        }

        private static Attachment CreateNewAttachment(System.Net.Mail.Attachment attachment)
        {
            var fileName = Path.GetFullPath(Path.GetRandomFileName());
            var fileStream = File.Open(fileName, FileMode.OpenOrCreate);
            attachment.ContentStream.CopyTo(fileStream);
            fileStream.Close();

            return new Attachment
            {
                FileLocation = fileName,
                Name = attachment.Name,
                Type = attachment.ContentType.MediaType
            };
        }

        public static void SendEmail(Email email)
        {
            new EmailManager().Send(email);
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var er = new EmailRepository(uow);

            var originalEmail = er.GetByKey(email.EmailID);
            email.Status = EmailStatusConstants.GetStatusRow(EmailStatusConstants.SENT);
            er.Update(email, originalEmail);
            uow.SaveChanges();
        }
    }
}
