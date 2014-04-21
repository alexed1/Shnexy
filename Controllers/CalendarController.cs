﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.Models;
using Data.DataAccessLayer.Repositories;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Services;
using Shnexy.Controllers.DayPilot;
using StructureMap;

namespace Shnexy.Controllers
{
    [HandleError]
    public class CalendarController : Controller
    {
        #region "Action"
         
        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            

            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            IBookingRequestRepository bookingRequestRepository = new BookingRequestRepository(uow);
            BookingRequestDO bookingRequestDO = bookingRequestRepository.GetByKey(id);
            if (bookingRequestDO == null) 
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Calendar = new CalendarServices(uow, bookingRequestDO);
            return View(bookingRequestDO);                       

        }

        private CalendarServices Calendar
        {
            get
            {
                return Session["EventManager"] as CalendarServices;
            }
            set
            {
                Session["EventManager"] = value;
            }
        }

        public ActionResult Day()
        {
            return new DayPilotCalendarControl(Calendar).CallBack(this);
        }

        public ActionResult Rtl()
        {
            return View();
        }
        public ActionResult Columns50()
        {
            return View();
        }
        public ActionResult Height100Pct()
        {
            return View();
        }

        public ActionResult Notify()
        {
            return View();
        }

        public ActionResult Crosshair()
        {
            return View();
        }

        public ActionResult ThemeBlue()
        {
            return View();
        }

        public ActionResult ThemeGreen()
        {
            return View();
        }

        public ActionResult ThemeWhite()
        {
            return View();
        }

        public ActionResult ThemeTraditional()
        {
            return View();
        }

        public ActionResult ThemeTransparent()
        {
            return View();
        }

        public ActionResult TimeHeaderCellDuration()
        {
            return View();
        }

        public ActionResult ActiveAreas()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult HeaderAutoFit()
        {
            return View();
        }

        public ActionResult ExternalDragDrop()
        {
            return View();
        }

        public ActionResult Open()
        {
            return View();
        }

        public ActionResult EventArrangement()
        {
            return View();
        }

        public ActionResult AutoRefresh()
        {
            return View();
        }

        public ActionResult Today()
        {
            return View();
        }

        public ActionResult DaysResources()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        public ActionResult ContextMenu()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }

        public ActionResult DayRange()
        {
            return View();
        }

        public ActionResult EventSelecting()
        {
            return View();
        }

        public ActionResult AutoHide()
        {
            return View();
        }

        public ActionResult GoogleLike()
        {
            return View();
        }

        public ActionResult RecurringEvents()
        {
            return View();
        }

        public ActionResult ThemeSilver()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult ThemeGreenWithBar()
        {
            return RedirectToAction("ThemeGreen");
        }

        public ActionResult Outlook2000()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult Backend()
        {
            return new DayPilotCalendarControl(Calendar).CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new DayPilotNavigatorControl().CallBack(this);
        }

        public ActionResult New(int emailID, string partStart, string partEnd)
        {
            return View(
                new CreateInvitationInfo
                {
                    EmailID = emailID,
                    DateStart = partStart,
                    DateEnd = partEnd
                }
            );
        }

        private static T GetValueFromForm<T>(FormCollection form, String name, T defaultValue = default(T))
        {
            string obj = form[name];
            if (obj == null)
                return defaultValue;

            Type returnType = typeof (T);
            if (returnType == typeof (String))
                return (T)(object)obj;
            if (returnType == typeof(DateTime))
            {
                return (T)(object)Convert.ToDateTime(obj);
            }
            if (returnType == typeof (int))
                return (T)(object)Convert.ToInt32(obj);
            throw new Exception("Invalid type provided");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(FormCollection form)
        {
            DateTime dtFromDate = GetValueFromForm(form, "DateStart", DateTime.MinValue);
            DateTime dtToDate = GetValueFromForm(form, "DateEnd", DateTime.MinValue);
            string strLocation = GetValueFromForm(form, "Location", String.Empty);
            string strStatus = GetValueFromForm(form, "Status", String.Empty);
            string strTransparency = GetValueFromForm(form, "TransparencyType", String.Empty);
            string strClass = GetValueFromForm(form, "Class", String.Empty);
            string strDescription = GetValueFromForm(form, "Description", String.Empty);
            int intPriority = GetValueFromForm(form, "Priority", 0);
            int intSequence = GetValueFromForm(form, "Sequence", 0);
            string strSummary = GetValueFromForm(form, "Summary", String.Empty);
            string strCategory = GetValueFromForm(form, "Category", String.Empty);
            int bookingRequestID = GetValueFromForm(form, "BookingRequestID", 0);

            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            InvitationRepository invitationRepository = new InvitationRepository(uow);
            InvitationDO invitationDO = new InvitationDO();
            invitationRepository.Add(invitationDO);

            BookingRequestRepository bookingRequestRepository = new BookingRequestRepository(uow);
            BookingRequestDO bookingRequestDO = bookingRequestRepository.GetByKey(bookingRequestID);
            bookingRequestDO.StatusID = EmailStatusConstants.PROCESSED;
            if (bookingRequestDO.Invitations == null)
                bookingRequestDO.Invitations = new List<InvitationDO>();
            bookingRequestDO.Invitations.Add(invitationDO);

            invitationDO.Attendees = new List<AttendeeDO>();
            invitationDO.Attendees.Add(new AttendeeDO
            {
                EmailAddress = bookingRequestDO.From.Address,
                Name = bookingRequestDO.From.Name,
                Invitation = invitationDO
            });
            //We also need to have the form show attendees

            invitationDO.StartDate = dtFromDate;
            invitationDO.EndDate = dtToDate;
            invitationDO.Location = strLocation;
            invitationDO.Status = strStatus;
            invitationDO.Transparency = strTransparency;
            invitationDO.Class = strClass;
            invitationDO.Description = strDescription;
            invitationDO.Priority = intPriority;
            invitationDO.Sequence = intSequence;
            invitationDO.Summary = strSummary;
            invitationDO.Category = strCategory;
            invitationDO.BookingRequest = bookingRequestDO;

            Calendar.DispatchEvent(invitationDO);

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        #endregion "Action"

        

        public class CreateInvitationInfo
        {
            public int EmailID;
            public string DateStart;
            public string DateEnd;
        }

    }
}
