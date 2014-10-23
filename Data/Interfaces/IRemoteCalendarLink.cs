using System;

namespace Data.Interfaces
{
    public interface IRemoteCalendarLink
    {
        int Id { get; set; }
        string RemoteCalendarHref { get; set; }
        string RemoteCalendarName { get; set; }
        bool IsDisabled { get; set; }

        int? LocalCalendarID { get; set; }
        ICalendar LocalCalendar { get; set; }

        int? ProviderID { get; set; }
        IRemoteCalendarProvider Provider { get; set; }

        DateTimeOffset DateSynchronizationAttempted { get; set; }
        DateTimeOffset DateSynchronized { get; set; }
        string LastSynchronizationResult { get; set; }
    }
}