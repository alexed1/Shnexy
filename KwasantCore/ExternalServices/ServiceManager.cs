﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace KwasantCore.ExternalServices
{
    public static class ServiceManager
    {
        public static Dictionary<Type, ServiceInformation> ServiceInfo = new Dictionary<Type, ServiceInformation>();

        public static List<String> GetServices()
        {
            lock (ServiceInfo)
                return ServiceInfo.Values.Select(v => v.Key).ToList();
        }

        public static ServiceInformation GetInformationForService(String key)
        {
            lock (ServiceInfo)
                return ServiceInfo.Values.FirstOrDefault(v => v.Key == key);
        }

        public static ServiceInformation GetInformationForService<TServiceType>()
        {
            return GetInformationForService(typeof (TServiceType));
        }
        public static ServiceInformation GetInformationForService(Type serviceType)
        {
            lock (ServiceInfo)
                return ServiceInfo[serviceType];
        }

        public static void RegisterService(Type serviceType, String serviceName, string groupName, object instance = null)
        {
            lock (ServiceInfo)
            {
                ServiceInformation service;
                if (ServiceInfo.ContainsKey(serviceType))
                    service = ServiceInfo[serviceType];
                else
                    ServiceInfo[serviceType] = service = new ServiceInformation();

                service.ServiceName = serviceName;
                service.GroupName = groupName;
                service.Instance = instance;
            }
        }

        public static void RegisterService<T>(String serviceName, String groupName, object instance = null)
        {
            RegisterService(typeof(T), serviceName, groupName, instance);
        }

        public static void SetFlag<T>(String flagName, Object value)
        {
            lock (ServiceInfo)
                ServiceInfo[typeof (T)].SetFlag(flagName, value);
        }

        public static void AddAction<T>(String serverAction, String displayName)
        {
            lock (ServiceInfo)
                ServiceInfo[typeof(T)].AddAction(serverAction, displayName);
        }

        public static void AddTest<T>(String serverAction, String displayName)
        {
            lock (ServiceInfo)
                ServiceInfo[typeof(T)].AddTest(serverAction, displayName);
        }

        public static void LogEvent<T>(String eventName)
        {
            lock (ServiceInfo)
                ServiceInfo[typeof(T)].AddEvent(eventName);
        }

        public static void StartingTest<T>()
        {
            lock (ServiceInfo)
                ServiceInfo[typeof(T)].StartingTest();
        }

        public static void FinishedTest<T>()
        {
            lock (ServiceInfo)
                ServiceInfo[typeof(T)].FinishedTest();
        }

        public static void LogSuccess<T>()
        {
            lock (ServiceInfo)
            {
                ServiceInfo[typeof(T)].AddAttempt();
                ServiceInfo[typeof (T)].AddSuccess();
            }
        }

        public static void LogFail<T>()
        {
            lock (ServiceInfo)
            {
                ServiceInfo[typeof(T)].AddAttempt();
                ServiceInfo[typeof (T)].AddFail();
            }
        }

        public class ServiceInformation
        {
            private readonly String _key = Guid.NewGuid().ToString();
            public String Key
            {
                get
                {
                    return _key;
                }
            }

            private Object _instance;
            public Object Instance
            {
                get
                {
                    lock (ServiceInfo)
                        return _instance;
                }
                set
                {
                    lock (ServiceInfo)
                        _instance = value;
                }
            }

            private String _serviceName;
            public String ServiceName
            {
                get
                {
                    lock (ServiceInfo)
                        return _serviceName;
                }
                set
                {
                    lock (ServiceInfo)
                        _serviceName = value;
                }
            }

            private String _groupName;
            public String GroupName
            {
                get
                {
                    lock (ServiceInfo)
                        return _groupName;
                }
                set
                {
                    lock (ServiceInfo)
                        _groupName = value;
                }
            }

            private DateTime? _lastFail;
            public DateTime? LastFail
            {
                get
                {
                    lock (ServiceInfo)
                        return _lastFail;
                }
            }

            private DateTime? _lastSuccess;
            public DateTime? LastSuccess
            {
                get
                {
                    lock (ServiceInfo)
                        return _lastSuccess;
                }
            }

            private readonly Dictionary<String, Object> _flags = new Dictionary<string, object>();
            public Dictionary<String, Object> Flags
            {
                get
                {
                    lock (ServiceInfo)
                        return new Dictionary<string, object>(_flags);
                }
            }

            private readonly Dictionary<String, String> _actions = new Dictionary<String, String>();
            public Dictionary<String, String> Actions
            {
                get
                {
                    lock (ServiceInfo)
                        return new Dictionary<String, String>(_actions);
                }
            }

            private readonly Dictionary<String, String> _tests = new Dictionary<String, String>();
            public Dictionary<String, String> Tests
            {
                get
                {
                    lock (ServiceInfo)
                        return new Dictionary<String, String>(_tests);
                }
            } 

            private readonly List<Tuple<DateTime, String>> _events = new List<Tuple<DateTime, String>>();
            public List<Tuple<DateTime, String>> Events
            {
                get
                {
                    lock (ServiceInfo)
                        return new List<Tuple<DateTime, String>>(_events);
                }
            }

            private int _attempts;
            public int Attempts
            {
                get
                {
                    lock (ServiceInfo)
                        return _attempts;
                }
                set
                {
                    lock (ServiceInfo)
                        _attempts = value;
                }
            }

            private bool _runningTest;
            public bool RunningTest
            {
                get
                {
                    lock (ServiceInfo)
                        return _runningTest;
                }
                set
                {
                    lock (ServiceInfo)
                        _runningTest = value;
                }
            }

            private int _success;
            public int Success
            {
                get
                {
                    lock (ServiceInfo)
                        return _success;
                }
                set
                {
                    lock (ServiceInfo)
                        _success = value;
                }
            }

            private int _fail;
            public int Fail
            {
                get
                {
                    lock (ServiceInfo)
                        return _fail;
                }
                set
                {
                    lock (ServiceInfo)
                        _fail = value;
                }
            }
            
            public void AddAttempt()
            {
                lock (ServiceInfo)
                    Attempts++;
            }

            public void AddSuccess()
            {
                lock (ServiceInfo)
                {
                    _lastSuccess = DateTime.Now;
                    Success++;
                }
            }

            public void AddFail()
            {
                lock (ServiceInfo)
                {
                    _lastFail = DateTime.Now;
                    Fail++;
                }
            }

            public void StartingTest()
            {
                lock (ServiceInfo)
                    RunningTest = true;
            }

            public void FinishedTest()
            {
                lock (ServiceInfo)
                    RunningTest = false;
            }

            public void AddEvent(String eventName)
            {
                lock (ServiceInfo)
                    _events.Add(new Tuple<DateTime, string>(DateTime.Now, eventName));
            }

            public void SetFlag(String flagName, Object flagValue)
            {
                lock (ServiceInfo)
                    _flags[flagName] = flagValue;
            }

            public void AddAction(String serverCall, String displayName)
            {
                lock (ServiceInfo)
                    _actions[serverCall] = displayName;
            }

            public void AddTest(String serverCall, String displayName)
            {
                lock (ServiceInfo)
                    _tests[serverCall] = displayName;
            }
        }
    }

    public class ServiceManager<T>
    {
        public ServiceManager(String serviceName, String groupName, object instance = null)
        {
            ServiceManager.RegisterService<T>(serviceName, groupName, instance);
        }

        public void SetFlag(String flagName, Object value)
        {
            ServiceManager.SetFlag<T>(flagName, value);
        }

        public void AddAction(String serverAction, String displayName)
        {
            ServiceManager.AddAction<T>(serverAction, displayName);
        }

        public void AddTest(String serverAction, String displayName)
        {
            ServiceManager.AddTest<T>(serverAction, displayName);
        }
        
        public void LogEvent(String eventName)
        {
            ServiceManager.LogEvent<T>(eventName);
        }
        
        public void LogSucessful(string eventName = null)
        {
            if (!String.IsNullOrEmpty(eventName))
                LogEvent(eventName);
            ServiceManager.LogSuccess<T>();
        }

        public void LogFail(Exception ex, string eventName = null)
        {
            var currException = ex;
            var exceptionMessages = new List<String>();
            while (currException != null)
            {
                exceptionMessages.Add(currException.Message);
                currException = currException.InnerException;
            }

            exceptionMessages.Add("*** Stacktrace ***");
            exceptionMessages.Add(ex.StackTrace);

            var exceptionMessage = String.Join(Environment.NewLine, exceptionMessages);

            if (eventName == null)
                eventName = exceptionMessage;
            else
                eventName += " " + exceptionMessage;
            
            LogEvent(eventName);
            ServiceManager.LogFail<T>();
        }
    }
}
