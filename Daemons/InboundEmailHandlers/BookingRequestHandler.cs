using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using System.Linq;
using StructureMap;

namespace Daemons.InboundEmailHandlers
{
    class BookingRequestHandler : IInboundEmailHandler
    {
        #region Implementation of IInboundEmailHandler

        public bool Process(MailMessage message)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailRepository emailRepo = uow.EmailRepository;
                EmailDO email = Email.ConvertMailMessageToEmail(emailRepo, message);

                var existingBookingRequest = (from t in uow.BookingRequestRepository.GetAll()
                            where t.Subject == email.Subject
                            && (t.Recipients.Any(e => e.EmailID == email.From.Id) || t.FromID == email.From.Id)
                            select t).FirstOrDefault();

                if (existingBookingRequest != null)
                {
                    email.ConversationId = existingBookingRequest.Id;
                    uow.UserRepository.GetOrCreateUser(email.From);
                    uow.SaveChanges();
                }
                else
                {
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(uow.BookingRequestRepository, message);
                    (new BookingRequest()).Process(uow, bookingRequest);
                    uow.SaveChanges();
                    AlertManager.BookingRequestCreated(bookingRequest.Id);
                    AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
                }
            }
            return true;
        }

        #endregion
    }
}