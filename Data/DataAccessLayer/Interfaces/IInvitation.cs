﻿using System;
using System.ComponentModel.DataAnnotations;
using Data.Models;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IInvitation
    {
        [Key]
        int InivitationID { get; set; }

        string Summary { get; set; }
        string Location { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        UserDO CreatedBy { get; set; }
    }
}