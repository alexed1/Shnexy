﻿using System;

namespace KwasantICS.DDay.iCal.Interfaces.Serialization.Factory
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, ISerializationContext ctx);
    }
}
