﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using DayPilot.Web.Mvc.Json;
using KwasantWeb.Controllers.DayPilot;
using StructureMap;
using UtilitiesLib;
using Calendar = KwasantCore.Services.Calendar;

namespace KwasantWeb.Controllers
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
            BookingRequestDO = bookingRequestRepository.GetByKey(id);
            if (BookingRequestDO == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Calendar = new Calendar(uow, BookingRequestDO);
            return View(BookingRequestDO);
        }

        private Calendar Calendar
        {
            get
            {
                return Session["CalendarServices"] as Calendar;
            }
            set
            {
                Session["CalendarServices"] = value;
            }
        }

        private BookingRequestDO BookingRequestDO
        {
            get
            {
                return Session["BookingRequestDO"] as BookingRequestDO;
            }
            set
            {
                Session["BookingRequestDO"] = value;
            }
        }

        public ActionResult Day()
        {
            return new DayPilotCalendarControl(Calendar).CallBack(this);
        }


        public ActionResult Month()
        {
            return new DayPilotMonthControl(Calendar).CallBack(this);
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

        public ActionResult New(string start, string end)
        {
            EventDO eventDO = new EventDO
            {
                StartDate = DateTime.Parse(start),
                EndDate = DateTime.Parse(end),
                BookingRequest = BookingRequestDO,
            };
            //If there's no time component for the start date (ie starting at midnight), and the end is exactly 1 day ahead, it's an all-day-event
            if (eventDO.StartDate.Equals(eventDO.StartDate.Date) &&
                eventDO.StartDate.AddDays(1).Equals(eventDO.EndDate))
                eventDO.IsAllDay = true;

            eventDO.Attendees = new List<AttendeeDO>
            {
                new AttendeeDO
                {
                    EmailAddress = BookingRequestDO.From.Address,
                    Name = BookingRequestDO.From.Name,
                    Event = eventDO
                }
            };

            return View("~/Views/Calendar/Open.cshtml", eventDO);
        }

        public ActionResult Open(int eventID)
        {
            return View(
                Calendar.GetEvent(eventID)
                );
        }

        public ActionResult DeleteEvent(int eventID)
        {
            EventDO actualEventDO = Calendar.GetEvent(eventID);
            return View(actualEventDO);
        }

        public ActionResult ConfirmDelete(int eventID)
        {
            Calendar.DeleteEvent(eventID);
            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        public ActionResult MoveEvent(int eventID, String newStart, String newEnd)
        {
            //This is a fake event that will be thrown away if Confirm() is not called
            EventDO eventDO = new EventDO();
            eventDO.EventID = eventID;
            EventDO actualEventDO = Calendar.GetEvent(eventID);
            eventDO.CopyFrom(actualEventDO);

            DateTime newStartDT = DateTime.Parse(newStart);
            DateTime newEndDT = DateTime.Parse(newEnd);

            eventDO.StartDate = newStartDT;
            eventDO.EndDate = newEndDT;

            string key = Guid.NewGuid().ToString();
            Session["FakedEvent_" + key] = eventDO;
            return View("~/Views/Calendar/BeforeSave.cshtml", new ConfirmEvent
            {
                Key = key,
                EventDO = eventDO
            });
        }

        private static T GetValueFromForm<T>(NameValueCollection collection, String name, T defaultValue = default(T))
        {
            string obj = collection[name];
            if (obj == null)
                return defaultValue;

            Type returnType = typeof(T);
            if (returnType == typeof(bool))
                return (T)(object)(obj == "on" || obj == "1" || obj == "true");
            if (returnType == typeof(String))
                return (T)(object)obj;
            if (returnType == typeof(DateTime))
            {
                return (T)(object)DateTime.ParseExact(obj, "MM/dd/yyyy hh:mm tt", CultureInfo.CurrentCulture);
            }
            if (returnType == typeof (int))
                return (T) (object) obj.ToInt();
            throw new Exception("Invalid type provided");
        }

        /// <summary>
        /// This method creates a template eventDO which we store. This event is presented to the user to review & confirm changes. If they confirm, Confirm(FormCollection form) is invoked
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult BeforeSave(FormCollection form)
        {
            DateTime dtFromDate = GetValueFromForm(Request.QueryString, "DateStart", DateTime.MinValue);
            DateTime dtToDate = GetValueFromForm(Request.QueryString, "DateEnd", DateTime.MinValue);
            bool isAllDay = GetValueFromForm(Request.QueryString, "IsAllDay", false);
            string strLocation = GetValueFromForm(Request.QueryString, "Location", String.Empty);
            string strStatus = GetValueFromForm(Request.QueryString, "Status", String.Empty);
            string strTransparency = GetValueFromForm(Request.QueryString, "TransparencyType", String.Empty);
            string strClass = GetValueFromForm(Request.QueryString, "Class", String.Empty);
            string strDescription = GetValueFromForm(Request.QueryString, "Description", String.Empty);
            int intPriority = GetValueFromForm(Request.QueryString, "Priority", 0);
            int intSequence = GetValueFromForm(Request.QueryString, "Sequence", 0);
            string strSummary = GetValueFromForm(Request.QueryString, "Summary", String.Empty);
            string strCategory = GetValueFromForm(Request.QueryString, "Category", String.Empty);
            int eventID = GetValueFromForm(Request.QueryString, "EventID", 0);
            string attendeesStr = GetValueFromForm(Request.QueryString, "Attendees", String.Empty);

            //This is a fake event that will be thrown away if Confirm() is not called
            EventDO eventDO = new EventDO
            {
                EventID = eventID,
                IsAllDay = isAllDay,
                StartDate = dtFromDate,
                EndDate = dtToDate,
                Location = strLocation,
                Status = strStatus,
                Transparency = strTransparency,
                Class = strClass,
                Description = strDescription,
                Priority = intPriority,
                Sequence = intSequence,
                Summary = strSummary,
                Category = strCategory
            };

            ManageAttendees(eventDO, attendeesStr);

            string key = Guid.NewGuid().ToString();
            Session["FakedEvent_" + key] = eventDO;
            return View(
                new ConfirmEvent
                {
                    Key = key,
                    EventDO = eventDO
                }
            );
        }

        //Manages adds/deletes and persists of attendees.
        private void ManageAttendees(EventDO eventDO, string attendeesStr)
        {
            List<AttendeeDO> originalAttendees;
            if (eventDO.EventID != 0)
            {
                EventDO oldEvent = Calendar.GetEvent(eventDO.EventID);
                originalAttendees = new List<AttendeeDO>(oldEvent.Attendees);
            }
            else
            {
                originalAttendees = new List<AttendeeDO>();
            }
            List<AttendeeDO> newAttendees = new List<AttendeeDO>();
            foreach (string email in attendeesStr.Split(','))
            {
                if (String.IsNullOrEmpty(email))
                    continue;

                List<AttendeeDO> sameAttendees = originalAttendees.Where(oa => oa.EmailAddress == email).ToList();
                if (sameAttendees.Any())
                {
                    newAttendees.AddRange(sameAttendees);
                }
                else
                {
                    newAttendees.Add(new AttendeeDO
                    {
                        EmailAddress = email
                    });
                }
            }
            List<AttendeeDO> attendeesToDelete = originalAttendees.Where(originalAttendee => !newAttendees.Select(a => a.EmailAddress).Contains(originalAttendee.EmailAddress)).ToList();
            if (attendeesToDelete.Any())
            {
                AttendeeRepository attendeeRepo = new AttendeeRepository(Calendar.UnitOfWork);
                foreach (AttendeeDO attendeeToDelete in attendeesToDelete)
                    attendeeRepo.Remove(attendeeToDelete);
            }
            eventDO.Attendees = newAttendees;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(FormCollection form)
        {
            string key = GetValueFromForm(form, "key", string.Empty);

            EventDO eventDO = Session["FakedEvent_" + key] as EventDO;
            if (eventDO.EventID == 0)
            {
                Calendar.AddEvent(eventDO);
            }
            else
            {
                EventDO oldEvent = Calendar.GetEvent(eventDO.EventID);
                oldEvent.CopyFrom(eventDO);
                eventDO = oldEvent;
            }

            eventDO.BookingRequest = BookingRequestDO;

            Calendar.DispatchEvent(eventDO);

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        #endregion "Action"

        public class ConfirmEvent
        {
            public string Key;
            public EventDO EventDO;
        }
    }
}
