﻿using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Repositories
{


    public class InvitationRepository : GenericRepository<InvitationDO>, IInvitationRepository
    {

        public InvitationRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IInvitationRepository : IGenericRepository<InvitationDO>
    {

    }
}
