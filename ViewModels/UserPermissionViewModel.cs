using Microsoft.AspNetCore.Mvc.Rendering;

namespace PawsPort.ViewModels
{
    public class UserPermissionViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string SystemName { get; set; }
        public string RoleName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public IEnumerable<SelectListItem> UserOptions { get; set; }

        public int MappingId { get; set; }

        public int SystemId { get; set; }

        public int RoleId { get; set; }


    }
}