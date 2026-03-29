namespace PawsPort.ViewModels
{
    public class BlockListViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool Status { get; set; }
    }
}
