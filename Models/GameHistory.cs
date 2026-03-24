using System;
using System.Collections.Generic;

namespace PawsPort.Models;

public partial class GameHistory
{
    public int HistoryId { get; set; }

    public int GameId { get; set; }

    public bool? StageClear { get; set; }

    public DateTime? LastPlayedDate { get; set; }

    public int PlayerId { get; set; }
}
