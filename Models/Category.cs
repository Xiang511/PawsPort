using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? CategoryDescription { get; set; }

    public int? ParentId { get; set; }

    public int Level { get; set; }

    public int SortOrder { get; set; }

    public bool IsExist { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? LastEditTime { get; set; }
}
