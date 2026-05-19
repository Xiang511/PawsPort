namespace PawsPort.Dtos
{
    public class GameResultSubmitDTO
    {
        
            public int PlayerId { get; set; }
            public int GameId { get; set; }
            public bool IsVictory { get; set; }
            public int BonusPoints { get; set; }

    }
}
