﻿using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface IQuestionDO
    {
        int Id { get; set; }
        int? QuestionStatus { get; set; }
        string Text { get; set; }
        string Response { get; set; }
       
    }
}
