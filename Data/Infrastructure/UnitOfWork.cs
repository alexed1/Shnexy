﻿using System;
using System.Transactions;
using Data.Interfaces;

namespace Data.Infrastructure
{
     public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope transaction;
        public IDBContext db;


        public UnitOfWork(IDBContext curDbContext)
        {
            db = curDbContext;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (transaction != null)
                transaction.Dispose();
        }

        public void StartTransaction()
        {
            transaction = new TransactionScope();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Commit()
        {
            db.SaveChanges();
            transaction.Complete();
            transaction.Dispose();
        }
        public void SaveChanges()
        {

            db.SaveChanges();



        }

        public IDBContext Db
        {
            get { return db; }
        }

    }
}
