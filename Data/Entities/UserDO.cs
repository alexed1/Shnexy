﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Infrastructure;
using Data.States;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

using Data.Interfaces;
using StructureMap;
using System.ComponentModel.DataAnnotations;
using Data.States.Templates;

namespace Data.Entities
{
    public class UserDO : IdentityUser, IUserDO, ICreateHook, IBaseDO, ISaveHook
    {
        [NotMapped]
        IEmailAddressDO IUserDO.EmailAddress
        {
            get { return EmailAddress; }
        }

        public UserDO()
        {
            UserBookingRequests = new List<BookingRequestDO>();
            BookerBookingRequests = new List<BookingRequestDO>();
            Calendars = new List<CalendarDO>();
            RemoteCalendarAuthData = new List<RemoteCalendarAuthDataDO>();
            Profiles = new List<ProfileDO>();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Boolean TestAccount { get; set; }

        //Booker only. Needs to be nullable otherwise DefaultValue doesn't work
        public bool? Available { get; set; }

        [ForeignKey("EmailAddress")]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        [Required, ForeignKey("UserStateTemplate"), DefaultValue(UserState.Active)]
        public int? State { get; set; }
        public virtual _UserStateTemplate UserStateTemplate { get; set; }
        
        [InverseProperty("Customer")]
        public virtual IList<BookingRequestDO> UserBookingRequests { get; set; }

        [InverseProperty("Booker")]
        public virtual IList<BookingRequestDO> BookerBookingRequests { get; set; }

        [InverseProperty("Owner")]
        public virtual IList<CalendarDO> Calendars { get; set; }

        [InverseProperty("User")]
        public virtual IList<ProfileDO> Profiles { get; set; }

        [InverseProperty("User")]
        public virtual IList<RemoteCalendarAuthDataDO> RemoteCalendarAuthData { get; set; }

        public bool IsRemoteCalendarAccessGranted(string providerName)
        {
            return RemoteCalendarAuthData
                .Any(r =>
                     r.Provider != null &&
                     r.Provider.Name == providerName &&
                     r.HasAccessToken());
        }

        void ICreateHook.BeforeCreate()
        {
            CreateDate = DateTimeOffset.Now;
        }

        void ICreateHook.AfterCreate()
        {
            //we only want to treat explicit customers, who have sent us a BR, a welcome message
            //if there exists a booking request with this user as its created by...
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (uow.BookingRequestRepository.FindOne(br => br.Customer.Id == Id) != null)
                    AlertManager.ExplicitCustomerCreated(Id);
            }

            AlertManager.CustomerCreated(this);
        }

        void ISaveHook.BeforeSave()
        {
            LastUpdated = DateTimeOffset.Now;
        }

        public String DisplayName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(FirstName) && !String.IsNullOrWhiteSpace(LastName))
                    return FirstName + " " + LastName;
                if (!String.IsNullOrWhiteSpace(FirstName))
                    return FirstName;
                if (!String.IsNullOrWhiteSpace(LastName))
                    return LastName;
                return UserName;
            }
        }

        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}

