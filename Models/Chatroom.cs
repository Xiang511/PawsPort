using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Chatroom
{
    public int ChatroomId { get; set; }

    public int UserId1 { get; set; }

    public int UserId2 { get; set; }

    public bool IsExist { get; set; }

    public DateTime CreateAt { get; set; }
}
