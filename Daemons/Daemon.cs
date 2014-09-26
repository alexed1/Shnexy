﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Daemons.EventExposers;
using KwasantCore.ExternalServices;

namespace Daemons
{
    //For more information, see https://maginot.atlassian.net/wiki/display/SH/Design+Document%3A+SH-21

    public abstract class Daemon
    {
        public abstract bool Start();
        public abstract void Stop();
    }
    public abstract class Daemon<T> : Daemon
        where T : Daemon<T>
    {
        public delegate void DaemonExecutedEventHandler();
        public event DaemonExecutedEventHandler DaemonExecuted;

        private Thread m_RunningThread;

        public abstract int WaitTimeBetweenExecution { get; }

        public bool IsRunning { get; protected set; }
        public bool IsStopping { get; private set; }

        protected abstract void Run();
        private readonly Queue<Action> _eventQueue = new Queue<Action>();
        private readonly HashSet<EventInfo> _activeEventHandlers = new HashSet<EventInfo>();
        private readonly HashSet<Exception> _loggedExceptions = new HashSet<Exception>();

        private readonly ServiceManager<T> _serviceManager;

        protected Daemon()
        {
            _serviceManager = new ServiceManager<T>(GetType().Name, "Daemons", this);
        }

        protected void SetFlag(String flagName, Object value)
        {
            _serviceManager.SetFlag(flagName, value);
        }
        protected void LogEvent(String message)
        {
            _serviceManager.LogEvent(message);
        }
        protected void LogAttempt(String message = null)
        {
            _serviceManager.LogAttempt(message);
        }
        protected void LogFail(Exception ex, String message = null)
        {
            _serviceManager.LogFail(ex, message);
        }
        protected void LogSuccess(String message = null)
        {
            _serviceManager.LogSucessful(message);
        }

        /// <summary>
        /// Currently unused, but will be a useful debugging tool when investigating event callbacks.
        /// </summary>
        public IList<Exception> LoggedExceptions
        {
            get
            {
                lock(_loggedExceptions)
                    return new List<Exception>(_loggedExceptions);
            }
        }

        /// <summary>
        /// Currently unused, but will be a useful debugging tool when investigating event callbacks.
        /// </summary>
        protected IList<EventInfo> ActiveEventHandlers
        {
            get { return new List<EventInfo>(_activeEventHandlers); }
        }

        /// <summary>
        /// Registers an event. Event callbacks will be marshalled into a thread-safe queue. The event will _not_ be dispatched automatically.
        /// To process an event, call <see cref="ProcessNextEvent">ProcessNextEvent</see> or <see cref="ProcessNextEventNoWait">ProcessNextEventNoWait</see>
        /// </summary>
        /// <param name="eventInfo">The EventInfo of your desired event. You can pass this manually, or use <see cref="ExposedEvent">ExposedEvent</see></param>
        /// <param name="callback">The delegate to be invoked when the queue is processed</param>
        protected void RegisterEvent<TArg1>(EventInfo eventInfo, Action<TArg1> callback)
        {
            Action<TArg1> action = arg1 =>
            {
                lock (_eventQueue)
                {
                    _eventQueue.Enqueue(() => callback(arg1));
                    Monitor.Pulse(_eventQueue);
                }
            };

            Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, action.Target, action.Method);
            eventInfo.AddEventHandler(this, handler);

            _activeEventHandlers.Add(eventInfo);
        }
        protected void RegisterEvent<TArg1, TArg2>(EventInfo eventInfo, Action<TArg1, TArg2> callback)
        {
            Action<TArg1, TArg2> action = (arg1, arg2) =>
            {
                lock (_eventQueue)
                {
                    _eventQueue.Enqueue(() => callback(arg1, arg2));
                    Monitor.Pulse(_eventQueue);
                }
            };

            Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, action.Target, action.Method);
            eventInfo.AddEventHandler(this, handler);

