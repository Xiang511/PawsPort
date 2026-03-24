using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Notify
{
    public long NotifyId { get; set; }

    public int UserId { get; set; }

    public int SenderId { get; set; }

    public int Type { get; set; }

    public int? TargetId { get; set; }

    public string? Content { get; set; }

    public bool IsRead { get; set; }

    public bool IsExist { get; set; }

    public DateTime CreateAt { get; set; }
}
