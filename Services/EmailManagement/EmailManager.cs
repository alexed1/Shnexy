﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using Shnexy.Models;
using Shnexy.Services.APIManagement.Packagers.Mandrill;

namespace Shnexy.Services.EmailManagement
{
    public class EmailManager
    {
        #region Members

        private MandrillPackager MandrillAPI;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        public EmailManager()
        {
            MandrillAPI = new MandrillPackager();
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        public void SendTemplate(string templateName, Email message, Dictionary<string, string> mergeFields)
        {
            var results = MandrillAPI.PostMessageSendTemplate(templateName, message, mergeFields);
        }


        public void Ping()
        {
            var results = MandrillAPI.PostPing();
            Debug.WriteLine(results);
        }

 

        #endregion

      
      
     

    }
}








