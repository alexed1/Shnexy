﻿using System;
using System.ComponentModel.DataAnnotations;
using Data.Constants;

namespace Data.Entities.Constants
{
    public class EmailStatusRow : IConstantRow<EmailStatus>
    {
        [Key]
        public int Id { get; set; }
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
