﻿using System;
using System.Collections.Generic;
using System.Text;
using Shnexy.DDay.Collections.Interfaces;

namespace Shnexy.DDay.iCal
{
    public interface ICalendarProperty :        
        ICalendarParameterCollectionContainer,
        ICalendarObject,
        IValueObject<object>
    {
        object Value { get; set; }
    }
}
