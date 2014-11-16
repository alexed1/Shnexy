﻿using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Services
{
    public class Report
    {
        public object Generate(IUnitOfWork uow, DateRange dateRange, string type)
        {
            switch (type)
            {
                case "alllogs":
                    return ShowAllLogs(uow, dateRange);
                case "usage":
                    return GenerateUsageReport(uow, dateRange);
                case "incident":
                    return ShowAllIncidents(uow, dateRange);
                case "fiveRecentIncident":
                    return ShowMostRecent5Incidents(uow, dateRange);
            }
            return this;
        }

        private List<object> ShowAllLogs(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.LogRepository.GetAll()
                .Where(e => e.Date > dateRange.StartTime && e.Date < dateRange.EndTime)
                .OrderByDescending(e => e.Date)
                .Select(l => (object)new
                {
                    Date = l.Date.ToString("yyyy MMMM dd HH:mm:ss"),
                    l.Name,
                    l.Level,
                    l.Message
                }).ToList();
        }

        private object GenerateUsageReport(IUnitOfWork uow, DateRange dateRange)
        {
            return
                uow.FactRepository.GetAll()
                    .Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime)
                    .Select(
                        f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Status = f.Status,
                            Data = f.Data,
                            CreateDate = f.CreateDate.ToString("M-d-yy hh:mm tt")
                        }).ToList();
        }

        private object ShowAllIncidents(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.IncidentRepository.GetAll().Where(e => e.CreateTime > dateRange.StartTime && e.CreateTime < dateRange.EndTime).Select(
                        f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Data = f.Notes,
                            CreateDate = f.CreateTime.ToString("M-d-yy hh:mm tt")
                        }).ToList();
        }

        private object ShowMostRecent5Incidents(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.IncidentRepository.GetAll().OrderByDescending(x => x.CreateTime).Take(5).Select(
                        f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Data = f.Notes,
                            CreateDate = f.CreateTime.ToString("M-d-yy hh:mm tt")
                        }).ToList();
        }
        
        public object GenerateHistoryReport(IUnitOfWork uow, DateRange dateRange, string primaryCategory, string bookingRequestId)
        {
            return uow.FactRepository.GetAll()
                .Where(e => e.PrimaryCategory == primaryCategory && (e.ObjectId == bookingRequestId) && e.CreateDate >= dateRange.StartTime && e.CreateDate <= dateRange.EndTime).OrderByDescending(e => e.CreateDate)
               .Select(
                        e =>
                            new
                            {
                                PrimaryCategory = e.PrimaryCategory,
                                SecondaryCategory = e.SecondaryCategory,
                                Activity = e.Activity,
                                Status = e.Status,
                                Data = e.Data,
                                CreateDate = e.CreateDate.ToString("M-d-yy hh:mm tt")
                            })
                    .ToList();
        }

        public object GenerateHistoryByBookingRequestId(IUnitOfWork uow, int bookingRequestId)
        {
            return uow.FactRepository.GetAll().Where(e => e.ObjectId == bookingRequestId.ToString())
                    .OrderByDescending(e => e.CreateDate)
                    .Select(
                        e =>
                            new
                            {
                                PrimaryCategory=e.PrimaryCategory,
                                Activity = e.Activity,
                                Status=e.Status,
                                Data=e.Data,
                                CreateDate = e.CreateDate.ToString("M-d-yy hh:mm tt")
                            })
                    .ToList();
        }
    }
}
