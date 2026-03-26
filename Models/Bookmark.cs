using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Bookmark
{
    public int BookmarkId { get; set; }

    public DateTime CreateAt { get; set; }

    public int UserId { get; set; }

    public int ArticleId { get; set; }

    public bool IsExist { get; set; }
}
