using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class QARecord
{
    public int Qaid { get; set; }

    public int UserId { get; set; }

    public DateTime QuestionDate { get; set; }

    public string QuestionType { get; set; } = null!;

    public string ChiefComplaint { get; set; } = null!;

    public string ChatContent { get; set; } = null!;

    public string Csname { get; set; } = null!;

    public string ReplyContent { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime ReplyDate { get; set; }

    public int? Score { get; set; }
}
