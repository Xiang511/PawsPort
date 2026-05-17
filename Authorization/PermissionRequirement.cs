using Microsoft.AspNetCore.Authorization;
using PawsPort.Enums;

namespace PawsPort.Authorization
{
    /// <summary>
    /// 權限需求 - 要求使用者必須擁有特定系統的特定角色
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public SystemEnum System { get; }
        public RoleEnum Role { get; }

        public PermissionRequirement(SystemEnum system, RoleEnum role)
        {
            System = system;
            Role = role;
        }
    }

    /// <summary>
    /// 權限處理器 - 驗證使用者的 Permission Claims
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // 取得使用者的所有 Permission Claims (格式: SystemId:RoleId)
            var userPermissions = context.User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value.Split(':'))
                .Where(parts => parts.Length == 2)
                .Select(parts => new
                {
                    SystemId = int.Parse(parts[0]),
                    RoleId = int.Parse(parts[1])
                })
                .ToList();

            // 檢查是否有符合的權限
            bool hasPermission = userPermissions.Any(p =>
                p.SystemId == (int)requirement.System &&
                p.RoleId == (int)requirement.Role);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
