using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? LastEditTime { get; set; }

    public string Content { get; set; } = null!;

    public int Status { get; set; }

    public string? Image { get; set; }

    public int ReportedCount { get; set; }

    public DateTime? LastReported { get; set; }

    public bool IsExist { get; set; }

    public int UserId { get; set; }

    public int ArticleId { get; set; }
}
