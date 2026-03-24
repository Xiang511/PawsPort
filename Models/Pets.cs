using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Pets
{
    public int PetId { get; set; }

    public int? SpeciesId { get; set; }

    public string? Name { get; set; }

    public int? Gender { get; set; }

    public int? Size { get; set; }

    public string? CoatColor { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Photo { get; set; }

    public int? CurrentStatus { get; set; }

    public string? BehavioralTraits { get; set; }

    public bool? IsHighMaintenance { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDesex { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
