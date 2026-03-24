using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class AdoptionRecords
{
    public int AdoptionId { get; set; }

    public int? PetId { get; set; }

    public int? UserId { get; set; }

    public DateOnly? ApplyDate { get; set; }

    public DateOnly? AdoptDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public string? ReturnReason { get; set; }

    public DateOnly? FollowUpDeadline { get; set; }

    public int? Status { get; set; }
}
