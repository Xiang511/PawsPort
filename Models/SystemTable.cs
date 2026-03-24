using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class SystemTable
{
    public int SystemId { get; set; }

    public string SystemName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
