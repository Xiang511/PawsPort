using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class HealthPassports
{
    public int PassportId { get; set; }

    public int? PetId { get; set; }

    public DateOnly? RecordDate { get; set; }

    public decimal? Weight { get; set; }

    public string? Note { get; set; }

    public int? RecordType { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }
}
