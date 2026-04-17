using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using System.Diagnostics;

namespace PawsPort.Models
{
    public partial class PetDbContext : DbContext
    {
        // 無參數建構函式（用於設計時工具，例如 Migrations）
        public PetDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 只有在沒有透過 DI 配置時才執行（例如在設計時工具或測試中）
            if (!optionsBuilder.IsConfigured)
            {
                // 建立配置，優先順序: User Secrets > 環境變數 > appsettings.json
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<PetDbContext>(optional: true)
                    .Build();

                // 檢查是否為本地開發環境
                string? isLocal = config["IS_LOCAL"];
                string? connectionString;

                if (isLocal == "true")
                {
                    // 本地開發環境使用 Windows 驗證
                    connectionString = "Data Source=.;Initial Catalog=PetDB;Integrated Security=True;Encrypt=False";
                    Log.Information("本地開發環境，使用 Windows 驗證連接資料庫");
                }
                else
                {
                    // 生產環境或遠端資料庫 - 從 User Secrets 或環境變數讀取
                    connectionString = config["PETDB"]
                        ?? Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING")
                        ?? throw new InvalidOperationException("找不到資料庫連接字串。請設定 User Secrets、環境變數或 appsettings.json。");

                    Log.Information("生產環境或遠端資料庫，使用指定連接字串（已遮蔽敏感資訊）");
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("找不到資料庫連接字串。請設定 User Secrets、環境變數或 appsettings.json。");
                }

                // 配置 SQL Server 提供者並啟用重試機制
                optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    // 啟用暫時性錯誤重試機制
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    // 設定命令逾時時間（秒）
                    sqlServerOptions.CommandTimeout(60);
                });
            }
        }
    }
}
