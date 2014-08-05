﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces;
using Data.States.Templates;

namespace KwasantWeb.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public _QuestionStatusTemplate Status { get; set; }
        public int NegotiationId { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public string AnswerType { get; set; }
    }
}