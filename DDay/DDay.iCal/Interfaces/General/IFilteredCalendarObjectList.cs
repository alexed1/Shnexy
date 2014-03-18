﻿using System;
using System.Collections.Generic;
using System.Collections;
using Shnexy.DDay.Collections;

namespace Shnexy.DDay.iCal
{
    public interface IFilteredCalendarObjectList<T> :
        ICollection<T>
        where T : ICalendarObject
    {
        event EventHandler<ObjectEventArgs<T>> ItemAdded;
        event EventHandler<ObjectEventArgs<T>> ItemRemoved;
        T this[int index] { get; }
        int IndexOf(T item);
    }
}
