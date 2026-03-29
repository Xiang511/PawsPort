using PawsPort.Models;

namespace PawsPort.ViewModels
{
    public class PermissionEditViewModel
    {
        // 當前權限資料
        public int MappingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int SystemId { get; set; }
        public int RoleId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // 下拉選單資料
        public List<SystemTable> Systems { get; set; } = new List<SystemTable>();
        public List<RoleTable> Roles { get; set; } = new List<RoleTable>();
    }
}
