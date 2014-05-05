﻿using System;
using System.Configuration;
using Twilio;

namespace KwasantCore.Managers.APIManager.Packagers.Twilio
{
    public class TwilioPackager
    {
        private const string AccountSIDWebConfigName = "TwilioAccountSID";
        private const string AuthTokenWebConfigName = "TwilioAuthToken";
        private const string FromNumberWebConfigName = "TwilioFromNumber";

        private readonly TwilioRestClient _twilio;
        private readonly String _twilioFromNumber;
        public TwilioPackager()
        {
            string accountSID = ConfigurationManager.AppSettings[AccountSIDWebConfigName];
            string accountAuthKey = ConfigurationManager.AppSettings[AuthTokenWebConfigName];
            _twilioFromNumber = ConfigurationManager.AppSettings[FromNumberWebConfigName];

            if (String.IsNullOrEmpty(accountSID))
                throw new ArgumentNullException(AccountSIDWebConfigName, "Value must be set in web.config");

            if (String.IsNullOrEmpty(accountAuthKey))
                throw new ArgumentNullException(AuthTokenWebConfigName, "Value must be set in web.config");

            if (String.IsNullOrEmpty(_twilioFromNumber))
                throw new ArgumentNullException(FromNumberWebConfigName, "Value must be set in web.config");

            _twilio = new TwilioRestClient(accountSID, accountAuthKey);
        }

        public SMSMessage SendSMS(String number, String message)
        {
            return _twilio.SendSmsMessage(_twilioFromNumber, number, message);
        }
    }
}