﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Utilities;

namespace Data.Entities
{
    public class IncidentDO : HistoryItemDO
    {
        public IncidentDO()
        {
            Priority = 1;
            //Notes = "No additional notes";
        }

        public int Priority { get; set; }
    
        [NotMapped]
        public bool IsHighPriority { get { return Priority >= 5; } }

        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues, IUnitOfWork uow)
        {
            base.OnModify(originalValues, currentValues, uow);

            var reflectionHelper = new ReflectionHelper<IncidentDO>();
            var priorityPropertyName = reflectionHelper.GetPropertyName(i => i.Priority);
            if (!MiscUtils.AreEqual(originalValues[priorityPropertyName], currentValues[priorityPropertyName])
                && IsHighPriority)
            {
                AlertManager.HighPriorityIncidentCreated(Id);
            }
        }

        public override void AfterCreate()
        {
            base.AfterCreate();

            if (IsHighPriority)
            {
                AlertManager.HighPriorityIncidentCreated(Id);
            }
        }
    }
}
