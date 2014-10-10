using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace Daemons.InboundEmailHandlers
{
    class InvitationResponseHandler : IInboundEmailHandler
    {
        public bool Process(MailMessage message)
        {
            if (!IsInvitationResponse(message))
                return false;
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                InvitationResponseDO curInvitationResponse = Email.ConvertMailMessageToEmail(uow.InvitationResponseRepository, message);

                (new InvitationResponse()).Process(uow, curInvitationResponse);

                uow.SaveChanges();

                AlertManager.EmailReceived(curInvitationResponse.Id, uow.UserRepository.GetOrCreateUser(curInvitationResponse.From).Id);
            }
            return true;
        }

        private bool IsInvitationResponse(MailMessage message)
        {
            var attachedCalendar = message.AlternateViews
                .FirstOrDefault(av => string.Equals(av.ContentType.MediaType, "text/calendar", StringComparison.Ordinal));
            if (attachedCalendar != null)
            {
                string content;
                using (var contentStream = new MemoryStream())
                {
                    attachedCalendar.ContentStream.CopyTo(contentStream);
                    attachedCalendar.ContentStream.Position = 0;
                    contentStream.Position = 0;
                    using (var sr = new StreamReader(contentStream))
                    {
                        content = sr.ReadToEnd();
                    }
                }
                if (content.Contains("METHOD:REPLY"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}