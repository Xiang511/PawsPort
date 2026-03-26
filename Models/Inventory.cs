using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int PlayerId { get; set; }

    public int SkinId { get; set; }

    public bool Enable { get; set; }

    public DateTime? CreateTime { get; set; }
}
