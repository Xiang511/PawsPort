using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawsPort.Models;

public class GameContent
{
    public int GameId { get; set; }
    [DisplayName("題目類型")]
    public string GameName { get; set; } = null!;
    [DisplayName("題目內容")]
    public string Questions { get; set; } = null!;
    [DisplayName("詳細解答")]
    public string AnswersDetail { get; set; } = null!;
    [DisplayName("Y/N")]
    public bool Answers { get; set; }
    [DisplayName("啟用")]
    public bool IsActive { get; set; }
    [DisplayName("獎勵")]
    public int Rewards { get; set; }
    [DisplayName("形式")]
    public string? Type { get; set; }
}
