﻿using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class NegotiationVM
    {
        public NegotiationVM()
        {
            Questions = new List<NegotiationQuestionVM>();
            Attendees = new List<string>();
        }

        public int? Id { get; set; }
        public int BookingRequestID { get; set; }
        public string Name { get; set; }
        public List<NegotiationQuestionVM> Questions { get; set; }
        public List<String> Attendees { get; set; }
    }

    public class NegotiationQuestionVM
    {
        public NegotiationQuestionVM()
        {
            Answers = new List<NegotiationAnswerVM>();
        }

        public int Id { get; set; }
        public int? CalendarID { get; set; }
        public string Text { get; set; }
        public List<NegotiationAnswerVM> Answers { get; set; }
        public string AnswerType { get; set; }  
    }

    public class NegotiationAnswerVM
    {
        public NegotiationAnswerVM()
        {
            VotedBy = new List<string>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public int? EventID { get; set; }
        public String EventStart { get; set; }
        public String EventEnd { get; set; }

        public DateTimeOffset? EventStartDate { get; set; }
        public DateTimeOffset? EventEndDate { get; set; }
        
        public bool Selected { get; set; }
        public int AnswerState { get; set; }

        public List<String> VotedBy { get; set; } 
    }
}