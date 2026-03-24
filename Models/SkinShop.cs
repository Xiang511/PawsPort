using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class SkinShop
{
    public int SkinId { get; set; }

    public string SkinName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Price { get; set; }

    public string SkinImage { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public bool? IsDel { get; set; }
}
