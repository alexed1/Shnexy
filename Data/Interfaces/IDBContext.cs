﻿using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IDBContext
    {
        int SaveChanges();

        IDbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;


        /* DBSets */
        IDbSet<InstructionDO> Instructions { get; }
    }
}
