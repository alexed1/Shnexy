﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Data.Interfaces;

namespace KwasantWeb.ViewModels
{
    public class ClarificationRequestViewModel
    {
        public int Id { get; set; }
        public int BookingRequestId { get; set; }
        public string Recipients { get; set; }
        [Required]
        public string Question { get; set; }
        public int QuestionsCount { get; set; }
    }

    public class NegotiationResponseViewModel
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public List<NegotiationQuestionViewModel> Questions { get; set; }
    }
}