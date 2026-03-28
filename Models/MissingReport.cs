using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class MissingReport
{
    public int ReportId { get; set; }

    public int? PetId { get; set; }

    public DateTime? LastSeenDate { get; set; }

    public bool? IsActive { get; set; }

    public decimal? LastSeenLat { get; set; }

    public decimal? LastSeenLng { get; set; }

    public string? LostLocation { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
