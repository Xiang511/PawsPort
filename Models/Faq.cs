using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PawsPort.Models;

public partial class Faq
{
    public int Faqid { get; set; }

    [Required(ErrorMessage = "請輸入問題類型")]
    public string QuestionType { get; set; } = null!;

    [Required(ErrorMessage = "請輸入問題")]
    public string Question { get; set; } = null!;

    [Required(ErrorMessage = "請輸入回答")]
    public string Answer { get; set; } = null!;

    public string? Note { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
    public DateTime CreateAt { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
    public DateTime? StoppedDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsExist { get; set; }
}
