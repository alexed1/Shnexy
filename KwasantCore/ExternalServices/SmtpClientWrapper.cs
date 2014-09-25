﻿using System.Net;
using System.Net.Mail;

namespace KwasantCore.ExternalServices
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private SmtpClient _internalClient;
        public void Initialize(string serverURL, int serverPort)
        {
            _internalClient = new SmtpClient(serverURL, serverPort);
        }

        public bool EnableSsl { get { return _internalClient.EnableSsl; } set { _internalClient.EnableSsl = value; } }
        public bool UseDefaultCredentials { get { return _internalClient.UseDefaultCredentials; } set { _internalClient.UseDefaultCredentials = value; } }
        public ICredentialsByHost Credentials { get { return _internalClient.Credentials; } set { _internalClient.Credentials = value; } }
        public void Send(MailMessage message)
        {
            _internalClient.Send(message);
        }
    }
}
