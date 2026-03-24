using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class ArticleImage
{
    public int ImageId { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? LastEditTime { get; set; }

    public string Image { get; set; } = null!;

    public int SortOrder { get; set; }

    public int ArticleId { get; set; }

    public bool IsExist { get; set; }
}
