using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Faq
{
    public int Faqid { get; set; }

    public string QuestionType { get; set; } = null!;

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? StoppedDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsExist { get; set; }
}
