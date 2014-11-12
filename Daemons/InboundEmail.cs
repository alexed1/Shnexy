﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using Data.Infrastructure;
using KwasantCore.ExternalServices;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using S22.Imap;
using StructureMap;
using Utilities;
using Utilities.Logging;
using IImapClient = KwasantCore.ExternalServices.IImapClient;

namespace Daemons
{
    public class InboundEmail : Daemon<InboundEmail>
    {
        private IImapClient _client;
        private readonly IConfigRepository _configRepository;
        private HashSet<String> _ignoreEmailsFrom;

        private readonly HashSet<String> _testSubjects = new HashSet<string>(); 
        public void RegisterTestEmailSubject(String subject)
        {
            lock (_testSubjects)
                _testSubjects.Add(subject);
        }

        public delegate void ExplicitCustomerCreatedHandler(string subject);
        public static event ExplicitCustomerCreatedHandler TestMessageReceived;

        private string _fromEmailAddress;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _intakeManager = ObjectFactory.GetInstance<IIntakeManager>();
            _ignoreEmailsFrom = new HashSet<string>();
            var ignoreEmailsString = _configRepository.Get("IgnoreEmailsFrom", String.Empty);
            if (!String.IsNullOrWhiteSpace(ignoreEmailsString))
            {
                foreach (var emailToIgnore in ignoreEmailsString.Split(','))
                    _ignoreEmailsFrom.Add(emailToIgnore);
            }


            _fromEmailAddress = _configRepository.Get("EmailAddress_GeneralInfo");
            AddTest("OutboundEmailDaemon_Test", "Test");
        }

        private string GetIMAPServer()
        {
            return _configRepository.Get("InboundEmailHost");
        }

        private int GetIMAPPort()
        {
            return _configRepository.Get<int>("InboundEmailPort");
        }

        public String UserName;
        public string GetUserName()
        {
            return UserName ?? _configRepository.Get("INBOUND_EMAIL_USERNAME");
        }

        public String Password;
        private string GetPassword()
        {
            return Password ?? _configRepository.Get("INBOUND_EMAIL_PASSWORD");
        }

        private bool UseSSL()
        {
            return _configRepository.Get<bool>("InboundEmailUseSSL");
        }

        public override int WaitTimeBetweenExecution
        {
            get
            {
                return (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
            }
        }

        private bool _alreadyListening;
        private readonly object _alreadyListeningLock = new object();
        private readonly IIntakeManager _intakeManager;

        protected override void Run()
        {
            LogEvent();
            lock (_alreadyListeningLock)
            {
                if (!_alreadyListening)
                {
                    _client = ObjectFactory.GetInstance<IImapClient>();
                    _client.Initialize(GetIMAPServer(), GetIMAPPort(), UseSSL());

                    string curUser = GetUserName();
                    string curPwd = GetPassword();
                    _client.Login(curUser, curPwd, AuthMethod.Login);

                    LogEvent("Waiting for messages at " + GetUserName() + "...");
                    _client.NewMessage += OnNewMessage;
                    _client.IdleError += OnIdleError;
                    
                    GetUnreadMessages(_client);

                    _alreadyListening = true;
                }
            }
        }

        private void OnIdleError(object sender, IdleErrorEventArgsWrapper args)
        {
            //Instead of logging it as an error - log it as an event. This doesn't mean we've lost any emails, so there's no reason to reduce success %.
            //This happens often on Kwasant - yet to be diagnosed.
            var eventName = "Idle error recieved.";
            var currException = args.Exception;
            var exceptionMessages = new List<String>();
            while (currException != null)
            {
                exceptionMessages.Add(currException.Message);
                currException = currException.InnerException;
            }
            exceptionMessages.Add("*** Stacktrace ***");
            exceptionMessages.Add(args.Exception.StackTrace);

            var exceptionMessage = String.Join(Environment.NewLine, exceptionMessages);

            eventName += " " + exceptionMessage;
            LogEvent(eventName);

            RestartClient();
        }

        public void OnNewMessage(object sender, IdleMessageEventArgsWrapper args)
        {
            LogEvent("New email notification received.");
            GetUnreadMessages(args.Client);
        }

        private void RestartClient()
        {
            lock (_alreadyListeningLock)
            {
                LogEvent("Restarting...");

                CleanUp();
            }
        }
        
        private void GetUnreadMessages(IImapClient client)
        {
            try
            {
                LogEvent("Querying for messages...");
                var messages = client.GetMessages(client.Search(SearchCondition.Unseen())).ToList();
                LogSuccess(messages.Count + " messages received.");

                foreach (var message in messages)
                    ProcessMessageInfo(message);
            }
            catch (SocketException ex) //we were getting strange socket errors after time, and it looks like a reset solves things
            {
                AlertManager.EmailProcessingFailure(DateTime.Now.to_S(), "Got that SocketException");
                LogFail(ex, "Hit SocketException. Trying to reset the IMAP Client.");
                RestartClient();
            }
            catch (Exception e)
            {
                LogFail(e);
                RestartClient();
            }
        }

        private void ProcessMessageInfo(MailMessage messageInfo)
        {
            var logString = "Processing message with subject '" + messageInfo.Subject + "'";
            Logger.GetLogger().Info(logString);
            LogEvent(logString);

            lock (_testSubjects)
            {
                if (_testSubjects.Contains(messageInfo.Subject))
                {
                    LogEvent("Test message detected.");
                    _testSubjects.Remove(messageInfo.Subject);

                    if (TestMessageReceived != null)
                    {
                        TestMessageReceived(messageInfo.Subject);
                        LogSuccess();
                    }
                    else
                        LogFail(new Exception("No one was listening for test message event..."));

                    return;
                }
            }

            if (FilterUtility.IsReservedEmailAddress(messageInfo.From.Address))
            {
                LogEvent("Email ignored from " + messageInfo.From.Address);
                return;
            }

            try
            {
                _intakeManager.AddEmail(messageInfo);
            }
            catch (Exception e)
            {
                AlertManager.EmailProcessingFailure(messageInfo.Headers["Date"], e.Message);
                LogFail(e, String.Format("EmailProcessingFailure Reported. ObjectID = {0}", messageInfo.Headers["Message-ID"]));
            }
        }

        protected override void CleanUp()
        {
            if (_client != null)
            {
                _client.NewMessage -= OnNewMessage;
                _client.IdleError -= OnIdleError;
                _client.Dispose();
                _client = null;
            }

            LogEvent("Shutting down...");
            _alreadyListening = false;
        }
    }
}
