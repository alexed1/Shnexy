﻿using System.ComponentModel.DataAnnotations;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class BookingRequestDO : EmailDO, IBookingRequest
    {
        [Required]
        public virtual CustomerDO Customer { get; set; }
    }
}
