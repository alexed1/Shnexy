﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.DataAccessLayer.Interfaces;
using Data.Models;

namespace Data.DataAccessLayer.Infrastructure
{


    public class ShnexyDbContext : DbContext
    {       
        //see web.config for connection string names.
        //azure is AzureAlexTestDb
        public ShnexyDbContext()
            : base("name=AzureAlexTestDb")
        {
            
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            var adds = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            var deletes = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();
            var modifies = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified)
                .Select(e =>
                {
                    const string displayChange = "[{0}]: [{1}] -> [{2}]";
                    var changedValues = new List<String>();
                    foreach (var prop in e.OriginalValues.PropertyNames)
                    {
                        object originalValue = e.OriginalValues[prop];
                        object currentValue = e.CurrentValues[prop];
                        if ((originalValue == null && currentValue != null) || (originalValue != null && !originalValue.Equals(currentValue)))
                        {
                            changedValues.Add(String.Format(displayChange, prop, originalValue,
                                currentValue));
                        }
                    }

                    var actualName = (e.Entity.GetType().FullName.StartsWith("System.Data.Entity.DynamicProxies") && e.Entity.GetType().BaseType != null)
                        ? e.Entity.GetType().BaseType.Name
                        : e.Entity.GetType().FullName;
                    return new
                    {
                        EntityName = actualName,
                        ChangedValue = changedValues
                    };
                })
                .Where(e => e.ChangedValue != null && e.ChangedValue.Count > 0)
                .ToList();
            foreach (DbEntityEntry<ISaveHook> entity in ChangeTracker.Entries<ISaveHook>().Where(e => e.State != EntityState.Unchanged))
            {
                entity.Entity.SaveHook(entity);
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentDO>().ToTable("Attachments");
            modelBuilder.Entity<AttendeeDO>().ToTable("Attendees");
            modelBuilder.Entity<BookingRequestDO>().ToTable("BookingRequests");
            modelBuilder.Entity<CustomerDO>().ToTable("Customers");
            modelBuilder.Entity<EmailAddressDO>().ToTable("EmailAddresses");
            modelBuilder.Entity<EmailDO>().ToTable("Emails");
            modelBuilder.Entity<EmailStatusDO>().ToTable("EmailStatuses");
            modelBuilder.Entity<InvitationDO>().ToTable("Events");
            modelBuilder.Entity<StoredFileDO>().ToTable("StoredFiles");
            modelBuilder.Entity<UserDO>().ToTable("Users");

            modelBuilder.Entity<InvitationDO>()
                .HasMany(ev => ev.Emails)
                .WithMany(e => e.Invitations)
                .Map(
                    mapping => mapping.MapLeftKey("EventID").MapRightKey("EmailID").ToTable("EventEmail")
                );
            
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.From);
            modelBuilder.Entity<EmailDO>()
                .HasRequired(e => e.Status)
                .WithMany(es => es.Emails)
                .HasForeignKey(e => e.StatusID);

            modelBuilder.Entity<EmailDO>()
                .HasMany(e => e.To)
                .WithOptional(ea => ea.ToEmail)
                .Map(m => m.MapKey("ToEmailID"));
            modelBuilder.Entity<EmailDO>()
                .HasMany(e => e.BCC)
                .WithOptional(ea => ea.BCCEmail)
                .Map(m => m.MapKey("BCCmailID"));
            modelBuilder.Entity<EmailDO>()
                .HasMany(e => e.CC)
                .WithOptional(ea => ea.CCEmail)
                .Map(m => m.MapKey("CCEmailID"));

            modelBuilder.Entity<AttachmentDO>()
                .HasRequired(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailID);

            modelBuilder.Entity<EmailAddressDO>()
                .HasOptional(ea => ea.FromEmail)
                .WithRequired(e => e.From)
                .Map(x => x.MapKey("FromEmailAddressID"));

            modelBuilder.Entity<InvitationDO>()
                .HasMany(e => e.Attendees)
                .WithRequired(a => a.Invitation)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AttachmentDO> Attachments { get; set; }

        public DbSet<AttendeeDO> Attendees { get; set; }

        public DbSet<BookingRequestDO> BookingRequests { get; set; }

        public DbSet<CustomerDO> Customers { get; set; }

        public DbSet<EmailDO> Emails { get; set; }

        public DbSet<EmailAddressDO> EmailAddresses { get; set; }

        public DbSet<EmailStatusDO> EmailStatuses { get; set; }
        
        public DbSet<InvitationDO> Invitations { get; set; }

        public DbSet<StoredFileDO> StoredFiles { get; set; }

        public DbSet<UserDO> Users { get; set; }
        
    }
}