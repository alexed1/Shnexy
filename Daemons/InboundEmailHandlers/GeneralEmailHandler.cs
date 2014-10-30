using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using System.Linq;
using StructureMap;
using Data.States;
using Utilities.Logging;

namespace Daemons.InboundEmailHandlers
{
    class GeneralEmailHandler : IInboundEmailHandler
    {
        #region Implementation of IInboundEmailHandler

        public bool Process(MailMessage message)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailDO email = Email.ConvertMailMessageToEmail(uow.EmailRepository, message);
                Email.ProcessReceivedMessage(uow, email, message);
            }
            return true;
        }

        #endregion
    }
}