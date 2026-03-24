using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class LoginActivityTable
{
    public int LogId { get; set; }

    public DateTime? LoginTime { get; set; }

    public string? Ipaddress { get; set; }

    public string? DeviceInfo { get; set; }

    public string? AuthType { get; set; }

    public bool? Status { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int? UserId { get; set; }
}
