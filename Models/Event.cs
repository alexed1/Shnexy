using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.DDay.iCal;

//This was originally DDay Code. 
using Shnexy.DDay.iCal.Serialization.iCalendar;
using Shnexy.Services.EmailManagement;

namespace Shnexy.Models
{
    /// <summary>
    /// A class that represents an RFC 5545 VEVENT component.
    /// </summary>
    /// <note>
    ///     TODO: Add support for the following properties:
    ///     <list type="bullet">
    ///         <item>Add support for the Organizer and Attendee properties</item>
    ///         <item>Add support for the Class property</item>
    ///         <item>Add support for the Geo property</item>
    ///         <item>Add support for the Priority property</item>
    ///         <item>Add support for the Related property</item>
    ///         <item>Create a TextCollection DataType for 'text' items separated by commas</item>
    ///     </list>
    /// </note>

    [Serializable]    
    public class Event : 
        RecurringComponent,
        IEvent
    {
        #region Public Properties

        public int Id { get; set; }
        public int CustomerId { get; set; } //added by shnexy
        /// <summary>
        /// The start date/time of the event.
        /// <note>
        /// If the duration has not been set, but
        /// the start/end time of the event is available,
        /// the duration is automatically determined.
        /// Likewise, if the end date/time has not been
        /// set, but a start and duration are available,
        /// the end date/time will be extrapolated.
        /// </note>
        /// </summary>
        public override IDateTime DTStart
        {
            get
            {
                return base.DTStart;
            }
            set
            {
                base.DTStart = value;
                ExtrapolateTimes();
            }
        }

        /// <summary>
        /// The end date/time of the event.
        /// <note>
        /// If the duration has not been set, but
        /// the start/end time of the event is available,
        /// the duration is automatically determined.
        /// Likewise, if an end time and duration are available,
        /// but a start time has not been set, the start time
        /// will be extrapolated.
        /// </note>
        /// </summary>
        virtual public IDateTime DTEnd
        {
            get { return Properties.Get<IDateTime>("DTEND"); }
            set
            {
                if (!object.Equals(DTEnd, value))
                {
                    Properties.Set("DTEND", value);
                    ExtrapolateTimes();
                }
            }
        }

        /// <summary>
        /// The duration of the event.
        /// <note>
        /// If a start time and duration is available,
        /// the end time is automatically determined.
        /// Likewise, if the end time and duration is
        /// available, but a start time is not determined,
        /// the start time will be extrapolated from
        /// available information.
        /// </note>
        /// </summary>
        // NOTE: Duration is not supported by all systems,
        // (i.e. iPhone) and cannot co-exist with DTEnd.
        // RFC 5545 states:
        //
        //      ; either 'dtend' or 'duration' may appear in
        //      ; a 'eventprop', but 'dtend' and 'duration'
        //      ; MUST NOT occur in the same 'eventprop'
        //
        // Therefore, Duration is not serialized, as DTEnd
        // should always be extrapolated from the duration.
        virtual public TimeSpan Duration
        {
            get { return Properties.Get<TimeSpan>("DURATION"); }
            set
            {
                if (!object.Equals(Duration, value))
                {
                    Properties.Set("DURATION", value);
                    ExtrapolateTimes();
                }
            }
        }

        /// <summary>
        /// An alias to the DTEnd field (i.e. end date/time).
        /// </summary>
        virtual public IDateTime End
        {
            get { return DTEnd; }
            set { DTEnd = value; }
        }

        /// <summary>
        /// Returns true if the event is an all-day event.
        /// </summary>
        virtual public bool IsAllDay
        {
            get { return !Start.HasTime; }
            set
            {
                // Set whether or not the start date/time
                // has a time value.
                if (Start != null)
                    Start.HasTime = !value;
                if (End != null)
                    End.HasTime = !value;

                if (value && 
                    Start != null &&
                    End != null &&
                    object.Equals(Start.Date, End.Date))
                {
                    Duration = default(TimeSpan);
                    End = Start.AddDays(1);
                }
            }
        }

        /// <summary>
        /// The geographic location (lat/long) of the event.
        /// </summary>
        public IGeographicLocation GeographicLocation
        {
            get { return Properties.Get<IGeographicLocation>("GEO"); }
            set { Properties.Set("GEO", value); }
        }

        /// <summary>
        /// The location of the event.
        /// </summary>
        public string Location
        {
            get { return Properties.Get<string>("LOCATION"); }
            set { Properties.Set("LOCATION", value); }
        }

        /// <summary>
        /// Resources that will be used during the event.
        /// <example>Conference room #2</example>
        /// <example>Projector</example>
        /// </summary>
        public IList<string> Resources
        {
            get { return Properties.GetMany<string>("RESOURCES"); }
            set { Properties.Set("RESOURCES", value); }
        }

        /// <summary>
        /// The status of the event.
        /// </summary>
        public EventStatus Status
        {
            get { return Properties.Get<EventStatus>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        }

        public String WorkflowState
        {
            get { return Properties.Get<string>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        }

        /// <summary>
        /// The transparency of the event.  In other words,
        /// whether or not the period of time this event
        /// occupies can contain other events (transparent),
        /// or if the time cannot be scheduled for anything
        /// else (opaque).
        /// </summary>
        public TransparencyType Transparency
        {
            get { return Properties.Get<TransparencyType>("TRANSP"); }
            set { Properties.Set("TRANSP", value); }
        }

        #endregion

        #region Private Fields

        EventEvaluator m_Evaluator;
        private ICustomer curCustomer;
        private IUnitOfWork _uow;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an Event object, with an <see cref="iCalObject"/>
        /// (usually an iCalendar object) as its parent.
        /// </summary>
        /// <param name="parent">An <see cref="iCalObject"/>, usually an iCalendar object.</param>
        public Event(IUnitOfWork uow) : base()
        {
            Initialize();
            _uow = uow;
            curCustomer = new Customer(new CustomerRepository(_uow));
        }

        //there are things deep within DDay that create Events, and we don't want to force the unit of work down there.
        public Event()
            : base()
        {
            Initialize();
       
        }

        private void Initialize()
        {
            this.Name = Components.EVENT;

            m_Evaluator = new EventEvaluator(this);
            SetService(m_Evaluator);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Use this method to determine if an event occurs on a given date.
        /// <note type="caution">
        ///     This event should be called only after the <see cref="Evaluate"/>
        ///     method has calculated the dates for which this event occurs.
        /// </note>
        /// </summary>
        /// <param name="DateTime">The date to test.</param>
        /// <returns>True if the event occurs on the <paramref name="DateTime"/> provided, False otherwise.</returns>
        virtual public bool OccursOn(IDateTime DateTime)
        {
            foreach (IPeriod p in m_Evaluator.Periods)
                // NOTE: removed UTC from date checks, since a date is a date.
                if (p.StartTime.Date == DateTime.Date ||    // It's the start date OR
                    (p.StartTime.Date <= DateTime.Date &&   // It's after the start date AND
                    (p.EndTime.HasTime && p.EndTime.Date >= DateTime.Date || // an end time was specified, and it's after the test date
                    (!p.EndTime.HasTime && p.EndTime.Date > DateTime.Date)))) // an end time was not specified, and it's before the end date
                    // NOTE: fixed bug as follows:
                    // DTSTART;VALUE=DATE:20060704
                    // DTEND;VALUE=DATE:20060705
                    // Event.OccursOn(new iCalDateTime(2006, 7, 5)); // Evals to true; should be false
                    return true;
            return false;
        }

        /// <summary>
        /// Use this method to determine if an event begins at a given date and time.
        /// </summary>
        /// <param name="DateTime">The date and time to test.</param>
        /// <returns>True if the event begins at the given date and time</returns>
        virtual public bool OccursAt(IDateTime DateTime)
        {
            foreach (IPeriod p in m_Evaluator.Periods)
                if (p.StartTime.Equals(DateTime))
                    return true;
            return false;
        }

        /// <summary>
        /// Determines whether or not the <see cref="Event"/> is actively displayed
        /// as an upcoming or occurred event.
        /// </summary>
        /// <returns>True if the event has not been cancelled, False otherwise.</returns>
        virtual public bool IsActive()
        {
            return (Status != EventStatus.Cancelled);            
        }

        #endregion

        #region Overrides

        protected override bool EvaluationIncludesReferenceDate
        {
            get
            {
                return true;
            }
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected override void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);

            ExtrapolateTimes();
        }        
        
        #endregion

        #region Private Methods

        private void ExtrapolateTimes()
        {
            if (DTEnd == null && DTStart != null && Duration != default(TimeSpan))
                DTEnd = DTStart.Add(Duration);
            else if (Duration == default(TimeSpan) && DTStart != null && DTEnd != null)
                Duration = DTEnd.Subtract(DTStart);
            else if (DTStart == null && Duration != default(TimeSpan) && DTEnd != null)
                DTStart = DTEnd.Subtract(Duration);
        }
        #endregion

        #region Added by Shnexy

        //extract the ICS, and put it into an outbound email message in the outbound queue
        public void Dispatch()
        {
            


            iCalendar iCal = new iCalendar();
            iCal.AddChild(this);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
           // string Body = serializer.Serialize(iCal); but we don't need the body right now

            //generate a unique file name, using customer id + eventID. 
            //EventId should be added to the event and synced to the customer and incremented.
            string filename = "invite_" + CustomerId.ToString() + "_" + Id.ToString();
            serializer.Serialize(filename);

           

            //Get Customer using CustomerId. retrieve the email target address
            curCustomer = curCustomer.GetByKey(CustomerId);

            //create an Email message addressed to the customer and attach the file.
            Email curEmail = new Email(new EmailRepository(_uow));
            curEmail.Configure(curCustomer.emailAddr, Id, filename);
            curEmail.FromEmail = "alex@edelstein.org";
            curEmail.FromName = "Alexed";
            curEmail.Text = "this is the body. ICS should be attached";
            curEmail.Subject = "Meeting Request from Alex Edelstein Sent by Booqit";

            //call Mandrill. need to reconcile the two email structures.
            EmailManager curEmailManager = new EmailManager();

            Attachment curAttachment = new Attachment();
            curAttachment.Name = filename;
            curAttachment.Type = "text/plain";
            curAttachment.Content = File.ReadAllText(filename);
            curEmail.Attachments.Add(curAttachment);

            curEmailManager.Send(curEmail);


            //skip for v.1: add EmailID to outbound queue


        }

        #endregion
    }
}
