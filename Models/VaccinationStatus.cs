using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class VaccinationStatus
{
    public int HistoryId { get; set; }

    public string? Type { get; set; }

    public string? Location { get; set; }

    public DateTime? Time { get; set; }

    public DateOnly? Forecast { get; set; }

    public int? PassportId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }
}
