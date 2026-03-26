using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class OauthTable
{
    public int OauthId { get; set; }

    public string AuthType { get; set; } = null!;

    public string ProviderKey { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public string? AccessToken { get; set; }

    public int UserId { get; set; }
}
