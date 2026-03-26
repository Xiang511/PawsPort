using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class LineBot
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime ChatDate { get; set; }

    public string QuestionType { get; set; } = null!;

    public string ChiefComplaint { get; set; } = null!;

    public string ChatContent { get; set; } = null!;
}
