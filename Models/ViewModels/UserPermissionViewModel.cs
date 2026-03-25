namespace PawsPort.Models.ViewModels
{
    public class UserPermissionViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string SystemName { get; set; }
        public string RoleName { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}