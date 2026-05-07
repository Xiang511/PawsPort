using Microsoft.AspNetCore.Mvc.Rendering;

namespace PawsPort.Dtos
{
    public class MemberPermissionUserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string SystemName { get; set; }
        public string RoleName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int MappingId { get; set; } = 0;

        public int SystemId { get; set; }

        public int RoleId { get; set; }
    }
}
