using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Messages
{
    public long MessageId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreateAt { get; set; }

    public string? Image { get; set; }

    public bool IsExist { get; set; }

    public int SenderId { get; set; }

    public int ChatroomId { get; set; }
}
