﻿using System;
using System.Transactions;
using Data.Interfaces;
using Data.Repositories;

namespace Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope _transaction;
        private readonly IDBContext _context;

        public UnitOfWork(IDBContext context)
        {
            context.UnitOfWork = this;
            _context = context;
        }

        private AttachmentRepository _attachmentRepository;

        public AttachmentRepository AttachmentRepository
        {
            get
            {
                return _attachmentRepository ?? (_attachmentRepository = new AttachmentRepository(_context));
            }
        }

        private AttendeeRepository _attendeeRepository;

        public AttendeeRepository AttendeeRepository
        {
            get
            {
                return _attendeeRepository ?? (_attendeeRepository = new AttendeeRepository(_context));
            }
        }

        private EmailAddressRepository _emailAddressRepository;

        public EmailAddressRepository EmailAddressRepository
        {
            get
            {
                return _emailAddressRepository ?? (_emailAddressRepository = new EmailAddressRepository(_context));
            }
        }

        private EmailEmailAddressRepository _emailEmailAddressRepository;

        public EmailEmailAddressRepository EmailEmailAddressRepository
        {
            get
            {
                return _emailEmailAddressRepository ?? (_emailEmailAddressRepository = new EmailEmailAddressRepository(_context));
            }
        }

        private BookingRequestRepository _bookingRequestRepository;

        public BookingRequestRepository BookingRequestRepository
        {
            get
            {
                return _bookingRequestRepository ?? (_bookingRequestRepository = new BookingRequestRepository(_context));
            }
        }

        private CalendarRepository _calendarRepository;

        public CalendarRepository CalendarRepository
        {
            get
            {
                return _calendarRepository ?? (_calendarRepository = new CalendarRepository(_context));
            }
        }

        private CommunicationConfigurationRepository _communicationConfigurationRepository;

        public CommunicationConfigurationRepository CommunicationConfigurationRepository
        {
            get
            {
                return _communicationConfigurationRepository ??
                       (_communicationConfigurationRepository = new CommunicationConfigurationRepository(_context));
            }
        }

        private EmailRepository _emailRepository;

        public EmailRepository EmailRepository
        {
            get
            {
                return _emailRepository ?? (_emailRepository = new EmailRepository(_context));
            }
        }

        private EventRepository _eventRepository;

        public EventRepository EventRepository
        {
            get
            {
                return _eventRepository ?? (_eventRepository = new EventRepository(_context));
            }
        }

        private InstructionRepository _instructionRepository;

        public InstructionRepository InstructionRepository
        {
            get
            {
                return _instructionRepository ?? (_instructionRepository = new InstructionRepository(_context));
            }
        }

        private PersonRepository _personRepository;

        public PersonRepository PersonRepository
        {
            get
            {
                return _personRepository ?? (_personRepository = new PersonRepository(_context));
            }
        }

        private StoredFileRepository _storedFileRepository;

        public StoredFileRepository StoredFileRepository
        {
            get
            {
                return _storedFileRepository ?? (_storedFileRepository = new StoredFileRepository(_context));
            }
        }

        private TrackingStatusRepository _trackingStatusRepository;

        public TrackingStatusRepository TrackingStatusRepository
        {
            get
            {
                return _trackingStatusRepository ?? (_trackingStatusRepository = new TrackingStatusRepository(_context));
            }
        }

        private UserRepository _userRepository;

        public UserRepository UserRepository
        {
            get
            {
                return _userRepository ?? (_userRepository = new UserRepository(_context));
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_transaction != null)
                _transaction.Dispose();
        }

        public void StartTransaction()
        {
            _transaction = new TransactionScope();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            _context.SaveChanges();
            _transaction.Complete();
            _transaction.Dispose();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public IDBContext Db
        {
            get { return _context; }
        }
    }
}
