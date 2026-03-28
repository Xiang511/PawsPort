using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class UserSystemRole
{
    public int MappingId { get; set; }

    public int UserId { get; set; }

    public int SystemId { get; set; }

    public int RoleId { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
