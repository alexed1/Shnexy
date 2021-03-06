﻿using System;
using System.IO;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.General;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.DataTypes;

namespace KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.Other
{
    public class EnumSerializer :
        EncodableDataTypeSerializer
    {
        #region Private Fields

        Type m_EnumType;

        #endregion

        #region Constructors

        public EnumSerializer(Type enumType)
        {
            m_EnumType = enumType;
        }

        #endregion

        public override Type TargetType
        {
            get { return m_EnumType; }
        }

        public override string SerializeToString(object enumValue)
        {
            try
            {
                ICalendarObject obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Encode the value as needed.
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = obj;
                    return Encode(dt, enumValue.ToString());
                }
                return enumValue.ToString();
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            try
            {
                ICalendarObject obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Decode the value, if necessary!
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = obj;
                    value = Decode(dt, value);
                }

                // Remove "-" characters while parsing Enum values.
                return Enum.Parse(m_EnumType, value.Replace("-", ""), true);
            }
            catch { }

            return value;
        }
    }
}
