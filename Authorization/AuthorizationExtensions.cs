using Microsoft.AspNetCore.Authorization;
using PawsPort.Enums;

namespace PawsPort.Authorization
{
    /// <summary>
    /// 授權服務擴展方法
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// 配置 PawsPort 授權策略
        /// </summary>
        public static IServiceCollection AddPawsPortAuthorization(this IServiceCollection services)
        {
            // 註冊授權處理器
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            // 設定授權策略
            services.AddAuthorization(options =>
            {
                // ========== 會員系統 (SystemId = 1) ==========
                options.AddPolicy("會員系統_系統管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.會員系統, RoleEnum.系統管理員)));

                options.AddPolicy("會員系統_普通管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.會員系統, RoleEnum.普通管理員)));

                options.AddPolicy("會員系統_一般成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.會員系統, RoleEnum.一般成員)));

                options.AddPolicy("會員系統_客服成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.會員系統, RoleEnum.客服成員)));

                // ========== 寵物系統 (SystemId = 2) ==========
                options.AddPolicy("寵物系統_系統管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.寵物系統, RoleEnum.系統管理員)));

                options.AddPolicy("寵物系統_普通管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.寵物系統, RoleEnum.普通管理員)));

                options.AddPolicy("寵物系統_一般成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.寵物系統, RoleEnum.一般成員)));

                options.AddPolicy("寵物系統_客服成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.寵物系統, RoleEnum.客服成員)));

                // ========== 遊戲系統 (SystemId = 3) ==========
                options.AddPolicy("遊戲系統_系統管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.遊戲系統, RoleEnum.系統管理員)));

                options.AddPolicy("遊戲系統_普通管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.遊戲系統, RoleEnum.普通管理員)));

                options.AddPolicy("遊戲系統_一般成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.遊戲系統, RoleEnum.一般成員)));

                options.AddPolicy("遊戲系統_客服成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.遊戲系統, RoleEnum.客服成員)));

                // ========== 客服系統 (SystemId = 4) ==========
                options.AddPolicy("客服系統_系統管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.客服系統, RoleEnum.系統管理員)));

                options.AddPolicy("客服系統_普通管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.客服系統, RoleEnum.普通管理員)));

                options.AddPolicy("客服系統_一般成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.客服系統, RoleEnum.一般成員)));

                options.AddPolicy("客服系統_客服成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.客服系統, RoleEnum.客服成員)));

                // ========== 社群系統 (SystemId = 5) ==========
                options.AddPolicy("社群系統_系統管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.社群系統, RoleEnum.系統管理員)));

                options.AddPolicy("社群系統_普通管理員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.社群系統, RoleEnum.普通管理員)));

                options.AddPolicy("社群系統_一般成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.社群系統, RoleEnum.一般成員)));

                options.AddPolicy("社群系統_客服成員", policy =>
                    policy.Requirements.Add(new PermissionRequirement(SystemEnum.社群系統, RoleEnum.客服成員)));
            });

            return services;
        }
    }
}
