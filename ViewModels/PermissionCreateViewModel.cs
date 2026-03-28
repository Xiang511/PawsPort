using PawsPort.Models;

namespace PawsPort.ViewModels
{
    public class PermissionCreateViewModel
    {
        public List<UserTable> Users { get; set; } = new List<UserTable>();
        public List<SystemTable> Systems { get; set; } = new List<SystemTable>();
        public List<RoleTable> Roles { get; set; } = new List<RoleTable>();
    }
}
