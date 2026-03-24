using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class ENewsletter
{
    public int NewsLetterId { get; set; }

    public string Title { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? PublishDate { get; set; }

    public string? Category { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public int UserId { get; set; }
}
