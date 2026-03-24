using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class UserAuthTable
{
    public int AuthId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime? PasswordResetExpires { get; set; }

    public string? PasswordResetToken { get; set; }

    public int UserId { get; set; }
}