            _activeEventHandlers.Add(eventInfo);
        }

        protected void RegisterEvent<TArg1, TArg2, TArg3>(EventInfo eventInfo, Action<TArg1, TArg2, TArg3> callback)
        {
            Action<TArg1, TArg2, TArg3> action = (arg1, arg2, arg3) =>
            {
                lock (_eventQueue)
                {
                    _eventQueue.Enqueue(() => callback(arg1, arg2, arg3));
                    Monitor.Pulse(_eventQueue);
                }
            };

            Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, action.Target, action.Method);
            eventInfo.AddEventHandler(this, handler);

            _activeEventHandlers.Add(eventInfo);
        }

        protected void RegisterEvent<TArg1, TArg2, TArg3, TArg4>(EventInfo eventInfo, Action<TArg1, TArg2, TArg3, TArg4> callback)
        {
            Action<TArg1, TArg2, TArg3, TArg4> action = (arg1, arg2, arg3, arg4) =>
            {
                lock (_eventQueue)
                {
                    _eventQueue.Enqueue(() => callback(arg1, arg2, arg3, arg4));
                    Monitor.Pulse(_eventQueue);
                }
            };

            Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, action.Target, action.Method);
            eventInfo.AddEventHandler(this, handler);

            _activeEventHandlers.Add(eventInfo);
        }

        /// <summary>
        /// Fires off the next event to the listening delegate, and will block until an event is received if no events are pending processing
        /// </summary>
        protected void ProcessNextEvent()
        {
            lock (_eventQueue)
            {
                while (_eventQueue.Count == 0)
                {
                    Monitor.Wait(_eventQueue);
                }
                _eventQueue.Dequeue()();
            }
        }

        /// <summary>
        /// Fires off the next event to the listening delegate. If no events are pending, it will not block. Returns true if an event was processed, false if not.
        /// </summary>
        /// <returns></returns>
        protected bool ProcessNextEventNoWait()
        {
            lock (_eventQueue)
            {
                if (_eventQueue.Count > 0)
                {
                    _eventQueue.Dequeue()();
                    return true;
                }
                return false;
            }
        }

        public override bool Start()
        {
            lock (this)
            {
                if (IsRunning && !IsStopping)
                {
                    LogEvent("Already running.");
                    return false;
                }

                IsRunning = true;
            }
            SetFlag("State", "Running");
            LogEvent("Starting...");

            IsStopping = false;

            if (WaitTimeBetweenExecution == -1)
            {
                m_RunningThread = new Thread(Run);
            }
            else
            {
                m_RunningThread = new Thread(() =>
                {
                    bool firstExecution = true;
                    DateTime lastExecutionTime = DateTime.Now;

                    while (!IsStopping)
                    {
                        try
                        {
                            DateTime currTime = DateTime.Now;
                            if (firstExecution ||
                                (currTime - lastExecutionTime).TotalMilliseconds > WaitTimeBetweenExecution)
                            {
                                lastExecutionTime = currTime;
                                firstExecution = false;
                                LogAttempt("Running...");
                                Run();
                                LogSuccess("Completed.");
                                if (DaemonExecuted != null)
                                    DaemonExecuted();
                            }
                            else
                            {
                                //Sleep until the approximate time that we're ready
                                double waitTime = (WaitTimeBetweenExecution - (currTime - lastExecutionTime).TotalMilliseconds);

                                Thread.Sleep((int) waitTime);
                            }

                        }
                        catch (Exception e)
                        {
                            HandleException(e);
                        }
                    }
                    LogEvent("Stopped.");

                    CleanupInternal();
                    IsRunning = false;
                    SetFlag("State", "Stopped");
                });
            }

            m_RunningThread.Start();
            return true;
        }

        public override void Stop()
        {
            SetFlag("State", "Stopping...");
            LogEvent("Stopping...");
            IsStopping = true;
            StopInternal();
        }

        protected virtual void StopInternal() { }

        private void CleanupInternal()
        {
            CleanUp();
        }

        /// <summary>
        /// Allows the daemon to cleanup any resources. Called when the daemon is shutting down
        /// </summary>
        protected virtual void CleanUp()
        {
            
        }

        private void HandleException(Exception e)
        {
            lock (_loggedExceptions)
                _loggedExceptions.Add(e);

            LogFail(e, "Error occured in " + GetType().Name);
        }
    }
}
