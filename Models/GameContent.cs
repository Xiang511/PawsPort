using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class GameContent
{
    public int GameId { get; set; }

    public string GameName { get; set; } = null!;

    public string Questions { get; set; } = null!;

    public string AnswersDetail { get; set; } = null!;

    public bool Answers { get; set; }

    public bool IsActive { get; set; }

    public int Rewards { get; set; }

    public string? Type { get; set; }
}
