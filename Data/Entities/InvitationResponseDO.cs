﻿using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class InvitationResponseDO : EmailDO, IInvitationResponse
    {
        [ForeignKey("Attendee")]
        public int? AttendeeId { get; set; }
        public virtual AttendeeDO Attendee { get; set; }
    }
}
